using CMT.Attributes;
using CMT.BL;
using CMT.BL.Managers;
using CMT.BO;
using CMT.BO.Admin;
using CMT.Helpers;
using CMT.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Results;

namespace CMT.Controllers
{
    [CMTAuthorize]

    [EnableCors("*", "Origin, Content-Type, Accept, Authorization, Cache-control, Pragma, Expires",
                                               "GET, PUT, POST, DELETE, OPTIONS", SupportsCredentials = true)]
    public class AdminSchemaController : BaseApiController
    {
        private JsonSerializerSettings SerializationSettings { get; } = new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Ignore };
        /// <summary>
        /// Gets the schema tree.
        /// </summary>
        /// <param name="countryCode">The country code.</param>
        /// <param name="searchTerm">The search term.</param>
        /// <returns></returns>
        [Route("api/AdminSchema/GetSchemaTree/{countryCode}/search/{searchTerm?}")]
        public IEnumerable<TreeElementBO> GetSchemaTree(string countryCode, string searchTerm = null)
        {
            using (ViewSchemaTreeManager manager = new ViewSchemaTreeManager())
            {
                List<TreeElementBO> result = manager.GetSchemaTree(countryCode, searchTerm);
                return result;
            }
        }

        [Route("api/AdminSchema/GetSchemas/{countryCode}/{searchString?}")]
        public IEnumerable<TreeElementBO> GetSchemaList(string countryCode, string searchString = null)
        {
            using (ViewSchemaTreeManager manager = new ViewSchemaTreeManager())
            {
                List<TreeElementBO> result = manager.GetSchemaList(countryCode, searchString);
                return result;
            }
        }

        [Route("api/AdminSchema/GetSchemaTree/{countryCode}/{schema}")]
        public JsonResult<List<TreeElementBO>> GetSchemaTree(string countryCode, Guid schema)
        {
            using (ViewSingleSchemaTreeManager manager = new ViewSingleSchemaTreeManager())
            {
                List<TreeElementBO> result = manager.GetSchemaTree(countryCode, schema);
                return Json(result, SerializationSettings);
            }
        }

        /// <summary>
        /// Gets the third party systems access by element identifier.
        /// </summary>
        /// <param name="schemaId">The schema identifier.</param>
        /// <returns></returns>
        [Route("api/AdminSchema/GetThirdPartySystemsAccessBySchemaId/{schemaId}")]
        public List<ThirdPartySystem> GetThirdPartySystemsAccessByElementId(Guid schemaId)
        {
            using (SchemaManager schemaManager = new SchemaManager())
            {
                return schemaManager.GetThirdPartySystems(schemaId);
            }

        }

