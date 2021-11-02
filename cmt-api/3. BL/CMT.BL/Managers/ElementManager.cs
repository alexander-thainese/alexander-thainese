using AutoMapper;
using CMT.BL.Core;
using CMT.BO;
using CMT.BO.Admin;
using CMT.Common;
using CMT.DL;
using CMT.DL.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Transactions;

namespace CMT.BL
{
    public class ElementManager : BaseObjectManager<CmtEntities, MetadataElement, ElementBO>
    {
        private ElementTypeManager ElementTypeManager { get; }
        private ValueListManager ValueListManager { get; }
        private ValueListLevelManager ValueListLevelManager { get; }
        public ElementManager(CmtEntities dbContext, ElementTypeManager elementTypeManager, ValueListManager valueListManager, ValueListLevelManager valueListLevelManager)
            : base(dbContext)
        {
            SetMapper();
            ElementTypeManager = elementTypeManager;
            ValueListManager = valueListManager;
            ValueListLevelManager = valueListLevelManager;
        }


        public ExtElementBO GetElementDetails(Guid id)
        {

            try
            {
                SetMapper();

                var query = (from o in DbQueryable
                             where o.ObjectId == id
                             select new
                             {
                                 Element = o,
                                 Levels = o.ValueListLevels
                             }).Single();

                ExtElementBO result = ConvertToBusinessObject<ExtElementBO, MetadataElement>(query.Element);
                result.LOVLevels = query.Levels.Any() ? query.Levels.OrderBy(o => o.Level).Select(o => o.Name).ToList() : null;

                return result;
            }
            catch (Exception e)
            {
                throw e;

            }
            finally
            {

                InitializeMapper();
            }
        }

        protected override MetadataElement GetDbObject(Guid objectId)
        {
            return DbQueryable.Include(p => p.ElementType).Single(p => p.ObjectId.Equals(objectId));
        }

        [DataObjectMethod(DataObjectMethodType.Select, true)]
        public override List<ElementBO> GetObjects()
        {
            List<ElementBO> list = ConvertToBusinessObjects(DbQueryable.Include(p => p.ElementType)).ToList();

            return list;
        }


        public void UpdateLovLabels(ExtElementBO element)
        {
            using (TransactionScope transactionScope = TransactionScopeBuilder.CreateScope())
            {
                Guid objectId = element.ObjectId;

                List<ValueListLevelBO> valueListLevels = ValueListLevelManager.GetObjectsUsingBOPredicate(o => o.ElementId == objectId).OrderBy(o => o.Level).ToList();
                int index = 0;

                foreach (ValueListLevelBO valueListLevel in valueListLevels)
                {
                    if (!string.Equals(valueListLevel.Name, element.LOVLevels[index]))
                    {
                        valueListLevel.Name = element.LOVLevels[index];
                        ValueListLevelManager.UpdateObject(valueListLevel);
                    }

                    index++;
                }

                transactionScope.Complete();
            }
        }

        public void UpdateMetadataElementDescription(ElementBO element)
        {



            try
            {
                SetMapper();

                using (TransactionScope transactionScope = TransactionScopeBuilder.CreateScope())
                {
                    ElementBO bo = GetObject(element.ObjectId);
                    bo.Description = element.Description;
                    UpdateObject(bo);

                    transactionScope.Complete();
                }
            }
            catch (Exception e)
            {
                throw e;

            }
            finally
            {

                InitializeMapper();
            }

        }


