using AutoMapper;
using CMT.BL.Core;
using CMT.BO;
using CMT.BO.Admin;
using CMT.BO.Metadata;
using CMT.Common;
using CMT.DL;
using CMT.DL.Core;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Transactions;

namespace CMT.BL
{
    public class ValueManager : BaseObjectManager<CmtEntities, Value, ValueBO>
    {
        public ValueDetailManager ValueDetailManager { get; }
        public ValueTagManager ValueTagManager { get; }

        public ValueManager(CmtEntities dbContext, ValueDetailManager valueDetailManager, ValueTagManager valueTagManager) : base(dbContext)
        {
            ValueDetailManager = valueDetailManager;
            ValueTagManager = valueTagManager;
        }

        public override void InitializeMapper()
        {
            MapperConfiguration config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Value, ValueBO>().ForMember(p => p.ParentId, opt => opt.MapFrom(src => src.ParentId));
                cfg.CreateMap<ValueBO, Value>().ForMember(p => p.ParentId, opt => opt.MapFrom(src => src.ParentId)).ForMember(p => p.ParentValue, opt => opt.Ignore());
            });

            Mapper = config.CreateMapper();
        }

        public IEnumerable<ValueBO> GetObjectsById(Guid objectId)
        {

            IQueryable<Value> schemas = (from s in DbQueryable
                           .Include(p => p.ValueList)
                                         where s.ObjectId == objectId
                                         select s);
            return ConvertToBusinessObjects(schemas);
        }

        public Guid? AddChildValue(TreeElementBO value)
        {
            if (!value.ParentId.HasValue || value.ParentId.Value == Guid.Empty)
            {
                throw new ArgumentNullException("Parent id cannot be null");
            }

            MetadataElement parentElement;
            Value parentValue = null;
            Guid? valueListId;

            parentElement = (from e in DbContext.MetadataElements
                             where e.ObjectId == value.ParentId
                             select e).FirstOrDefault();

            if (parentElement == null)
            {
                parentValue = (from v in DbQueryable
                               where v.ObjectId == value.ParentId
                               select v).FirstOrDefault();
            }
            if (parentElement == null && parentValue == null)
            {
                throw new UserFriendlyException("Parent object not found", HttpStatusCode.NotAcceptable);
            }
            else
            {
                using (TransactionScope transactionScope = TransactionScopeBuilder.CreateScope())
                {
                    valueListId = parentElement != null ? parentElement.ValueListId : parentValue.ChildListId;
                    if (!valueListId.HasValue)
                    {
                        ValueList valueListDbObj = new ValueList()
                        {
                            Name = Guid.NewGuid().ToString()
                        };
                        DbContext.ValueLists.Add(valueListDbObj);
                        DbContext.SaveChanges();
                        valueListId = valueListDbObj.ObjectId;
                        if (valueListId == Guid.Empty)
                        {
                            throw new NullReferenceException("valueListId cannot be empty");
                        }

                        ViewMetadataElementTree parent = DbContext.ViewMetadataElementTrees.First(o => o.ObjectId == value.ParentId);
                        Guid elementId = parentElement?.ObjectId ?? parent.RootId.Value;
                        string elementName = DbContext.MetadataElements.First(p => p.ObjectId == elementId).Name;


                        if (!DbContext.ValueListLevels.Any(o => o.ElementId == elementId && o.Level == parent.Level + 1))
                        {
                            ValueListLevel valueListLevel = new ValueListLevel()
                            {
                                ElementId = elementId,
                                Level = (byte)(parent.Level + 1),
                                Name = string.Format("{0} Level {1}", elementName, parent.Level + 1)
                            };

                            DbContext.ValueListLevels.Add(valueListLevel);
                            DbContext.SaveChanges();
                        }
                    }
                    else
                    {
                        string name = value.Name;
                        if (DbContext.Values.Any(p => p.ValueListId == valueListId && p.TextValue == name))
                        {
                            throw new UserFriendlyException("Value already exists on this level", HttpStatusCode.NotAcceptable);
                        }
                    }

                    Value valueDbObj = new Value();
                    valueDbObj.TextValue = value.Name;
                    valueDbObj.Status = 1;
                    valueDbObj.ValueListId = valueListId;

                    DbContext.Values.Add(valueDbObj);
                    if (parentElement != null && !parentElement.ValueListId.HasValue)
                    {
                        parentElement.ValueListId = valueListId;
                    }
                    else if (parentValue != null && !parentValue.ChildListId.HasValue)
                    {
                        parentValue.ChildListId = valueListId;
                    }

                    DbContext.SaveChanges();
                    transactionScope.Complete();
                    return valueDbObj.ObjectId;
                }
            }
        }

        public Dictionary<string, Stream> GetValueDetailExtracts(DateTime now, Guid? schemaId = null, Guid? countryId = null)
        {
            Dictionary<string, Stream> result = new Dictionary<string, Stream>();

            var query = (from vt in DbContext.ViewValueTrees
                         join e in DbContext.MetadataElements on vt.ElementId equals e.ObjectId
                         from vd in DbContext.ValueDetails.Where((v) => vt.ValueId == v.ValueId && v.Type == (byte)ValueDetailType.LocalValue).DefaultIfEmpty()
                         from c in DbContext.Countries.Where((t) => (countryId == null || t.ObjectId == countryId.Value) && (!vt.CountryId.HasValue || vt.CountryId == t.ObjectId))
                         where (vd.CountryId == c.ObjectId || (vd == null && e.Attributes.HasValue && (e.Attributes.Value & 2) == 2 && vt.CountryId == c.ObjectId)) &&
                               (schemaId == null || vt.SchemaId == schemaId.Value)
                               && (vd != null || vt.CountryId.HasValue || (e.Attributes.HasValue && (e.Attributes.Value & 2) == 2))
                         select new
                         {
                             CountryId = c.ObjectId,
                             c.Code,
                             vt,
                             vd,
                             e.Attributes
                         }).Distinct().ToList();


            var grupeduer = query.GroupBy(p => p.CountryId);
            foreach (var countryGroup in grupeduer)
            {
                StreamWriter writer = new StreamWriter(new MemoryStream());
                writer.WriteLine("LovalValueId|GlobalValueId|LocalValue|LocalCode|LocalValueType|ElementId|CreateUser|CreateDate|ChangeUser|ChangeDate");
                foreach (var item in countryGroup.GroupBy(p => new { p.vt.ValueId, p.vt.ElementId }))
                {

                    if (item.First().vd != null)
                    {

                        writer.WriteLine(string.Format("{0}|{1}|{2}|{9}|{3:g}|{4}|{5}|{6:yyyyMMdd_HHmmss}|{7}|{8:yyyyMMdd_HHmmss}"
                        , item.First().vd != null ? item.First().vd.ObjectId : item.Key.ValueId                                  //0
                        , item.First().vd != null ? item.First().vd.ValueId : item.First().vd.ValueId                   //1
                        , item.First().vd != null ? item.First().vd.Value.WrapToExport() : item.First().vt.TextValue.WrapToExport()      //2
                        , item.First().vd != null ? (ValueDetailType)item.First().vd.Type : 0    //3
                        , item.First().vt.ElementId                 //4
                        , item.First().vd != null ? item.First().vd.CreateUser.WrapToExport() : item.First().vt.CreateUser.WrapToExport() //5
                        , item.First().vd != null ? item.First().vd.CreateDate : item.First().vt.CreateDate                //6
                        , item.First().vd != null ? item.First().vd.ChangeUser.WrapToExport() : item.First().vt.ChangeUser.WrapToExport() //7
                        , item.First().vd != null ? item.First().vd.ChangeDate : item.First().vt.ChangeDate                //8
                        , item.First().vd != null ? item.First().vd.LocalCode.WrapToExport() : item.First().vt.GlobalCode.WrapToExport()  //9
                        ));
                    }
                    else
                    {
                        writer.WriteLine(string.Format("{0}|{1}|{2}|{9}|{3:g}|{4}|{5}|{6:yyyyMMdd_HHmmss}|{7}|{8:yyyyMMdd_HHmmss}"
                          , item.First().vt.ValueId                                //0
                          , item.First().vt.GlobalValueId                 //1
                          , item.First().vt.TextValue    //2
                          , (ValueDetailType)0    //3
                          , item.First().vt.ElementId                 //4
                          , item.First().vt != null ? item.First().vt.CreateUser.WrapToExport() : item.First().vt.CreateUser.WrapToExport() //5
                          , item.First().vt != null ? item.First().vt.CreateDate : item.First().vt.CreateDate                //6
                          , item.First().vt != null ? item.First().vt.ChangeUser.WrapToExport() : item.First().vt.ChangeUser.WrapToExport() //7
                          , item.First().vt != null ? item.First().vt.ChangeDate : item.First().vt.ChangeDate                //8
                          , item.First().vt != null ? item.First().vt.GlobalCode.WrapToExport() : item.First().vt.GlobalCode.WrapToExport()  //9
                          ));

                    }
                }
                writer.Flush();
                result.Add(string.Format("{0}_CMT_{2}_LocalValueDetail_{1:yyyyMMdd_HHmmss}.txt", countryGroup.First().Code, now, ApplicationSettings.DocumentationVersion), writer.BaseStream);
            }

            return result;
        }

        public Dictionary<string, Stream> GetProductMappingExtracts(DateTime now)
        {
            Dictionary<string, Stream> result = new Dictionary<string, Stream>();

            var query = (from e in DbContext.MetadataElements.Where(elem => elem.Attributes.HasValue && (elem.Attributes.Value & 1) == 1)
                         join l in DbContext.ValueLists on e.ValueListId equals l.ObjectId
                         join lv in DbContext.Values.Where(elem => elem.ParentId.HasValue) on e.ValueListId equals lv.ValueListId
                         join lvp in DbContext.Values on lv.ParentId equals lvp.ObjectId
                         join lvpd in DbContext.ValueDetails.Where(elem => elem.Type == (byte)ValueDetailType.ValueAttribute) on lvp.ObjectId equals lvpd.ValueId
                         join c in DbContext.Countries on lvp.CountryId == null ? lv.CountryId : lvp.CountryId equals c.ObjectId
                         select new
                         {
                             GlobalPfizerBrandID = lvp.ExternalId,
                             GlobalPfizerBrandName = lvp.TextValue,
                             CountryId = lvp.CountryId == null ? lv.CountryId : lvp.CountryId,
                             Code = c.Code,
                             ElementId = e.ObjectId,
                             LocalValue = e.Name, //?
                             LovalValueId = lvpd.ObjectId,
                             AttributeElementId = lvpd.AttributeElementId,
                             AttributeValueId = lvpd.AttributeValueId,
                             lvpd,
                             lvp
                         }).Distinct().ToList();


            var grupeduer = query.GroupBy(p => p.CountryId);

            foreach (var countryGroup in grupeduer)
            {
                StreamWriter writer = new StreamWriter(new MemoryStream());
                writer.WriteLine("GlobalPfizerBrandID|GlobalPfizerBrandName|AttributeElementId|AttributeValueId|ElementId|CreateUser|CreateDate|ChangeUser|ChangeDate");
                foreach (var item in countryGroup.GroupBy(p => new { p.GlobalPfizerBrandID, p.GlobalPfizerBrandName, p.ElementId, p.AttributeElementId, p.AttributeValueId }).OrderBy(p => p.First().GlobalPfizerBrandName))
                {

                    writer.WriteLine(string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6:yyyyMMdd_HHmmss}|{7}|{8:yyyyMMdd_HHmmss}"
                    , item.First().GlobalPfizerBrandID                  //0
                    , item.First().GlobalPfizerBrandName                //1
                    , item.First().AttributeElementId                   //2
                    , item.First().AttributeValueId                     //3
                    , item.First().ElementId                            //4
                    , item.First().lvpd != null ? item.First().lvpd.CreateUser.WrapToExport() : item.First().lvp.CreateUser.WrapToExport() //5
                    , item.First().lvpd != null ? item.First().lvpd.CreateDate : item.First().lvp.CreateDate                //6
                    , item.First().lvpd != null ? item.First().lvpd.ChangeUser.WrapToExport() : item.First().lvp.ChangeUser.WrapToExport() //7
                    , item.First().lvpd != null ? item.First().lvpd.ChangeDate : item.First().lvp.ChangeDate                //8
                    ));
                }
                writer.Flush();
                result.Add(string.Format("{0}_CMT_{2}_BrandMapping_{1:yyyyMMdd_HHmmss}.txt", countryGroup.First().Code, now, ApplicationSettings.DocumentationVersion), writer.BaseStream);
            }

            return result;
        }

        public Dictionary<string, Stream> GetValuesExtracts(DateTime now, Guid? schemaId = null, Guid? countryId = null)
        {
            Dictionary<string, Stream> result = new Dictionary<string, Stream>();

            var q = (from v in DbContext.ViewValueTrees
                     join gb in DbContext.Values on v.GlobalValueId equals gb.ObjectId into gbv
                     from gbi in gbv.DefaultIfEmpty()
                     join e in DbContext.MetadataElements on v.ElementId equals e.ObjectId
                     from c in DbContext.Countries.Where((t) => (countryId == null || t.ObjectId == countryId.Value) && (!v.CountryId.HasValue || v.CountryId == t.ObjectId))
                     from p in DbContext.Values.Where((pv) => pv.ObjectId == v.GlobalValueId).DefaultIfEmpty()
                     where (schemaId == null || v.SchemaId == schemaId.Value) //&& c.Code == "DK"// && (v.ElementId == new Guid("daa56cf1-5315-e711-942c-00155dcb050a") || v.ElementId == new Guid("19A0BE05-C579-E611-942A-00155D034947"))
                     select new
                     {
                         v,
                         p,
                         CountryId = c.ObjectId,
                         c.Code,
                         gbi,
                         e.Attributes

                     }).Distinct().ToList();
            var query = q.GroupBy(p => p.CountryId);
            foreach (var countryGroup in query)
            {
                StreamWriter writer = new StreamWriter(new MemoryStream());
                writer.WriteLine("GlobalValueId|GlobalValue|GlobalCode|ParentId|ParentValue|IsLeaf|Level|ElementId|ValueStatus|ValueRequestedBy|ValueApprovedBy|ValueRejectedBy|CreateUser|CreateDate|ChangeUser|ChangeDate");
                foreach (var item in countryGroup.GroupBy(p => new { p.v.ValueId, p.v.ElementId }).ToList())
                {

                    //element has global values
                    if (item.First().Attributes.HasValue && ((item.First().Attributes.Value & 2) == 2))
                    {

                        if (item.First().gbi != null)
                        {
                            writer.WriteLine("{0}|{1}|{14}|{2}|{15}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}|{11:yyyyMMdd_HHmmss}|{12}|{13:yyyyMMdd_HHmmss}"
                               , item.First().gbi.ObjectId                                     //0
                               , item.First().gbi.TextValue      //1
                               , ""                 //2
                               , item.First().v.IsLeaf                        //3
                               , item.First().v.Level                         //4
                               , item.First().v.ElementId                     //5
                               , item.First().gbi.CountryId.HasValue && item.First().p != null ? item.First().p.Status : item.First().v.Status                        //6
                               , item.First().gbi.RequestedBy.WrapToExport()    //7
                               , item.First().gbi.ApprovedBy.WrapToExport()     //8
                               , item.First().gbi.RejectedBy.WrapToExport()     //9
                               , item.First().gbi.CountryId.HasValue && item.First().p != null ? item.First().p.CreateUser.WrapToExport() : item.First().v.CreateUser.WrapToExport()     //10
                               , item.First().gbi.CountryId.HasValue && item.First().p != null ? item.First().p.CreateDate : item.First().v.CreateDate                   //11
                               , item.First().gbi.CountryId.HasValue && item.First().p != null ? item.First().p.ChangeUser.WrapToExport() : item.First().v.ChangeUser.WrapToExport()    //12
                               , item.First().gbi.CountryId.HasValue && item.First().p != null ? item.First().p.ChangeDate : item.First().v.ChangeDate                    //13
                               , item.First().gbi.CountryId.HasValue && item.First().p != null ? item.First().p.GlobalCode.WrapToExport() : item.First().v.GlobalCode.WrapToExport()     //{14}
                               , ""     //15

                           );
                        }

                    }
                    else
                    {
                        writer.WriteLine("{0}|{1}|{14}|{2}|{15}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}|{11:yyyyMMdd_HHmmss}|{12}|{13:yyyyMMdd_HHmmss}"
                            , item.Key.ValueId                                     //0
                            , item.First().v.CountryId.HasValue && item.First().p != null ? item.First().p.TextValue.WrapToExport() : item.First().v.TextValue.WrapToExport()      //1
                            , item.First().v.ParentValueId                 //2
                            , item.First().v.IsLeaf                        //3
                            , item.First().v.Level                         //4
                            , item.First().v.ElementId                     //5
                            , item.First().v.CountryId.HasValue && item.First().p != null ? item.First().p.Status : item.First().v.Status                        //6
                            , item.First().v.RequestedBy.WrapToExport()    //7
                            , item.First().v.ApprovedBy.WrapToExport()     //8
                            , item.First().v.RejectedBy.WrapToExport()     //9
                            , item.First().v.CountryId.HasValue && item.First().p != null ? item.First().p.CreateUser.WrapToExport() : item.First().v.CreateUser.WrapToExport()     //10
                            , item.First().v.CountryId.HasValue && item.First().p != null ? item.First().p.CreateDate : item.First().v.CreateDate                   //11
                            , item.First().v.CountryId.HasValue && item.First().p != null ? item.First().p.ChangeUser.WrapToExport() : item.First().v.ChangeUser.WrapToExport()    //12
                            , item.First().v.CountryId.HasValue && item.First().p != null ? item.First().p.ChangeDate : item.First().v.ChangeDate                    //13
                            , item.First().v.CountryId.HasValue && item.First().p != null ? item.First().p.GlobalCode.WrapToExport() : item.First().v.GlobalCode.WrapToExport()     //{14}
                            , item.First().v.ParentValue.WrapToExport()     //15

                            );
                    }


                }
                writer.Flush();
                result.Add(string.Format("{0}_CMT_{2}_GlobalValue_{1:yyyyMMdd_HHmmss}.txt", countryGroup.First().Code, now, ApplicationSettings.DocumentationVersion), writer.BaseStream);
            }
            return result;
        }

        public override void DeleteObject(Guid objectId)
        {
            DeleteObject(objectId, true);
        }

        public void DeleteObject(Guid objectId, bool useSoftDelete)
        {
            if (useSoftDelete)
            {
                SoftDeleteObject(objectId);
            }
            else
            {
                DeleteObjectInner(objectId);
            }
        }

        private void DeleteObjectInner(Guid objectId)
        {
            Value dbObject = DbSet
                .Single(p => p.ObjectId.Equals(objectId));
            if (dbObject.CampaignValues != null && dbObject.CampaignValues.Count > 0)
            {
                DbContext.CampaignValues.RemoveRange(dbObject.CampaignValues);
            }
            if (dbObject.ValueDetails != null && dbObject.ValueDetails.Count > 0)
            {
                DbContext.ValueDetails.RemoveRange(dbObject.ValueDetails);
            }
            DeleteObjectCore(dbObject);
        }

        private void SoftDeleteObject(Guid objectId)
        {
            ValueBO bo = GetObject(objectId);
            bo.Status = (byte)ValueStatus.Inactive;
            UpdateObject(bo);
            ValueTagManager.RemoveTagsByValueId(bo.ObjectId);
        }

        public void ActivateObject(Guid objectId)
        {
            ValueBO bo = GetObject(objectId);
            bo.Status = (byte)ValueStatus.Active;
            UpdateObject(bo);
        }
        public IEnumerable<ValueBO> GetParentObjectsByValueListId(Guid valueListId)
        {
            IQueryable<Value> query = (from v in DbQueryable.Include(o => o.ValueDetails)
                                       where v.ValueListId == valueListId && v.ParentId != null
                                       select v.ParentValue).Distinct().ToList().AsQueryable();

            return ConvertToBusinessObjects(query);


        }
        public IEnumerable<ValueBO> GetObjectsByValueListId(Guid valueListId)
        {
            IQueryable<Value> query = (from v in DbQueryable.Include(o => o.ValueDetails)
                                       where v.ValueListId == valueListId
                                       select v).ToList().AsQueryable();

            return ConvertToBusinessObjects(query);


        }
        public IEnumerable<ValueBO> GetObjectsByValueListIdAndCountryid(Guid valueListId, Guid countryId)
        {
            IQueryable<Value> query = (from v in DbQueryable.Include(o => o.ValueDetails)
                                       where v.ValueListId == valueListId && v.CountryId == countryId

                                       select v).ToList().AsQueryable();

            return ConvertToBusinessObjects(query);


        }

        public Dictionary<Guid, string> GetValuesForSchemaElement(Guid schemaElementId, int level, Guid schemaId, Guid countryId, string searchTerm)
        {
            var query = (from vvt in DbContext.ViewValueTrees
                         from vd in DbContext.ValueDetails.Where(o => o.ValueId == vvt.ValueId && o.CountryId == countryId).DefaultIfEmpty()
                         from p in DbContext.Values.Where(v => v.ObjectId == vvt.GlobalValueId).DefaultIfEmpty()
                         join se in DbContext.MetadataSchemaElements on vvt.ElementId equals se.ElementId
                         where (!vvt.CountryId.HasValue || vvt.CountryId == countryId) &&
                               se.ObjectId == schemaElementId &&
                               vvt.SchemaId == schemaId &&
                               vvt.Level == level &&
                               vvt.Status == 1 &&
                               vvt.ValueId != null
                         select new
                         {
                             ObjectId = vvt.ValueId.Value,
                             GlobalValue = p != null ? p.TextValue : vvt.CountryId.HasValue ? "(Undefined)" : vvt.TextValue,
                             GlobalCode = p != null ? p.GlobalCode : vvt.CountryId.HasValue ? null : vvt.GlobalCode,
                             LocalValue = vd != null && p == null ? ((vd.Value == null || vd.Value.Trim() == string.Empty) ? vvt.TextValue : vd.Value) : vvt.TextValue,
                             LocalCode = vd != null ? vd.LocalCode : p != null ? vvt.GlobalCode : null,
                         }).ToList();

            var query2 = from q in query
                         select
                         new
                         {
                             ObjectId = q.ObjectId,
                             LocalName = FormatValueForSchemaElement(q.LocalValue, q.LocalCode),
                             GlobalName = FormatValueForSchemaElement(q.GlobalValue, q.GlobalCode)
                         };

            var groupedQuery = query2
                .GroupBy(o => o.ObjectId)
                .Select(o => new
                {
                    ObjectId = o.Key,
                    Name = o.First().GlobalName + " - " + string.Join(", ", o.Select(t => t.LocalName))
                });

            if (!string.IsNullOrEmpty(searchTerm))
            {
                return groupedQuery.Where(o => o.Name.ToLower().Contains(searchTerm.ToLower())).OrderBy(o => o.Name).ToDictionary(o => o.ObjectId, o => o.Name);
            }

            return groupedQuery.OrderBy(o => o.Name).ToDictionary(o => o.ObjectId, o => o.Name);
        }

        private string FormatValueForSchemaElement(string name, string code)
        {
            if (!string.IsNullOrWhiteSpace(code))
            {
                return string.Format("{0} ({1})", name, code);
            }
            else
            {
                return name;
            }
        }


        public Dictionary<Guid, string> GetValuesForElement(Guid elementId, int level, Guid countryId, string searchTerm)
        {
            var query = (from vvt in DbContext.ViewValueTrees
                         from vd in DbContext.ValueDetails.Where(o => o.ValueId == vvt.ValueId && o.CountryId == countryId).DefaultIfEmpty()
                         from p in DbContext.Values.Where(v => v.ObjectId == vvt.GlobalValueId).DefaultIfEmpty()
                         join se in DbContext.MetadataElements on vvt.ElementId equals se.ObjectId
                         where (!vvt.CountryId.HasValue || vvt.CountryId == countryId) &&
                               se.ObjectId == elementId &&
                               vvt.Level == level &&
                               vvt.Status == 1 &&
                               vvt.ValueId != null
                         select new
                         {
                             ObjectId = vvt.ValueId.Value,
                             GlobalValue = p != null ? p.TextValue : vvt.CountryId.HasValue ? "(Undefined)" : vvt.TextValue,
                             GlobalCode = p != null ? p.GlobalCode : vvt.CountryId.HasValue ? null : vvt.GlobalCode,
                             LocalValue = vd != null && p == null ? ((vd.Value == null || vd.Value.Trim() == string.Empty) ? vvt.TextValue : vd.Value) : vvt.TextValue,
                             LocalCode = vd != null ? vd.LocalCode : p != null ? vvt.GlobalCode : null,
                         }).Distinct().ToList();

            var query2 = from q in query
                         select
                         new
                         {
                             ObjectId = q.ObjectId,
                             LocalName = FormatValueForSchemaElement(q.LocalValue, q.LocalCode),
                             GlobalName = FormatValueForSchemaElement(q.GlobalValue, q.GlobalCode)
                         };

            var groupedQuery = query2
                            .GroupBy(o => o.ObjectId)
                            .Select(o => new
                            {
                                ObjectId = o.Key,
                                Name = o.First().GlobalName + " - " + string.Join(", ", o.Select(t => t.LocalName))
                            });

            if (!string.IsNullOrEmpty(searchTerm))
            {
                return groupedQuery.Where(o => o.Name.ToLower().Contains(searchTerm.ToLower())).OrderBy(o => o.Name).ToDictionary(o => o.ObjectId, o => o.Name);
            }

            return groupedQuery.OrderBy(o => o.Name).ToDictionary(o => o.ObjectId, o => o.Name);
        }

        public List<ParentValueAttribute> GetParentValueAttributes()
        {
            MapperConfiguration config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<GetValuesAttributeResult, ParentValueAttribute>()
                .ForMember(dest => dest.ParentId, opt => opt.MapFrom(src => src.ValueId))
                .ForMember(dest => dest.AttributeValue, opt => opt.MapFrom(src => src.AttributeValue))
                .ForMember(dest => dest.ElementType, opt => opt.MapFrom(src => src.ElementType))
                .ForMember(dest => dest.Color, opt => opt.MapFrom(src => GetColorOfElementType(src.ElementType)))
                .ForMember(dest => dest.ElementName, opt => opt.MapFrom(src => src.ElementName));

            });
            Mapper = config.CreateMapper();
            List<GetValuesAttributeResult> parentsAttributes = DbContext.GetValuesAttribute().ToList();
            List<ParentValueAttribute> parentsValueAttributes = new List<ParentValueAttribute>();
            Mapper.Map(parentsAttributes, parentsValueAttributes);
            return parentsValueAttributes;
        }

        private string GetColorOfElementType(int? elementType)
        {
            if (elementType == null)
            {
                return string.Empty;
            }

            switch (elementType)
            {
                case (int)AttributeElementType.BU:
                    return "red";
                case (int)AttributeElementType.SUBBU:
                    return "green";
                case (int)AttributeElementType.PORTFOLIO:
                    return "blue";
                case (int)AttributeElementType.INDICATION:
                    return "black";
                default:
                    break;
            }
            return string.Empty;
        }
    }

    [DataContract]
    public class MappingInfo
    {
        [DataMember]
        public string Value { get; set; }
        [DataMember]
        public List<Item> MappedTo { get; set; }
        [DataMember]
        public int Count { get; set; }
        [DataMember]
        public int CampaignCount { get; set; }

        public MappingInfo(string value, int count, List<Item> mappedTo, int campaignCount)
        {
            if (mappedTo == null)
            {
                throw new ArgumentNullException(nameof(mappedTo));
            }

            Value = value;
            Count = count;
            CampaignCount = campaignCount;
            MappedTo = mappedTo.OrderBy(o => o.Name).ToList();
        }

        [DataContract]
        public class Item
        {
            [DataMember(Name = Consts.OBJECT_ID_SERIALIZED_NAME)]
            public Guid ObjectId { get; set; }
            [DataMember]
            public string Name { get; set; }

            public Item(Guid objectId, string name)
            {
                Name = name;
                ObjectId = objectId;
            }
        }
    }
}