        /// <summary>
        /// Updates the third party systems access.
        /// </summary>
        /// <param name="system">The system.</param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException"></exception>
        [Route("api/AdminSchema/UpdateThirdPartySystemsAccess")]
        public async Task<ApiResponse<bool>> UpdateThirdPartySystemsAccess(ThirdPartySystem system)
        {
            using (SchemaManager schemaManager = new SchemaManager())
            {
                if (await schemaManager.UpdateThirdPartySystemsAccess(system))
                {
                    return new ApiResponse<bool>(true, null);
                }
                else
                {
                    ApiResponse<string> retValue = new ApiResponse<string>(true, new List<string> { "Unable to change third party system access" });
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.InternalServerError, retValue);
                    throw new HttpResponseException(response);
                }
            }

        }

        /// <summary>
        /// Updates the schema description.
        /// </summary>
        /// <param name="schema">The schema.</param>
        [HttpPost]
        [Route("api/AdminSchema/UpdateSchemaDescription")]
        public void UpdateSchemaDescription(SchemaBO schema)
        {
            using (SchemaManager schemaManager = new SchemaManager())
            {
                SchemaBO schemaFromDb = schemaManager.GetObject(schema.ObjectId);
                schemaFromDb.Description = schema.Description;
                schemaManager.UpdateObject(schemaFromDb);
            }
        }

        /// <summary>
        /// Creates the schema.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/AdminSchema/CreateSchema")]
        public Guid CreateSchema(SchemaBO schema)
        {
            using (SchemaManager schemaManager = new SchemaManager())
            {
                schema.DefinedBy = User.Identity.Name;
                schema.DefinitionDate = DateTime.UtcNow;
                schema.IsActive = false;
                return schemaManager.CreateSchema(schema);
            }
        }

        /// <summary>
        /// Updates the schema elements.
        /// </summary>
        /// <param name="schemaId">The schema identifier.</param>
        /// <param name="elementIds">The element ids.</param>
        [HttpPost]
        [Route("api/AdminSchema/UpdateSchemaElements/{schemaId}")]
        public void UpdateSchemaElements(Guid schemaId, List<Guid> elementIds)
        {
            using (SchemaElementManager schemaElementManager = new SchemaElementManager())
            {
                schemaElementManager.UpdateSelectedSchemaElements(schemaId, elementIds);
            }
        }

        /// <summary>
        /// Deletes the schema.
        /// </summary>
        /// <param name="schemaId">The schema identifier.</param>
        [HttpPost]
        [Route("api/AdminSchema/DeleteSchema/{schemaId}")]
        public void DeleteSchema(Guid schemaId)
        {
            using (SchemaManager schemaManager = new SchemaManager())
            {
                schemaManager.DeleteSchema(schemaId);
            }
        }

        /// <summary>
        /// Activates the schema.
        /// </summary>
        /// <param name="schemaId">The schema identifier.</param>
        [HttpPost]
        [Route("api/AdminSchema/ActivateSchema/{schemaId}")]
        public void ActivateSchema(Guid schemaId)
        {
            using (SchemaManager schemaManager = new SchemaManager())
            {
                SchemaBO newSchema = schemaManager.GetObject(schemaId);
                Guid channelId = newSchema.ChannelId;
                newSchema.IsActive = true;
                newSchema.ActivatedBy = User.Identity.Name;
                newSchema.ActivationDate = DateTime.UtcNow;
                newSchema.DeactivatedBy = null;
                newSchema.DeactivationDate = null;

                schemaManager.UpdateObject(newSchema);

            }
        }

        /// <summary>
        /// Deactivates the schema.
        /// </summary>
        /// <param name="schemaId">The schema identifier.</param>
        [HttpPost]
        [Route("api/AdminSchema/DeactivateSchema/{schemaId}")]
        public void DeactivateSchema(Guid schemaId)
        {
            using (SchemaManager schemaManager = new SchemaManager())
            {
                SchemaBO schema = schemaManager.GetObject(schemaId);
                schema.IsActive = false;
                schema.DeactivatedBy = User.Identity.Name;
                schema.DeactivationDate = DateTime.UtcNow;
                schemaManager.UpdateObject(schema);
            }
        }

        /// <summary>
        /// Gets the list of channels.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/AdminSchema/GetChannels")]
        public List<ChannelBO> GetChannels()
        {
            using (ChannelManager channelManager = new ChannelManager())
            {
                return channelManager.GetObjects().OrderBy(p => p.Name).ToList();
            }
        }

        /// <summary>
        /// Gets the countries.
        /// </summary>
        /// <param name="schemaId">The schema identifier.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/AdminSchema/GetCountries/{schemaId?}")]
        public List<CountryBO> GetCountries(Guid? schemaId = null)
        {
            if (schemaId.HasValue)
            {
                using (SchemaManager schemaManager = new SchemaManager())
                {
                    return schemaManager.GetCountries(schemaId.Value).OrderBy(p => p.Name).ToList();
                }
            }
            else
            {
                using (CountryManager countryManager = new CountryManager())
                {
                    return countryManager.GetObjects().OrderBy(p => p.Name).ToList();
                }
            }
        }

        /// <summary>
        /// Adds schema to the country.
        /// </summary>
        /// <param name="schemaId">The schema identifier.</param>
        /// <param name="countryId">The country identifier.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/AdminSchema/AddCountry/{schemaId}/{countryId}")]
        public bool AddCountry(Guid schemaId, Guid countryId)
        {
            using (SchemaManager schemaManager = new SchemaManager())
            {
                try
                {
                    schemaManager.AddCountry(schemaId, countryId);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Deletes schema from the country.
        /// </summary>
        /// <param name="schemaId">The schema identifier.</param>
        /// <param name="countryId">The country identifier.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/AdminSchema/DeleteCountry/{schemaId}/{countryId}")]
        public bool DeleteCountry(Guid schemaId, Guid countryId)
        {
            using (SchemaManager schemaManager = new SchemaManager())
            {
                try
                {
                    schemaManager.DeleteCountry(schemaId, countryId);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Adds tag to schema.
        /// </summary>
        /// <param name="schemaId">The schema identifier.</param>
        /// <param name="countryId">The country identifier.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/AdminSchema/AddTag/{schemaId}")]
        public bool AddCountry(Guid schemaId, [FromBody]string tagName)
        {
            using (SchemaManager schemaManager = new SchemaManager())
            {
                try
                {
                    schemaManager.AddTag(schemaId, tagName);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Deletes tag from schema.
        /// </summary>
        /// <param name="schemaId">The schema identifier.</param>
        /// <param name="tagId">The tag identifier.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/AdminSchema/DeleteTag/{schemaId}/{tagId}")]
        public bool DeleteTag(Guid schemaId, Guid tagId)
        {
            using (SchemaManager schemaManager = new SchemaManager())
            {
                try
                {
                    schemaManager.DeleteTag(schemaId, tagId);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }


        /// <summary>
        /// Gets tags for all schemas
        /// </summary>
        /// <returns></returns>
        [Route("api/AdminSchema/GetTags")]
        public List<ItemModel> GetTags()
        {
            return GetTags(null);
        }

        /// <summary>
        /// Gets tags for schema
        /// </summary>
        /// <param name="schemaId">The schema identifier</param>
        /// <returns></returns>
        [Route("api/AdminSchema/GetTags/{schemaId}")]
        public List<ItemModel> GetTags(Guid? schemaId)
        {
            using (SchemaManager schemaManager = new SchemaManager())
            {
                return schemaManager.GetSchemaTags(schemaId).Select(i => new ItemModel(i.Value, i.Key)).ToList();
            }
        }

        /// <summary>
        /// Gets the schemas by channel identifier.
        /// </summary>
        /// <param name="channelId">The channel identifier.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/AdminSchema/GetSchemasByChannelId/{channelId}")]
        public List<SchemaBO> GetSchemasByChannelId(Guid channelId)
        {
            string countryCode = IdentityHelper.GetUserCountryCode(User.Identity);

            using (SchemaManager schemaManager = new SchemaManager())
            {
                return schemaManager.GetSchemasByChannelByCountry(channelId, countryCode);
            }
        }

        [HttpPost]
        [Route("api/AdminSchema/AddElementToSchema/{schemaId}/{elementId}")]
        public Guid AddElementToSchema(Guid schemaId, Guid elementId)
        {
            using (SchemaManager schemaManager = new SchemaManager())
            {
                return schemaManager.AddElementToSchema(schemaId, elementId);
            }
        }

        [HttpPost]
        [Route("api/AdminSchema/DeleteElementFromSchema/{schemaId}/{elementId}")]
        public void DeleteElementFromSchema(Guid schemaId, Guid elementId)
        {
            using (SchemaManager schemaManager = new SchemaManager())
            {
                schemaManager.DeleteElementFromSchema(schemaId, elementId);
            }
        }

        [HttpPost]
        [Route("api/AdminSchema/SetSchemaElementProperties/{schemaId}/{elementId}")]
        public void SetSchemaElementProperties(Guid schemaId, Guid elementId, SchemaElementBO properties)
        {
            using (SchemaManager schemaManager = new SchemaManager())
            {
                schemaManager.SetSchemaElementProperties(schemaId, elementId, properties);
            }
        }
    }
}