        private void SetMapper()
        {
            MapperConfiguration config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<MetadataElement, ElementBO>()
                .ForMember(p => p.ValueList, opt => opt.Ignore())
                .ForMember(p => p.ElementTypeName, opt => opt.Ignore())
                .ForMember(p => p.IsLov, opt => opt.MapFrom(p => (p != null && p.ElementType != null) && p.ElementType.Type == ElementTypeType.Lov));
                cfg.CreateMap<ElementBO, MetadataElement>()
                .ForMember(p => p.ValueList, opt => opt.Ignore());
                cfg.CreateMap<MetadataElement, ExtElementBO>()
                .ForMember(p => p.ValueList, opt => opt.Ignore())
                .ForMember(p => p.ElementTypeName, opt => opt.Ignore());
                cfg.CreateMap<ExtElementBO, MetadataElement>()
                .ForMember(p => p.ValueList, opt => opt.Ignore());
            });
            Mapper = config.CreateMapper();

        }

        public Guid? AddElement(ExtElementBO element)
        {
            try
            {
                SetMapper();

                if (element.SourceElementId.HasValue)
                {
                    return CreateDerivedElement(element);
                }

                string name = element.Name;
                VerifyElementName(name);

                if (element.TypeId == Guid.Empty)
                {
                    throw new UserFriendlyException("Please select element type", HttpStatusCode.NotAcceptable);
                }

                CreateElementValueList(element);

                InsertObject(element);

                AddAllThirdPartySystems(element.ObjectId, false);

                CreateValueListLevels(element);

                return element.ObjectId;

            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                InitializeMapper();
            }
        }

        public void CreateElementValueList(ElementBO element)
        {
            ValueList valueListDbObj = new ValueList()
            {
                Name = Guid.NewGuid().ToString()
            };

            DbContext.ValueLists.Add(valueListDbObj);
            DbContext.SaveChanges();

            element.Status = 1;
            element.ValueListId = valueListDbObj.ObjectId;
        }

        private void VerifyElementName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new UserFriendlyException("Element name cannot be empty", HttpStatusCode.NotAcceptable);
            }

            if (DbContext.MetadataElements.Any((p) => p.Name == name))
            {
                throw new UserFriendlyException("Element with this name already exists", HttpStatusCode.NotAcceptable);
            }
        }

        public Dictionary<string, Stream> GetElementExtracts(DateTime now, Guid? schemaId = null, Guid? countryId = null)
        {
            Dictionary<string, Stream> result = new Dictionary<string, Stream>();


            var dependendElements = (from me in DbContext.MetadataElements
                                     join root in DbContext.MetadataElements on me.ValueListId equals root.ValueListId
                                     where me.Attributes.HasValue && (me.Attributes & 4) == 4 && ((root.Attributes & 4) != 4 || !root.Attributes.HasValue)
                                     select new
                                     {
                                         DependendId = me.ObjectId,
                                         ParentId = root.ObjectId
                                     }).ToList();

            var query = (from me in DbContext.MetadataElements
                             //join mse in ObjectContext.MetadataSchemaElements.Where(t => schemaId == null || t.SchemaId == schemaId.Value) on me.ObjectId equals mse.ElementId into mse2
                             //from leftMse in mse2.DefaultIfEmpty()
                         from c in DbContext.Countries.Where((t) => countryId == null || t.ObjectId == countryId.Value)
                         where schemaId == null ? true : me.MetadataSchemaElements.Any((p) => p.SchemaId == schemaId)
                         && (me.MetadataSchemaElements.Any((p) => !p.Schema.Countries.Any() && (schemaId ?? p.SchemaId) == p.SchemaId)
                                ? (me.CountryId == null ? true : me.CountryId == c.ObjectId)
                                : me.MetadataSchemaElements.Any((p) => p.Schema.Countries.Contains(c) && (schemaId ?? p.SchemaId) == p.SchemaId))
                         select new
                         {
                             me.ObjectId,
                             //SchemaId = leftMse == null ? (Guid?)null : leftMse.SchemaId,
                             me.Name,
                             me.Description,
                             Type = me.ElementType.Name,
                             //IsRequired = leftMse == null ? (bool?)null : leftMse.IsRequired,
                             me.Status,
                             CountryId = c.ObjectId,
                             c.Code,
                             me.Attributes,
                             me.CreateDate,
                             me.CreateUser,
                             me.ChangeDate,
                             me.ChangeUser,
                             me.DefaultValue,
                         }).ToList().Distinct().GroupBy((p) => p.CountryId);
            foreach (var countryGroup in query)
            {
                StreamWriter writer = new StreamWriter(new MemoryStream());
                writer.WriteLine("ElementId|ElementName|ElementDescription|ElementType|ElementStatus|UseLocalValuesForMapping|CreateUser|CreateDate|ChangeUser|ChangeDate|DefaultValue|ParentElementId");
                foreach (var item in countryGroup)
                {
                    var parent = dependendElements.Where(o => o.DependendId == item.ObjectId).FirstOrDefault();



                    writer.WriteLine("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7:yyyyMMdd_HHmmss}|{8}|{9:yyyyMMdd_HHmmss}|{10}|{11}"
                        , item.ObjectId                    //0
                        , item.Name.WrapToExport()         //1
                        , item.Description.WrapToExport()  //2
                        , item.Type.WrapToExport()         //3
                        , item.Status                      //4
                        , (item.Attributes & 2) == 2       //5
                        , item.CreateUser.WrapToExport()   //6
                        , item.CreateDate                  //7
                        , item.ChangeUser.WrapToExport()   //8 
                        , item.ChangeDate                  //9 
                        , item.DefaultValue //10
                        , parent != null ? parent.ParentId.ToString() : string.Empty //11
                    );
                }
                writer.Flush();
                result.Add(string.Format("{0}_CMT_{2}_Element_{1:yyyyMMdd_HHmmss}.txt", countryGroup.First().Code, now, ApplicationSettings.DocumentationVersion), writer.BaseStream);
            }
            return result;
        }

        internal class ElementExtractItem
        {
            public string ElementName { get; set; }
            public Guid ElementId { get; set; }
            public string Value1 { get; set; }
            public Guid Value1Id { get; set; }
            public string Value2 { get; set; }
            public Guid Value2Id { get; set; }
            public string Value3 { get; set; }
            public Guid Value3Id { get; set; }
            public string ValueDetailName { get; set; }

            public Guid? LeafValueId { get; set; }
            public Guid? LeafParentValueId { get; set; }
            public Guid? ValueDetailId { get; set; }

            public string LocalCode { get; set; }
            public string GlobalCode { get; set; }

        }

        internal class ElementExtractItemEqualityComparer : IEqualityComparer<ElementExtractItem>
        {
            public bool Equals(ElementExtractItem x, ElementExtractItem y)
            {
                return x.Value1Id == y.Value1Id
                    && x.Value2Id == y.Value2Id
                    && x.Value3Id == y.Value3Id
                    && x.ValueDetailId == y.ValueDetailId;
            }

            public int GetHashCode(ElementExtractItem obj)
            {
                return obj.Value1Id.GetHashCode() ^ obj.Value2Id.GetHashCode() ^ obj.Value3Id.GetHashCode() ^ obj.ValueDetailId.GetHashCode();
            }
        }

        private void AddAllThirdPartySystems(Guid elementId, bool saveChanges)
        {
            List<ThirdPartySystem> systems = GetThirdPartySystems(elementId);
            foreach (ThirdPartySystem system in systems)
            {
                DataRange dr = new DataRange() { ElementId = system.RelatedObjectId, ApplicationId = system.Id };
                DbContext.DataRanges.Add(dr);
            }
            if (saveChanges)
            {
                DbContext.SaveChanges();
            }
        }

        public List<ThirdPartySystem> GetThirdPartySystems(Guid elementId)
        {
            List<ThirdPartySystem> result = (from s in DbContext.Applications
                                             join dr in DbContext.DataRanges.Where((p) => p.ElementId == elementId) on s.ObjectId equals dr.ApplicationId into j
                                             from data in j.DefaultIfEmpty()
                                             where s.Attributes.HasValue
                                             && (s.Attributes.Value & (byte)ApplicationAttributes.IsThirdPartyApp) == (byte)ApplicationAttributes.IsThirdPartyApp
                                             select new ThirdPartySystem()
                                             {
                                                 Id = s.ObjectId,
                                                 RelatedObjectId = elementId,
                                                 IsAllowed = data != null,
                                                 Name = s.Name
                                             }).ToList();
            return result;
        }

        public ElementBO GetBrandElement()
        {


            try
            {
                SetMapper();
                IQueryable<MetadataElement> result = (from s in DbQueryable.Include(p => p.ElementType)
                                                      where s.Attributes.HasValue
                                                      && (s.Attributes.Value & (byte)ElementAttributes.Brand) == (byte)ElementAttributes.Brand
                                                      select s);

                return ConvertToBusinessObjects(result).Single();
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                InitializeMapper();
            }


        }

        public ElementBO GetElementByAttributes(ElementAttributes attributes)
        {


            try
            {
                SetMapper();
                IQueryable<MetadataElement> result = (from s in DbQueryable.Include(p => p.ElementType)
                                                      where s.Attributes.HasValue
                                                      && (s.Attributes.Value & (byte)attributes) == (byte)attributes
                                                      select s);

                return ConvertToBusinessObjects(result).Single();
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                InitializeMapper();
            }


        }

        public ElementBO GetElementById(Guid id)
        {
            try
            {
                SetMapper();
                IQueryable<MetadataElement> result = (from s in DbQueryable.Include(p => p.ElementType)
                                                      where s.ObjectId == id
                                                      select s);

                return ConvertToBusinessObjects(result).Single();
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                InitializeMapper();
            }
        }

        public async Task<bool> UpdateThirdPartySystemsAccess(ThirdPartySystem system)
        {
            try
            {
                if (system.IsAllowed)
                {
                    DataRange dr = new DataRange() { ElementId = system.RelatedObjectId, ApplicationId = system.Id };
                    DbContext.DataRanges.Add(dr);
                    await DbContext.SaveChangesAsync();
                }
                else
                {
                    DataRange obj = await DbContext.DataRanges.SingleOrDefaultAsync((p) => p.ApplicationId == system.Id && p.ElementId == system.RelatedObjectId);
                    DbContext.DataRanges.Remove(obj);
                    await DbContext.SaveChangesAsync();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool RemoveDerivedElement(Guid elementId)
        {
            // check if element is dependent element
            if (!VerifyElementIsDerivedElement(elementId))
            {
                return false;
            }
            using (TransactionScope transactionScope = TransactionScopeBuilder.CreateScope())
            {
                // remove value from related tables (ValueListLevel, DataRanges)
                ValueListLevelManager.DeleteObjectsByElementId(elementId);
                RemoveThirdPartySystemsAccess(elementId);
                DeleteObject(elementId, false);
                // complete transaction
                transactionScope.Complete();
            }
            return true;
        }

        private void RemoveThirdPartySystemsAccess(Guid elementId)
        {
            MetadataElement metadataElement = GetDbObject(elementId);
            if (metadataElement == null)
            {
                throw new NullReferenceException("Metadata element does not exist in database");
            }
            if (metadataElement.DataRanges == null)
            {
                return;
            }

            DbContext.DataRanges.RemoveRange(metadataElement.DataRanges);
            DbContext.SaveChanges();
        }

        private bool VerifyElementIsDerivedElement(Guid elementId)
        {
            if (TryGetObject(elementId, out ElementBO element))
            {
                return (element.Attributes & ElementAttributes.Derived) == ElementAttributes.Derived;
            }
            else
            {
                return false;
            }
        }

        private Guid? CreateDerivedElement(ExtElementBO element)
        {
            try
            {

                SetMapper();
                string name = element.Name;
                VerifyElementName(name);

                Guid? sourceElementId = element.SourceElementId;
                ElementBO sourceElement = GetObject(sourceElementId.Value);

                if (sourceElement == null)
                {
                    throw new UserFriendlyException("Source element not found", HttpStatusCode.NotAcceptable);
                }

                if (!sourceElement.ValueListId.HasValue)
                {
                    throw new UserFriendlyException("Source element is not a LOV element", HttpStatusCode.NotAcceptable);
                }

                element.ValueListId = sourceElement.ValueListId;
                element.DefaultValue = sourceElement.DefaultValue;
                element.TypeId = sourceElement.TypeId;
                element.Status = sourceElement.Status;
                element.Readonly = true;
                element.Attributes = ((sourceElement.Attributes ?? 0) & ElementAttributes.HasGlobalValues) | ElementAttributes.Derived;

                InsertObject(element);

                AddAllThirdPartySystems(element.ObjectId, false);

                CreateValueListLevels(element);
                return element.ObjectId;
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                InitializeMapper();
            }
        }

        internal void CreateValueListLevels(ElementBO element)
        {
            ExtElementBO extElement = element as ExtElementBO;
            int sourceLevelsCount = 1;
            if (extElement != null && extElement.SourceElementId.HasValue)
            {
                Guid? sourceId = extElement.SourceElementId;
                sourceLevelsCount = DbContext.ValueListLevels.Where((p) => p.ElementId == sourceId).Count();
            }

            for (int i = 1; i <= sourceLevelsCount; i++)
            {
                ValueListLevel valueListLevel = new ValueListLevel()
                {
                    ElementId = element.ObjectId,
                    Level = (byte)i,
                    Name = string.Format("{0} Level {1}", element.Name, i)
                };
                DbContext.ValueListLevels.Add(valueListLevel);
            }
            DbContext.SaveChanges();
        }

        public Stream GetElementStructure(Guid elementId, Guid countryId)
        {
            StreamWriter writer = new StreamWriter(new MemoryStream());
            //header
            List<ValueListLevel> levelNames = DbContext.ValueListLevels.Where((p) => p.ElementId == elementId).OrderBy((p) => p.Level).ToList();
            if (levelNames.Count > 3)
            {
                throw new Exception(string.Format("Too many levels ({1}) defined for element: {0}", elementId, levelNames.Count));
            }

            writer.Write("ElementName|ElementId");
            for (int i = 1; i <= 3; i++)
            {
                string name = "";
                if (levelNames.Count < i)
                {
                    name = string.Format("LOV Level {0}", i);
                }
                else
                {
                    name = levelNames[i - 1].Name;
                }

                writer.Write(string.Format("|{0}", name));
            }
            writer.WriteLine("|LeafLocalValue|LeafValueId|LeafValueLocalId|LeafParentValueId|GlobalCode|LocalCode");
            //body            
            IQueryable<ElementExtractItem> q1 = (from v1 in DbContext.ViewValueTrees
                                                 join e in DbContext.MetadataElements on v1.ElementId equals e.ObjectId
                                                 join vd1a in DbContext.ValueDetails.Where((p) => (p.CountryId ?? countryId) == countryId) on v1.ValueId equals vd1a.ValueId into vd1b
                                                 from vd1 in vd1b.DefaultIfEmpty()
                                                 where v1.ElementId == elementId
                                                 && (v1.CountryId ?? countryId) == countryId
                                                 && v1.Status > 0
                                                 && v1.ParentValueId == null
                                                 && (vd1 == null ? true : vd1.Type == 0)
                                                 select new ElementExtractItem()
                                                 {
                                                     ElementName = e.Name,
                                                     ElementId = e.ObjectId,
                                                     Value1 = v1.TextValue,
                                                     Value2 = string.Empty,
                                                     Value3 = string.Empty,
                                                     Value1Id = v1.ValueId.Value,
                                                     Value2Id = Guid.Empty,
                                                     Value3Id = Guid.Empty,
                                                     ValueDetailName = vd1.Value,
                                                     LeafValueId = v1.ValueId.Value,
                                                     LeafParentValueId = null,
                                                     ValueDetailId = vd1.ObjectId,
                                                     LocalCode = vd1.LocalCode,
                                                     GlobalCode = v1.GlobalCode
                                                 });
            IQueryable<ElementExtractItem> q2 = (from v1 in DbContext.ViewValueTrees
                                                 join e in DbContext.MetadataElements on v1.ElementId equals e.ObjectId
                                                 join v2 in DbContext.ViewValueTrees on v1.ValueId equals v2.ParentValueId
                                                 join vd2a in DbContext.ValueDetails.Where((p) => (p.CountryId ?? countryId) == countryId) on v2.ValueId equals vd2a.ValueId into vd2b
                                                 from vd2 in vd2b.DefaultIfEmpty()
                                                 where v1.ElementId == elementId
                                                 && (v2.CountryId ?? countryId) == countryId
                                                 && v2.ElementId == elementId
                                                 && v1.Status > 0
                                                 && v2.Status > 0
                                                 && v1.ParentValueId == null
                                                 && (vd2 == null ? true : vd2.Type == 0)
                                                 select new ElementExtractItem()
                                                 {
                                                     ElementName = e.Name,
                                                     ElementId = e.ObjectId,
                                                     Value1 = v1.TextValue,
                                                     Value2 = v2.TextValue,
                                                     Value3 = string.Empty,
                                                     Value1Id = v1.ValueId.Value,
                                                     Value2Id = v2.ValueId.Value,
                                                     Value3Id = Guid.Empty,
                                                     ValueDetailName = vd2.Value,
                                                     LeafValueId = v2.ValueId.Value,
                                                     LeafParentValueId = v1.ValueId.Value,
                                                     ValueDetailId = vd2.ObjectId,
                                                     LocalCode = vd2.LocalCode,
                                                     GlobalCode = v2.GlobalCode
                                                 });
            IQueryable<ElementExtractItem> q3 = from v1 in DbContext.ViewValueTrees
                                                join e in DbContext.MetadataElements on v1.ElementId equals e.ObjectId
                                                join v2 in DbContext.ViewValueTrees on v1.ValueId equals v2.ParentValueId
                                                join v3 in DbContext.ViewValueTrees on v2.ValueId equals v3.ParentValueId
                                                join vd3a in DbContext.ValueDetails.Where((p) => (p.CountryId ?? countryId) == countryId) on v3.ValueId equals vd3a.ValueId into vd3b
                                                from vd3 in vd3b.DefaultIfEmpty()
                                                where v1.ElementId == elementId
                                                && (v3.CountryId ?? countryId) == countryId
                                                && v2.ElementId == elementId
                                                && v3.ElementId == elementId
                                                && v1.Status > 0
                                                && v2.Status > 0
                                                && v3.Status > 0
                                                && v1.ParentValueId == null
                                                && (vd3 == null ? true : vd3.Type == 0)
                                                select new ElementExtractItem()
                                                {
                                                    ElementName = e.Name,
                                                    ElementId = e.ObjectId,
                                                    Value1 = v1.TextValue,
                                                    Value2 = v2.TextValue,
                                                    Value3 = v3.TextValue,
                                                    Value1Id = v1.ValueId.Value,
                                                    Value2Id = v2.ValueId.Value,
                                                    Value3Id = v3.ValueId.Value,
                                                    ValueDetailName = vd3.Value,
                                                    LeafValueId = v3.ValueId.Value,
                                                    LeafParentValueId = v2.ValueId.Value,
                                                    ValueDetailId = vd3.ObjectId,
                                                    LocalCode = vd3.LocalCode,
                                                    GlobalCode = v3.GlobalCode
                                                };

            List<ElementExtractItem> query = q1.Concat(q2).Concat(q3).OrderBy(p => p.Value1).ThenBy(p => p.Value2).ThenBy(p => p.Value3).ToList().Distinct(new ElementExtractItemEqualityComparer()).ToList();

            foreach (ElementExtractItem item in query)
            {
                writer.WriteLine(string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}"
                    , item.ElementName.WrapToExport()          //0
                    , item.ElementId                           //1
                    , item.Value1.WrapToExport()               //2
                    , item.Value2.WrapToExport()               //3
                    , item.Value3.WrapToExport()               //4
                    , item.ValueDetailName.WrapToExport()      //5
                    , item.LeafValueId                         //6
                    , item.ValueDetailId                       //7
                    , item.LeafParentValueId                   //8
                    , item.GlobalCode
                    , item.LocalCode
                ));
            }
            writer.Flush();
            return writer.BaseStream;
        }

        public virtual bool CanDeleteDerivedElement(Guid elementId)
        {
            // check if element is dependent element
            bool result = VerifyElementIsDerivedElement(elementId);

            result &= !IsUsedInSchema(elementId);

            return result;
        }

        public bool UpdateElementType(Guid elementId, Guid typeId)
        {
            bool result = true;
            result &= ElementTypeManager.ObjectExists(typeId, false);
            result &= TryGetObject(elementId, out ElementBO element);
            result &= element?.TypeId != typeId;

            if (result)
            {
                using (TransactionScope transactionScope = TransactionScopeBuilder.CreateScope())
                {
                    try
                    {
                        element.TypeId = typeId;
                        Guid? valueListId = element.ValueListId;
                        element.ValueListId = null;

                        UpdateObject(element);
                        result &= RemoveDependentValues(element, valueListId);
                        CreateElementValueList(element);
                        CreateValueListLevels(element);
                        transactionScope.Complete();
                    }
                    catch
                    {
                        result = false;
                    }
                }
            }
            return result;
        }

        private bool RemoveDependentValues(ElementBO element, Guid? valueListId)
        {
            if (valueListId == Guid.Empty)
            {
                throw new ArgumentOutOfRangeException("ValueListId cannot be Guid.Empty");
            }

            Guid elementId = element.ObjectId;

            ValueListLevelManager.DeleteObjectsByElementId(elementId);

            if (valueListId.HasValue)
            {
                ValueListManager.DeleteObjectsRecursively(valueListId.Value);
            }
            return true;
        }

        public List<ListValue> GetElementValuesForDefault(Guid elementId)
        {
            return DbContext.ViewMetadataElementTrees.Where((p) => p.ParentId == elementId)
                .Select((p) => new { p.ObjectId, p.Name }).Distinct().ToList().OrderBy((p) => p.Name)
                .Select((p) => new ListValue(p.Name, p.ObjectId.ToString()))
                .ToList();
        }

        public bool SetDefaultValue(Guid elementId, string value)
        {
            try
            {
                MetadataElement obj = DbQueryable.FirstOrDefault(p => p.ObjectId == elementId);
                if (obj == null)
                {
                    throw new UserFriendlyException("Element not found", HttpStatusCode.NotAcceptable);
                }

                obj.DefaultValue = value;
                DbContext.SaveChanges();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public Dictionary<Guid, string> GetDepentdentElements(Guid id)
        {
            return (from e in DbQueryable
                    from e1 in DbQueryable
                    where
                    e.ObjectId == id
                    && e1.Status == 1
                    && e1.ValueListId == e.ValueListId
                    && (e1.Attributes & 4) == 4
                    select e1)
                    .ToDictionary(k => k.ObjectId, v => v.Name);

        }

        public KeyValuePair<Guid, string> getParentElementByChildId(Guid id)
        {
            return (from e in DbQueryable
                    from e1 in DbQueryable
                    where
                    e.ObjectId == id
                    && e1.ObjectId != e.ObjectId
                    && e1.ValueListId == e.ValueListId
                    && ((e1.Attributes ?? 0) & 4) != 4
                    select new { e1.ObjectId, e1.Name })
                    .ToList()
                    .Select(e1 => new KeyValuePair<Guid, string>(e1.ObjectId, e1.Name))
             .FirstOrDefault();
        }

        public virtual void DeleteObject(Guid objectId, bool useSoftDelete)
        {
            if (useSoftDelete)
            {
                DeleteObject(objectId);
            }
            else
            {
                base.DeleteObject(objectId);
            }
        }

        public override void DeleteObject(Guid objectId)
        {
            ElementBO bo = GetObject(objectId);
            bo.Status = 0;
            UpdateObject(bo);
        }

        public void ActivateObject(Guid objectId)
        {
            ElementBO bo = GetObject(objectId);
            bo.Status = 1;
            UpdateObject(bo);
        }

        public IEnumerable<ElementBO> GetMetadataElementsByCampaignAndImport(Guid campaignId, Guid schemaId, bool showIndirect, out int totalRowCount, Guid? importId = null, int startIndex = 0, int count = 10)
        {


            List<GetElementsForCampaignAndImportResult> result = DbContext.GetElementsForCampaignAndImport(campaignId, importId, schemaId, showIndirect, startIndex, count).ToList();

            List<ElementBO> elements = new List<ElementBO>();
            foreach (GetElementsForCampaignAndImportResult r in result)
            {



                ElementBO e = new ElementBO();
                e.ObjectId = r.ElementId;
                e.Name = r.Name;
                if (r.IsFullyMapped == 0 && r.IsPartiallyMapped == 0)
                {
                    e.MappingStatus = MappingStatus.NotMapped;
                }
                else if (r.IsFullyMapped == 1)
                {
                    e.MappingStatus = MappingStatus.AllMapped;
                }
                else
                {
                    e.MappingStatus = MappingStatus.PartiallyMapped;
                }
                e.IsDirect = r.IsDirect == 1;
                elements.Add(e);



            }
            totalRowCount = 0;
            if (result.Any())
            {
                totalRowCount = result.FirstOrDefault().AllRows ?? 0;
            }


            return elements;
        }

        public List<TreeElementBO> GetActiveElementsWithDescription()
        {
            List<TreeElementBO> result = (from e in base.DbQueryable
                                          join v in DbContext.Values on e.DefaultValue equals v.ObjectId.ToString() into j
                                          from res in j.DefaultIfEmpty()
                                          where e.Status == 1
                                          select new { e, res })
                          .ToList()
                          .Select(
                (p) => new TreeElementBO()
                {
                    ObjectId = p.e.ObjectId,
                    Name = p.e.Name,
                    DataType = p.e.ElementType.Name,
                    IsRequired = false,
                    Description = p.e.Description,
                    DefaultValue = new ListValue(p.e.DefaultValue, p.res == null ? p.e.DefaultValue : p.res.TextValue)
                })
                .OrderBy((p) => p.Name)
                .ToList();
            return result;
        }

        public List<TreeElementBO> GetActiveElementsWithDescriptionBySchema(Guid schemaId)
        {
            List<TreeElementBO> result = (from e in DbContext.ViewMetadataElementTrees
                                          from el in DbQueryable
                                          from se in DbContext.MetadataSchemaElements
                                          .Where((s) => s.SchemaId == schemaId && s.ElementId == e.ObjectId).DefaultIfEmpty()
                                          from v in DbContext.Values.Where((p) => p.ObjectId.ToString() == se.DefaultValue).DefaultIfEmpty()
                                          where e.Level == 0
                                          && el.ObjectId == e.ObjectId
                                          select new { e, se, el.Description, v })
                          .ToList()
                          .Select((p) =>
                new TreeElementBO()
                {
                    ObjectId = p.e.ObjectId,
                    Name = p.e.Name,
                    DataType = p.e.DataType,
                    Description = p.Description,
                    IsActive = p.se != null,
                    IsRequired = p.se != null ? p.se.IsRequired : false,
                    IsLov = p.e.IsLov.Value,
                    DefaultValue = p.se == null ? new ListValue(p.e.DefaultValueText, p.e.DefaultValue)
                    : new ListValue(p.v == null ? p.se.DefaultValue : p.v.TextValue, p.se.DefaultValue)
                })
                .OrderBy((p) => p.Name)
                .ToList();
            return result;
        }

        private bool IsUsedInSchema(Guid elementId)
        {
            return DbContext.MetadataSchemaElements.Any(schemaElement => schemaElement.ElementId == elementId);
        }
    }
}
