using AutoMapper;
using CMT.BL.Core;
using CMT.BO;
using CMT.BO.Admin;
using CMT.Common;
using CMT.DL;
using CMT.PV.Security;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects.DataClasses;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CMT.BL
{
    public class SchemaManager : BaseObjectManager<CmtEntities, MetadataSchema, SchemaBO>
    {
        private UserStore UserStore { get; }

        public SchemaManager()
            : base()
        {
            UserStore = new UserStore(DbContext);

        }
        public IEnumerable<SchemaBO> GetObjectsByCountry(string countryCode, Guid? userId = null)
        {
            List<string> errors = new List<string>();

            if (string.IsNullOrWhiteSpace(countryCode))
            {
                errors.Add("CountryCode must be specified");
            }

            if (!DbContext.Countries.Any((o) => o.Code.ToLower() == countryCode.ToLower()))
            {
                errors.Add(string.Format("Country with code [{0}] does not exists", countryCode));
            }

            if (errors.Any())
            {
                throw new ValidationException(errors);
            }


            try
            {
                MapperConfiguration config = new MapperConfiguration(cfg =>
                {


                    cfg.CreateMap<MetadataSchema, SchemaBO>().ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.SchemaTags.Select(p => p.Name)));



                });
                Mapper = config.CreateMapper();



                List<MetadataSchema> schemas = (from s in DbQueryable
                               .Include(p => p.Countries)
                                                where s.Countries.Any(c => c.Code == countryCode)
                                                && (userId.HasValue ? s.DataRanges.SelectMany(p => p.Application.Users).Any(p => p.ObjectId == userId.Value) : true)
                                                select s).ToList();

                // var sc = this.ConvertToBusinessObject(schemas);
                List<SchemaBO> retValue = new List<SchemaBO>();
                foreach (MetadataSchema sc in schemas)
                {
                    retValue.Add(ConvertToBusinessObject(sc));
                }

                InitializeMapper();
                return retValue;
            }
            catch
            {
                throw;
            }
            finally
            {
                InitializeMapper();
            }


        }

        public List<ThirdPartySystem> GetThirdPartySystems(Guid schemaId)
        {
            List<ThirdPartySystem> result = (from s in DbContext.Applications
                                             join dr in DbContext.DataRanges.Where((p) => p.SchemaId == schemaId) on s.ObjectId equals dr.ApplicationId into j
                                             from data in j.DefaultIfEmpty()
                                             where s.Attributes.HasValue
                                             && (s.Attributes.Value & (byte)ApplicationAttributes.IsThirdPartyApp) == (byte)ApplicationAttributes.IsThirdPartyApp
                                             select new ThirdPartySystem()
                                             {
                                                 Id = s.ObjectId,
                                                 RelatedObjectId = schemaId,
                                                 IsAllowed = data != null,
                                                 Name = s.Name
                                             }).ToList();
            return result;
        }

        public async Task<bool> UpdateThirdPartySystemsAccess(ThirdPartySystem system)
        {
            try
            {
                if (system.IsAllowed)
                {
                    DataRange dr = new DataRange() { SchemaId = system.RelatedObjectId, ApplicationId = system.Id };
                    DbContext.DataRanges.Add(dr);
                    await DbContext.SaveChangesAsync();
                }
                else
                {
                    DataRange obj = await DbContext.DataRanges.SingleOrDefaultAsync((p) => p.ApplicationId == system.Id && p.SchemaId == system.RelatedObjectId);
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

        public IEnumerable<SchemaBO> GetAllActiveObjects(Guid? userId = null)
        {
            try
            {
                MapperConfiguration config = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<MetadataSchema, SchemaBO>().ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.SchemaTags.Select(p => p.Name)));
                });
                Mapper = config.CreateMapper();

                List<MetadataSchema> schemas = (from s in base.DbQueryable
                                                    //   .Include(p => p.Countries).Include(p => p.SchemaTags)
                                                join dr in DbContext.DataRanges on s.ObjectId equals dr.SchemaId
                                                join u in DbContext.Users on dr.ApplicationId equals u.ApplicationId
                                                where u.ObjectId == userId

                                                select s).ToList();

                List<SchemaBO> retValue = new List<SchemaBO>();
                foreach (MetadataSchema sc in schemas)
                {
                    retValue.Add(ConvertToBusinessObject(sc));
                }

                InitializeMapper();
                return retValue;
            }
            catch
            {
                throw;
            }
            finally
            {
                InitializeMapper();
            }
        }

        private Func<Guid, EntityCollection<DataRange>, bool> IsCoveredByDataRange = (u, dr) => dr.SelectMany(d => d.Application.Users).Any(au => au.ObjectId == u);

        public async Task<SchemaBO> GetSchemaDefinition(Guid objectId, Guid userId)
        {
            try
            {
                MapperConfiguration config = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<ValueDetail, ValueDetailBO>();
                    cfg.CreateMap<Value, ValueBO>().ForMember(dest => dest.ChildValues, opt => opt.MapFrom(src => src.ChildList.Values))
                                                   .ForMember(dest => dest.Details, opt => opt.MapFrom(src => src.ValueDetails))
                                                   .ForMember(dest => dest.ParentValue, opt => opt.MapFrom(src => src.ParentValue))
                                                   ;

                    cfg.CreateMap<MetadataElement, ElementBO>().ForMember(dest => dest.ValueList, opt => opt.MapFrom(src => src.ValueList.Values));
                    cfg.CreateMap<MetadataSchema, SchemaBO>().ForMember(dest => dest.SchemaElements, opt => opt.MapFrom(src => src.MetadataSchemaElements))
                    .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.SchemaTags.Select(p => p.Name)));
                    cfg.CreateMap<MetadataSchemaElement, SchemaElementBO>().ForMember(dest => dest.Element, opt => opt.MapFrom(src => src.MetadataElement));

                });
                Mapper = config.CreateMapper();




                MetadataSchema schema = await GetMetadataSchema(objectId, userId);


                if (schema == null)
                {

                    List<string> errors = new List<string>();
                    errors.Add("Schema is not accesible");
                    throw new ValidationException(errors);
                }

                List<Guid> elements = (from e in DbContext.MetadataSchemaElements
                                       join dr in DbContext.DataRanges on e.ElementId equals dr.ElementId
                                       join u in DbContext.Users on dr.ApplicationId equals u.ApplicationId
                                       where e.SchemaId == objectId && u.ObjectId == userId
                                       select e.ObjectId).ToList();


                SchemaBO returnValue = ConvertToBusinessObject(schema);

                FillValueTags(returnValue.SchemaElements);

                returnValue.SchemaElements = (from e in returnValue.SchemaElements
                                              join dre in elements on e.ObjectId equals dre
                                              select e).ToList();


                InitializeMapper();
                return returnValue;


            }
            catch
            {
                throw;
            }
            finally
            {
                InitializeMapper();
            }
        }


        public async Task<SchemaBO> GetSchemaDefinitionByCountry(string countryCode, Guid objectId, Guid userId)
        {
            try
            {
                MapperConfiguration config = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<ValueDetail, ValueDetailBO>();

                    cfg.CreateMap<Value, ValueBO>().ForMember(dest => dest.ChildValues, opt => opt.MapFrom(src => src.ChildList.Values))
                                                   .ForMember(dest => dest.Details, opt => opt.MapFrom(src => src.ValueDetails))
                                                   .ForMember(dest => dest.ParentValue, opt => opt.MapFrom(src => src.ParentValue));

                    cfg.CreateMap<MetadataElement, ElementBO>().ForMember(dest => dest.ValueList, opt => opt.MapFrom(src => src.ValueList.Values));
                    cfg.CreateMap<MetadataSchema, SchemaBO>().ForMember(dest => dest.SchemaElements, opt => opt.MapFrom(src => src.MetadataSchemaElements))
                    .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.SchemaTags.Select(p => p.Name)));
                    cfg.CreateMap<MetadataSchemaElement, SchemaElementBO>().ForMember(dest => dest.Element, opt => opt.MapFrom(src => src.MetadataElement));

                });
                Mapper = config.CreateMapper();
                MetadataSchema schema = await GetMetadataSchema(objectId, userId, countryCode);

                if (schema == null)
                {
                    List<string> errors = new List<string>();
                    errors.Add("Schema is not accesible");
                    throw new ValidationException(errors);
                }

                List<Guid> elements = (from e in DbContext.MetadataSchemaElements
                                       join dr in DbContext.DataRanges on e.ElementId equals dr.ElementId
                                       join u in DbContext.Users on dr.ApplicationId equals u.ApplicationId
                                       where e.SchemaId == objectId && u.ObjectId == userId
                                       select e.ObjectId).ToList();


                SchemaBO returnValue = ConvertToBusinessObject(schema);
                List<SchemaElementBO> elementList = (from e in returnValue.SchemaElements
                                                     join dre in elements on e.ObjectId equals dre
                                                     select e).ToList();
                Guid countryid = (from country in DbContext.Countries
                                  where country.Code == countryCode
                                  select country.ObjectId).FirstOrDefault();

                foreach (SchemaElementBO el in elementList)
                {
                    if (el.Element.ValueList != null)
                    {
                        el.Element.ValueList = el.Element.ValueList.Where(v => v.CountryId == null || v.CountryId == countryid).ToList();
                    }

                }


                FillValueTags(elementList);

                returnValue.SchemaElements = elementList;


                InitializeMapper();
                return returnValue;


            }
            catch
            {
                throw;
            }
            finally
            {
                InitializeMapper();
            }
        }

        private async Task<MetadataSchema> GetMetadataSchema(Guid objectId, Guid userId, string countryCode = null)
        {
            MetadataSchema schema;
            if (await UserStore.IsInRoleAsync(userId, UserStore.AdminRoleName))
            {
                schema = DbQueryable.FirstOrDefault(p => p.ObjectId == objectId);
            }
            else
            {
                schema = (from s in base.DbQueryable
                          join dr in DbContext.DataRanges on s.ObjectId equals dr.SchemaId
                          join u in DbContext.Users on dr.ApplicationId equals u.ApplicationId
                          where s.ObjectId == objectId && u.ObjectId == userId && (countryCode == null || s.Countries.Any((co) => co.Code == countryCode))
                          select s).FirstOrDefault();
            }

            return schema;
        }

        private void FillValueTags(List<SchemaElementBO> elementList)
        {
            MapperConfiguration config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ValueTag, ValueTagBO>()

                .ForMember(dest => dest.SchemaTag, opt => opt.MapFrom(src => src.SchemaTag))
                .ForMember(dest => dest.TagElementId, opt => opt.MapFrom(src => src.TagElementId))
                .ForMember(dest => dest.TagValueId, opt => opt.MapFrom(src => src.TagValueId))
                .ForMember(dest => dest.Element, opt => opt.MapFrom(src => src.MetadataElement.Name))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value_TagValueId.TextValue))
                .ForMember(dest => dest.CountryCode, opt => opt.MapFrom(src => src.Country.Code));
            });
            IMapper Mapper = config.CreateMapper();

            IEnumerable<ValueTagBO> valueTagBOs = DbContext.ValueTags
                .Include(p => p.MetadataElement)
                .Include(p => p.Value_TagValueId)
                .Include(p => p.Country)
                .AsEnumerable()
                .Select(vt => Mapper.Map<ValueTag, ValueTagBO>(vt))
                .ToList();

            foreach (SchemaElementBO schemaElement in elementList)
            {
                FillValueTagRecursive(valueTagBOs, schemaElement.Element.ValueList);
            }
        }

        private static void FillValueTagRecursive(IEnumerable<ValueTagBO> valueTagBOs, List<ValueBO> valueList)
        {
            foreach (ValueBO value in valueList.Where(p => valueTagBOs.Any(v => v.ValueId == p.ObjectId) || p.ChildValues.Any()))
            {
                List<ValueTagBO> valueTags = valueTagBOs.Where(p => p.ValueId == value.ObjectId).ToList();
                value.Tags = valueTags;
                if (value.ChildValues.Any())
                {
                    FillValueTagRecursive(valueTagBOs, value.ChildValues);
                }
            }
        }

        public List<CountryBO> GetCountries(Guid schemaId)
        {
            ICollection<Country> countries = (from s in DbQueryable
                                              where s.ObjectId == schemaId
                                              select s.Countries).Single();
            return countries.OrderBy(p => p.Name).Select(p => new CountryBO()
            {
                ObjectId = p.ObjectId,
                Name = p.Name
            }).ToList();
        }

        public Guid AddElementToSchema(Guid schemaId, Guid elementId)
        {
            MetadataElement sourceElement = DbContext.MetadataElements.Single((p) => p.ObjectId == elementId);
            MetadataSchemaElement element = new MetadataSchemaElement()
            {
                SchemaId = schemaId,
                ElementId = elementId,
                DefaultValue = sourceElement.DefaultValue
            };
            DbContext.MetadataSchemaElements.Add(element);
            DbContext.SaveChanges();
            return element.ObjectId;
        }

        public void DeleteElementFromSchema(Guid schemaId, Guid elementId)
        {
            MetadataSchemaElement element = DbContext.MetadataSchemaElements
                .Single((p) => p.SchemaId == schemaId && p.ElementId == elementId);
            DbContext.MetadataSchemaElements.Remove(element);
            DbContext.SaveChanges();
        }

        public void SetSchemaElementProperties(Guid schemaId, Guid elementId, SchemaElementBO properties)
        {
            MetadataSchemaElement se = DbContext.MetadataSchemaElements.Single((p) => p.SchemaId == schemaId
            && p.ElementId == elementId);
            se.IsRequired = properties.IsRequired;
            se.DefaultValue = properties.DefaultValue;
            DbContext.SaveChanges();
        }

        public List<SchemaBO> GetSchemasByChannelByCountry(Guid channelId, string countryCode)
        {
            IQueryable<MetadataSchema> schemas = (from s in DbQueryable
                                                  where s.ChannelId == channelId
                                                  && s.Countries.Any(p => p.Code == countryCode)
                                                  && s.IsActive
                                                  select s);
            return ConvertToBusinessObjects(schemas).ToList();
        }

        public List<Item> GetSchemaElementsByImportId(Guid importId)
        {
            var import = (from i in DbContext.Imports
                          where i.ObjectId == importId
                          select new { Import = i, Schema = i.MetadataSchema }).FirstOrDefault();
            if (import == null)
            {
                throw new Exception("Import not found");
            }

            Guid schemaId = import.Schema.ObjectId;

            var result = (from s in DbContext.MetadataSchemaElements
                          join i in DbContext.Imports on s.SchemaId equals i.SchemaId
                          where s.SchemaId == schemaId && i.ObjectId == importId
                          select new
                          {
                              Id = s.ObjectId,
                              Label = s.MetadataElement.Name,
                          }).ToList();

            return result.Select(o => new Item(o.Id, o.Label)).ToList();
        }



        public IEnumerable<Option> GetElementLevels(Guid elementId)
        {

            var query = from e in DbContext.MetadataElements
                        from vll in DbContext.ValueListLevels.Where((o) => o.ElementId == e.ObjectId).DefaultIfEmpty()
                        where e.ObjectId == elementId
                        select new
                        {
                            Level = vll.Level,
                            LevelName = vll.Name,
                            ElementName = e.Name
                        };


            List<Option> retValue = new List<Option>();
            int totalCount = query.Count();
            foreach (var vl in query)
            {
                if (totalCount > 1)
                {
                    retValue.Add(new Option(string.Format("{0} - {1}", vl.ElementName, vl.LevelName), (int)vl.Level));
                }
                else
                {
                    retValue.Add(new Option(vl.ElementName, (int)vl.Level));
                }
            }

            return retValue;

        }




        public List<SchemaElementData> GetSchemaElementsByImportId(Guid importId, Guid? schemaElementId, string searchTerm)
        {
            var import = (from i in DbContext.Imports
                          where i.ObjectId == importId
                          select new { Import = i, Schema = i.MetadataSchema }).FirstOrDefault();
            if (import == null)
            {
                throw new Exception("Import not found");
            }

            Guid lovTypeId = (from t in DbContext.ElementTypes
                              where t.Name.ToLower() == "lov"
                              select t.ObjectId).SingleOrDefault();

            Guid schemaId = import.Schema.ObjectId;

            List<SchemaElementData> result = new List<SchemaElementData>();

            IQueryable<Guid> augments = (from cv in DbContext.CampaignValues
                                         join ci in DbContext.CampaignImports on cv.CampaignId equals ci.CampaignId
                                         join se in DbContext.MetadataSchemaElements on cv.SchemaElementId equals se.ObjectId

                                         from sv in DbContext.SourceValues.Where((o) => o.CampaignValueId == cv.ObjectId).DefaultIfEmpty()
                                         where ci.ImportId == importId &&
                                               cv.ValueId != null &&
                                               cv.Value == null &&
                                               sv == null
                                         select se.ElementId).Distinct();

            if (import.Import.CampaignColumnNameHash != null && !import.Import.CampaignColumnNameHash.SequenceEqual(new byte[20]) && importId != schemaElementId)
            {
                var query = (from s in DbContext.MetadataSchemaElements
                             join e in DbContext.MetadataElements on s.ElementId equals e.ObjectId
                             from vll in DbContext.ValueListLevels.Where((o) => o.ElementId == e.ObjectId).DefaultIfEmpty()
                             from sc in DbContext.ColumnHeaders.Where((o) => o.SchemaElementId == s.ObjectId && o.ImportId == importId && (schemaElementId == null || o.SchemaElementId != schemaElementId)).DefaultIfEmpty()
                             join i in DbContext.Imports on s.SchemaId equals i.SchemaId
                             where s.SchemaId == schemaId &&
                                   i.ObjectId == importId &&
                                   e.TypeId == lovTypeId &&
                                   sc == null
                             select new
                             {
                                 Id = s.ObjectId,
                                 e.Name,
                                 Level = vll != null ? vll.Level : -1,
                                 LevelName = vll != null ? vll.Name : null,
                                 FullName = vll != null ? e.Name + " - " + vll.Name : e.Name,
                                 ElementId = e.ObjectId
                             }).ToList().Where((o) => !augments.Contains(o.ElementId) || o.Id == schemaElementId).ToList();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    result = query.OrderBy(o => o.Name).ThenBy(o => o.Level)
                        .Where(o => o.FullName.ToLower().Contains(searchTerm.ToLower()))
                        .Select(o => new SchemaElementData(o.Id, o.Name, o.Level > 0 ? o.Level : (int?)null, o.LevelName, query.Count(a => a.Id == o.Id) == 1)).ToList();
                }
                else
                {
                    result = query.OrderBy(o => o.Name).ThenBy(o => o.Level).Select(o => new SchemaElementData(o.Id, o.Name, o.Level > 0 ? o.Level : (int?)null, o.LevelName, query.Count(a => a.Id == o.Id) == 1)).ToList();
                }
            }

            if (schemaElementId != null && importId != schemaElementId)
            {

                Guid elementId = DbContext.MetadataSchemaElements.Where((o) => o.ObjectId == schemaElementId).Select((o) => o.ElementId).FirstOrDefault();
                if (!augments.Contains(elementId))
                {
                    byte? level = (from sc in DbContext.ColumnHeaders
                                   where sc.ImportId == importId &&
                                         sc.SchemaElementId == schemaElementId
                                   select sc.Level).FirstOrDefault();

                    if (level.HasValue)
                    {
                        SchemaElementData currentSchemaElementData = result.SingleOrDefault(o => o.Id == schemaElementId && o.Level == level);

                        if (currentSchemaElementData != null)
                        {
                            result.Remove(currentSchemaElementData);
                            result.Insert(0, currentSchemaElementData);
                        }
                    }
                }
                else
                {
                    int? level = (from cv in DbContext.CampaignValues
                                  join ci in DbContext.CampaignImports on cv.CampaignId equals ci.CampaignId
                                  join se in DbContext.MetadataSchemaElements on cv.SchemaElementId equals se.ObjectId
                                  from sv in DbContext.SourceValues.Where((o) => o.CampaignValueId == cv.ObjectId).DefaultIfEmpty()
                                  join vvt in DbContext.ViewValueTrees on cv.ValueId equals vvt.ValueId
                                  where ci.ImportId == importId &&
                                        cv.ValueId != null &&
                                        cv.Value == null &&
                                        sv == null &&
                                        se.ElementId == elementId
                                  select vvt.Level).First();

                    SchemaElementData currentSchemaElementData = result.SingleOrDefault(o => o.Id == schemaElementId && o.Level == level);

                    if (currentSchemaElementData != null)
                    {
                        result.Remove(currentSchemaElementData);
                        result.Insert(0, currentSchemaElementData);
                    }

                    List<SchemaElementData> schemaElementDatasToRemove = result.Where(o => o.Id == schemaElementId && o.Level != level).ToList();

                    foreach (SchemaElementData item in schemaElementDatasToRemove)
                    {
                        result.Remove(item);
                    }
                }
            }

            string sourceId = "Unique ID";

            if (import.Import.CampaignColumnNameHash == null || import.Import.CampaignColumnNameHash.SequenceEqual(new byte[20]) || schemaElementId == importId)
            {
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    if (sourceId.ToLower().Contains(searchTerm.ToLower()))
                    {
                        result.Insert(0, new SchemaElementData(importId, sourceId, null, null, true));
                    }
                }
                else
                {
                    result.Insert(0, new SchemaElementData(importId, sourceId, null, null, true));
                }
            }

            return result;
        }

        public List<Item> GetSchemaElementsByCampaignAndImportId(Guid campaignId, Guid? importId)
        {
            var elements = (from c in DbContext.CampaignValues
                            where c.CampaignId == campaignId
                            select new { Import = c.Import, Schema = c.MetadataSchemaElement.Schema }).FirstOrDefault();
            if (elements == null)
            {
                throw new Exception("Campaign not found");
            }

            Guid schemaId = elements.Schema.ObjectId;

            var result = (from s in DbContext.MetadataSchemaElements
                          join i in DbContext.Imports on s.SchemaId equals i.SchemaId
                          where s.SchemaId == schemaId && i.ObjectId == importId
                          select new
                          {
                              Id = s.ObjectId,
                              Label = s.MetadataElement.Name,
                          }).ToList();

            return result.Select(o => new Item(o.Id, o.Label)).ToList();
        }

        public SchemaBO GetSchema(Guid countryId, Guid channelId)
        {
            MetadataSchema query = (from o in DbContext.MetadataSchemata
                                    where o.ChannelId == channelId &&
                                          o.Countries.Any((p) => p.ObjectId == countryId) && o.IsActive
                                    select o).Single();

            return ConvertToBusinessObject(query);
        }

        public Dictionary<string, Stream> GetSchemaElementExtracts(DateTime now, Guid? schemaId = null, Guid? countryId = null)
        {
            Dictionary<string, Stream> result = new Dictionary<string, Stream>();

            List<Country> query = DbContext.Countries.Where((o) => countryId == null || o.ObjectId == countryId.Value).Include((o) => o.MetadataSchemata1).ToList();
            foreach (Country country in query)
            {
                StreamWriter writer = new StreamWriter(new MemoryStream());
                writer.WriteLine("SchemaId|ElementId|ElementName|IsActive|DefaultValue|IsRequired");
                foreach (MetadataSchemaElement item in country.MetadataSchemata1.Where(s => schemaId == null || s.ObjectId == schemaId.Value).SelectMany(p => p.MetadataSchemaElements))
                {
                    writer.WriteLine(string.Format("{0}|{1}|{2}|{3}|{4}|{5}"
                        , item.SchemaId                             //0
                        , item.ElementId                            //1
                        , item.MetadataElement.Name.WrapToExport()  //2
                        , item.Schema.IsActive                      //3
                        , item.DefaultValue                      //4
                         , item.IsRequired
                    ));
                }
                writer.Flush();
                result.Add(string.Format("{0}_CMT_{2}_Schema_Element_{1}.txt", country.Code, now.ToString("yyyyMMdd_HHmmss"), ApplicationSettings.DocumentationVersion), writer.BaseStream);
            }
            return result;
        }

        public Dictionary<string, Stream> GetSchemaExtracts(DateTime now, Guid? schemaId = null, Guid? countryId = null)
        {
            Dictionary<string, Stream> result = new Dictionary<string, Stream>();

            List<Country> query = DbContext.Countries.Where((o) => countryId == null || o.ObjectId == countryId.Value).Include((o) => o.MetadataSchemata1).ToList();

            foreach (Country country in query)
            {
                StreamWriter writer = new StreamWriter(new MemoryStream());
                writer.WriteLine("SchemaId|SchemaName|SchemaDescription|SchemaActivatedBy|SchemaActivationDate|SchemaDefinedBy|SchemaDefinitionDate|ChannelId|IsActive|Tags|CreateUser|CreateDate|ChangeUser|ChangeDate");

                foreach (MetadataSchema metadataSchema in country.MetadataSchemata1.Where(s => schemaId == null || s.ObjectId == schemaId.Value))
                {
                    writer.WriteLine(string.Format("{0}|{1}|{2}|{3}|{4:yyyyMMdd_HHmmss}|{5}|{6:yyyyMMdd_HHmmss}|{7}|{8}|\"{9}\"|{10}|{11:yyyyMMdd_HHmmss}|{12}|{13:yyyyMMdd_HHmmss}"
                        , metadataSchema.ObjectId       //0
                        , metadataSchema.Name.WrapToExport()           //1
                        , metadataSchema.Description.WrapToExport()    //2
                        , metadataSchema.ActivatedBy.WrapToExport()    //3
                        , metadataSchema.ActivationDate //4
                        , metadataSchema.DefinedBy.WrapToExport()      //5
                        , metadataSchema.DefinitionDate //6
                        , metadataSchema.ChannelId      //7
                        , metadataSchema.IsActive       //8
                        , string.Join(",", metadataSchema.SchemaTags.Select(p => p.Name))       //9
                        , metadataSchema.CreateUser.WrapToExport()     //10
                        , metadataSchema.CreateDate     //11
                        , metadataSchema.ChangeUser.WrapToExport()     //12
                        , metadataSchema.ChangeDate     //13
                    ));
                }

                writer.Flush();
                result.Add(string.Format("{0}_CMT_{2}_Schema_{1}.txt", country.Code, now.ToString("yyyyMMdd_HHmmss"), ApplicationSettings.DocumentationVersion), writer.BaseStream);
            }

            return result;
        }

        public Guid CreateSchema(SchemaBO schema)
        {
            if (!schema.SourceSchemaId.HasValue)
            {
                InsertObject(schema);
                MetadataSchema schemaDbObject = DbQueryable.Include(b => b.Countries).Single(s => s.ObjectId == schema.ObjectId);
                IQueryable<Country> countries = (from c in DbContext.Countries
                                                 select c);
                foreach (Country c in countries)
                {
                    schemaDbObject.Countries.Add(c);
                }

                DbContext.SaveChanges();
                return schema.ObjectId;
            }
            else
            {
                System.Data.Entity.Core.Objects.ObjectResult<CloneSchemaResult> result = DbContext.CloneSchema(schema.SourceSchemaId, schema.Name, schema.ChannelId, schema.DefinedBy);
                return result.Single().SchemaId.Value;
            }
        }

        public void DeleteSchema(Guid objectId)
        {
            MetadataSchema schema = DbQueryable.Single(p => p.ObjectId == objectId);
            List<object> objectsToDelete = new List<object>();
            schema.Countries.Clear();
            foreach (MetadataSchemaElement e in schema.MetadataSchemaElements.ToList())
            {
                DbContext.MetadataSchemaElements.Remove(e);
            }

            foreach (DataRange d in schema.DataRanges.ToList())
            {
                DbContext.DataRanges.Remove(d);
            }

            DbQueryable.Remove(schema);
            DbContext.SaveChanges();
        }

        public void AddCountry(Guid schemaId, Guid countryId)
        {
            MetadataSchema schemaDbObject = DbQueryable.Include(b => b.Countries).Single(p => p.ObjectId == schemaId);
            if (schemaDbObject.Countries.Any(p => p.ObjectId == countryId))
            {
                return;
            }

            Country country = (from c in DbContext.Countries
                               where c.ObjectId == countryId
                               select c).Single();
            schemaDbObject.Countries.Add(country);
            DbContext.SaveChanges();
        }

        public void DeleteCountry(Guid schemaId, Guid countryId)
        {
            MetadataSchema schemaDbObject = DbQueryable.Single(p => p.ObjectId == schemaId);
            if (!schemaDbObject.Countries.Any(p => p.ObjectId == countryId))
            {
                return;
            }

            Country country = (from c in DbContext.Countries
                               where c.ObjectId == countryId
                               select c).Single();
            schemaDbObject.Countries.Remove(country);
            DbContext.SaveChanges();
        }

        public void AddTag(Guid schemaId, string tagName)
        {
            MetadataSchema schemaDbObject = DbQueryable.Include(b => b.SchemaTags).Single(p => p.ObjectId == schemaId);
            if (schemaDbObject.SchemaTags.Any(p => p.Name == tagName))
            {
                return;
            }

            SchemaTag tag = new SchemaTag()
            {
                Name = tagName,
                SchemaId = schemaId
            };
            schemaDbObject.SchemaTags.Add(tag);
            DbContext.SaveChanges();
        }

        public void DeleteTag(Guid schemaId, Guid tagId)
        {
            MetadataSchema schemaDbObject = DbQueryable.Single(p => p.ObjectId == schemaId);
            if (!schemaDbObject.SchemaTags.Any(p => p.ObjectId == tagId))
            {
                return;
            }

            SchemaTag tag = (from c in DbContext.SchemaTags
                             where c.ObjectId == tagId
                             select c).Single();
            DbContext.SchemaTags.Remove(tag);
            DbContext.SaveChanges();
        }

        public Dictionary<Guid, string> GetSchemaTags(Guid? schemaId)
        {
            Dictionary<Guid, string> schemaTags = (from s in DbContext.SchemaTags
                                                   where !schemaId.HasValue || s.SchemaId == schemaId
                                                   select new { s.ObjectId, s.Name }).OrderBy((p) => p.Name).ToDictionary(k => k.ObjectId, v => v.Name);
            if (!schemaId.HasValue)
            {
                schemaTags = schemaTags.GroupBy(p => p.Value).ToDictionary(k => k.First().Key, v => v.Key);
            }

            return schemaTags;
        }
    }
}
