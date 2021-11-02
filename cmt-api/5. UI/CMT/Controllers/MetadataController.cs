using CMT.Attributes;
using CMT.BL;
using CMT.BL.Core;
using CMT.BO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.ModelBinding;
using System.Web.Http.Results;

namespace CMT.Controllers
{
    [CMTAuthorize(AllowExternal = true)]
    public class MetadataController : BaseApiController
    {
        public ElementManager ElementManager { get; }

        public MetadataController(ElementManager elementManager)
        {
            ElementManager = elementManager;
        }

        protected override BadRequestResult BadRequest()
        {
            return base.BadRequest();
        }

        protected override InvalidModelStateResult BadRequest(ModelStateDictionary modelState)
        {
            return base.BadRequest(modelState);
        }


        protected override BadRequestErrorMessageResult BadRequest(string message)
        {
            return base.BadRequest(message);
        }
        /// <summary>
        /// Returns list of schemas per country
        /// </summary>
        /// <param name="countryCode">The country code.</param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException"></exception>

        [CheckModelForNull]
        [Route("api/metadata/country/{countryCode}")]
        public List<SchemaBO> GetByCountry(string countryCode)
        {

            try
            {
                using (SchemaManager manager = new SchemaManager())
                {
                    return manager.GetObjectsByCountry(countryCode, AuthenticatedUserId).ToList();
                }
            }
            catch (ValidationException ve)
            {

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.NotAcceptable, ve.Errors);
                throw new HttpResponseException(response);
            }


        }


        /// <summary>
        /// Gets the schema by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        [Route("api/metadata/{id}")]
        [CMTAuthorize(AllowExternal = true)]
        public async Task<SchemaBO> GetById(Guid id)
        {

            try
            {
                using (SchemaManager manager = new SchemaManager())
                {
                    return await manager.GetSchemaDefinition(id, AuthenticatedUserId);
                }
            }
            catch (ValidationException ve)
            {

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.NotAcceptable, ve.Errors);
                throw new HttpResponseException(response);
            }


        }

        /// <summary>
        /// Gets the schema by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        ///  <param name="countryCode">The code of country </param>
        /// <returns></returns>
        [Route("api/metadata/{countryCode}/{id}")]
        [CMTAuthorize(AllowExternal = true)]
        public async Task<SchemaBO> GetById(string countryCode, Guid id)
        {

            try
            {
                using (SchemaManager manager = new SchemaManager())
                {
                    return await manager.GetSchemaDefinitionByCountry(countryCode, id, AuthenticatedUserId);
                }
            }
            catch (ValidationException ve)
            {

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.NotAcceptable, ve.Errors);
                throw new HttpResponseException(response);
            }


        }


        /// <summary>
        /// Gets list of schemas.
        /// </summary>
        /// <returns></returns>
        [Route("api/metadata")]
        public IEnumerable<SchemaBO> Get()
        {
            using (SchemaManager manager = new SchemaManager())
            {
                return manager.GetAllActiveObjects(AuthenticatedUserId);
            }
        }

        /// <summary>
        /// Gets the element levels.
        /// </summary>
        /// <param name="elementId">The element identifier.</param>
        /// <returns></returns>
        [EnableCors("*", "*", "GET, OPTIONS", SupportsCredentials = true)]
        [Route("api/metadata/GetElementLevels")]
        public IEnumerable<Option> GetElementLevels(Guid elementId)
        {
            using (SchemaManager manager = new SchemaManager())
            {
                return manager.GetElementLevels(elementId);
            }
        }

        /// <summary>
        /// Gets the metadata elements by campaign and import.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <param name="schemaId">The schema identifier.</param>
        /// <param name="showIndirect">Show mapped elements outside of selected schema.</param>
        /// <param name="importId">The import identifier.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        [EnableCors("*", "*", "GET, OPTIONS", SupportsCredentials = true)]
        [Route("api/metadata/GetMetadataElements")]
        public DataTableResultModel<ElementBO> GetMetadataElementsByCampaignAndImport(Guid campaignId, Guid schemaId, bool showIndirect, Guid? importId = null, int startIndex = 0, int count = 10)
        {
            int totalRowCount;

            IEnumerable<ElementBO> retValue = ElementManager.GetMetadataElementsByCampaignAndImport(campaignId, schemaId, showIndirect, out totalRowCount, importId, startIndex, count);
            return new DataTableResultModel<ElementBO> { Data = retValue, Total = totalRowCount };
        }


        /// <summary>
        /// Gets the channels.
        /// </summary>
        /// <returns></returns>
        [CMTAuthorize(AllowExternal = true)]
        [EnableCors("*", "*", "GET, OPTIONS", SupportsCredentials = true)]
        [Route("api/metadata/GetChannels")]
        public List<ChannelBO> GetChannels()
        {
            using (ChannelManager man = new ChannelManager())
            {
                return man.GetObjects();
            }

        }
    }
}