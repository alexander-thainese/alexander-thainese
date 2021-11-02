using AutoMapper;
using CMT.Attributes;
using CMT.BL;
using CMT.BL.Core;
using CMT.BL.Managers;
using CMT.BO;
using CMT.BO.Admin;
using CMT.BO.Metadata;
using CMT.Handlers;
using CMT.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;

namespace CMT.Controllers
{
    [CMTAuthorize]

    [EnableCors("*", "Origin, Content-Type, Accept, Authorization, Cache-control, Pragma, Expires",
                                               "GET, PUT, POST, DELETE, OPTIONS", SupportsCredentials = true)]
    public class AdminMetadataController : BaseApiController
    {
        public ElementManager ElementManager { get; }
        public ElementTypeManager ElementTypeManager { get; }
        public ValueManager ValueManager { get; }
        public ValueTagManager ValueTagManager { get; }
        public ValueListManager ValueListManager { get; }
        public ValueListLevelManager ValueListLevelManager { get; }
        public ValueDetailManager ValueDetailManager { get; private set; }

        public AdminMetadataController(ElementManager elementManager, ElementTypeManager elementTypeManager, ValueManager valueManager, ValueListManager valueListManager, ValueListLevelManager valueListLevelManager, ValueDetailManager valueDetailManager, ValueTagManager valueTagManager)
        {
            ElementManager = elementManager;
            ElementTypeManager = elementTypeManager;
            ValueManager = valueManager;
            ValueListManager = valueListManager;
            ValueListLevelManager = valueListLevelManager;
            ValueDetailManager = valueDetailManager;
            ValueTagManager = valueTagManager;
        }

        /// <summary>
        /// Gets the view element list for admin screens.
        /// </summary>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="page">The page.</param>
        /// 
        /// <returns></returns>
        [Route("api/AdminMetadata/GetViewElementList")]
        public IEnumerable<ViewElementBO> GetViewElementList(int pageSize = -1, int page = 1)
        {
            using (ViewElementManager manager = new ViewElementManager())
            {
                int skip = (page - 1) * pageSize;
                return manager.GetObjects().Skip(skip).Take(pageSize);
            }
        }

        /// <summary>
        /// Gets the view element count.
        /// </summary>
        /// 
        /// <returns></returns>
        [Route("api/AdminMetadata/GetViewElementCount")]
        public int GetViewElementCount()
        {
            using (ViewElementManager manager = new ViewElementManager())
            {
                return manager.GetObjectCount();
            }
        }

        /// <summary>
        /// Updates the  text value for value element.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <exception cref="System.ArgumentNullException">id of value cannot be null</exception>
        /// <exception cref="UserFriendlyException">Value already exists on this level</exception>
        [HttpPost]
        [Route("api/AdminMetadata/UpdateValueTextValue")]
        public void UpdateValueTextValue(ValueBO value)
        {
            if (value.ObjectId == Guid.Empty)
            {
                throw new ArgumentNullException("id of value cannot be null");
            }

            ValueBO obj = ValueManager.GetObject(value.ObjectId);

            Guid? valueListId = obj.ValueListId;
            string textValue = value.TextValue;
            Guid objectId = obj.ObjectId;

            if (ValueManager.GetObjectsUsingBOPredicate(o => o.ValueListId == valueListId && o.TextValue == textValue && o.ObjectId != objectId).Any() && string.IsNullOrEmpty(value.GlobalCode))
            {
                throw new UserFriendlyException("Value already exists on this level", HttpStatusCode.NotAcceptable);
            }

            obj.TextValue = value.TextValue;
            obj.GlobalCode = value.GlobalCode;
            ValueManager.UpdateObject(obj);
        }

        /// <summary>
        /// Gets the metadata element by ids.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        [Route("api/AdminMetadata/GetMetadataElementDetails/{id}")]
        public ElementBO GetMetadataElementBO(Guid id)
        {
            return ElementManager.GetElementDetails(id);
        }

        /// <summary>
        /// Updates the element details.
        /// </summary>
        /// <param name="element">The element.</param>
        [HttpPost]
        [Route("api/AdminMetadata/UpdateMetadataElementDetails")]
        public void UpdateElementDetails(ExtElementBO element)
        {
            ElementManager.UpdateLovLabels(element);
        }

        /// <summary>
        /// Updates the metadata element description.
        /// </summary>
        /// <param name="element">The element.</param>
        [HttpPost]
        [Route("api/AdminMetadata/UpdateMetadataElementDescription")]
        public void UpdateMetadataElementDescription(ExtElementBO element)
        {
            ElementManager.UpdateMetadataElementDescription(element);
        }

        /// <summary>
        /// Updates the name of the metadata element.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <exception cref="System.ArgumentNullException">id of value cannot be null</exception>
        [HttpPost]
        [Route("api/AdminMetadata/UpdateMetadataElementName")]
        public void UpdateMetadataElementName(ElementBO value)
        {
            if (value.ObjectId == Guid.Empty)
            {
                throw new ArgumentNullException("id of value cannot be null");
            }

            ElementBO bo = ElementManager.GetObject(value.ObjectId);
            bo.Name = value.Name;

            ElementManager.UpdateObject(bo);
        }

        /// <summary>
        /// Gets the metadata elements tree.
        /// </summary>
        /// <param name="countryCode">The country code.</param>
        /// <param name="searchTerm">The search term.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">Country code cannot be null</exception>
        [Route("api/AdminMetadata/GetMetadataElementsTree/{countryCode}/{searchTerm?}")]
        public List<TreeElementBO> GetMetadataElementsTree(string countryCode, string searchTerm = null)
        {
            if (string.IsNullOrEmpty(countryCode))
            {
                throw new ArgumentNullException("Country code cannot be null");
            }

            using (ViewMetadataElementTreeManager manager = new ViewMetadataElementTreeManager())
            {
                return manager.GetElementsTree(countryCode, searchTerm);
            }
        }

        /// <summary>
        /// Translates the value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="countryCode">The country code.</param>
        /// <exception cref="System.ArgumentNullException">id of value cannot be null</exception>
        [HttpPost]
        [Route("api/AdminMetadata/TranslateValue/{countryCode}")]
        public void TranslateValue(TreeElementBO value, string countryCode)
        {
            if (value.ObjectId == Guid.Empty)
            {
                throw new ArgumentNullException("id of value cannot be null");
            }

            ValueDetailManager.TranslateValue(value, countryCode);
        }

        /// <summary>
        /// Disables the value.
        /// </summary>
        /// <param name="id">The identifier.</param>
        [HttpPost]
        [Route("api/AdminMetadata/DisableValue/{id}")]
        public void DisableValue(Guid id)
        {
            ValueBO value = ValueManager.GetObject(id);
            value.Status = (byte)ValueStatus.Inactive;
            ValueManager.UpdateObject(value);
            ValueTagManager.RemoveTagsByValueId(value.ObjectId);

        }

        /// <summary>
        /// Gets the value translations asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        [Route("api/AdminMetadata/GetValueTranslations/{id}")]
        public async Task<Dictionary<string, string>> GetValueTranslationsAsync(Guid id)
        {
            return await ValueDetailManager.GetValueTranslations(id);
        }

        /// <summary>
        /// Adds the child value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/AdminMetadata/AddChildValue")]
        public Guid? AddChildValue(TreeElementBO value)
        {
            return ValueManager.AddChildValue(value);
        }

        /// <summary>
        /// Adds the element.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/AdminMetadata/AddElement")]
        public Guid? AddElement(ExtElementBO value)
        {
            return ElementManager.AddElement(value);
        }

        /// <summary>
        /// Gets list of metadata element types
        /// </summary>
        /// <returns></returns>
        [Route("api/AdminMetadata/GetElementTypes")]
        public List<ElementTypeBO> GetElementTypes()
        {
            return ElementTypeManager.GetObjects().OrderBy(o => o.Name).ToList();
        }

        /// <summary>
        /// Gets the value list levels.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        [Route("api/AdminMetadata/GetValueListLevels/{id}")]
        public List<ValueListLevelBO> GetValueListLevels(Guid id)
        {
            return ValueListLevelManager.GetObjectsUsingBOPredicate(o => o.ElementId == id);
        }

        /// <summary>
        /// Deactivates the object.
        /// </summary>
        /// <param name="obj">The object.</param>
        [HttpPost]
        [Route("api/AdminMetadata/DeleteObject")]
        public void DeactivateObject(TreeElementBO obj)
        {
            if (obj.Type == 2)
            {
                ElementManager.DeleteObject(obj.ObjectId);
            }
            else
            {
                ValueManager.DeleteObject(obj.ObjectId);
            }
        }

        /// <summary>
        /// Reactivates the object.
        /// </summary>
        /// <param name="obj">The object.</param>
        [HttpPost]
        [Route("api/AdminMetadata/ReactivateObject")]
        public void ReactivateObject(TreeElementBO obj)
        {
            if (obj.Type == 2)
            {
                ElementManager.ActivateObject(obj.ObjectId);
            }
            else
            {
                ValueManager.ActivateObject(obj.ObjectId);
            }
        }

        /// <summary>
        /// Gets the element definition.
        /// </summary>
        /// <param name="elementId">The element identifier.</param>
        /// <param name="countryCode">The country code.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/AdminMetadata/GetElementDefinition/{elementId}/{countryCode}")]
        public HttpResponseMessage GetElementDefinition(Guid elementId, string countryCode)
        {
            using (CountryManager countryManager = new CountryManager())
            {
                CountryBO country = countryManager.GetObjectsUsingBOPredicate(o => o.Code == countryCode).Single();
                ElementBO element = ElementManager.GetObjectsUsingBOPredicate(o => o.ObjectId == elementId, new List<string>() { "ElementType" }).Single();
                Stream stream = ElementManager.GetElementStructure(elementId, country.ObjectId);
                stream.Position = 0;

                HttpResponseMessage response = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StreamContent(stream)
                };

                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = string.Format("{0}_{1}.txt", countryCode, element.Name)
                };

                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/txt");

                return response;
            }
        }

        /// <summary>
        /// Gets the third party systems access by element identifier.
        /// </summary>
        /// <param name="elementId">The element identifier.</param>
        /// <returns></returns>
        [Route("api/AdminMetadata/GetThirdPartySystemsAccessByElementId/{elementId}")]
        public List<ThirdPartySystem> GetThirdPartySystemsAccessByElementId(Guid elementId)
        {
            return ElementManager.GetThirdPartySystems(elementId);
        }

        /// <summary>
        /// Updates the third party systems access.
        /// </summary>
        /// <param name="system">The system.</param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException"></exception>
        [Route("api/AdminMetadata/UpdateThirdPartySystemsAccess")]
        public async Task<ApiResponse<bool>> UpdateThirdPartySystemsAccess(ThirdPartySystem system)
        {
            if (await ElementManager.UpdateThirdPartySystemsAccess(system))
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

        /// <summary>
        /// Gets the schema definition.
        /// </summary>
        /// <param name="schemaId">The schema identifier.</param>
        /// <param name="countryCode">The country code.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/AdminMetadata/GetSchemaDefinition/{schemaId}/{countryCode}")]
        public HttpResponseMessage GetSchemaDefinition(Guid schemaId, string countryCode)
        {
            using (CountryManager countryManager = new CountryManager())
            {
                using (SchemaManager schemaManager = new SchemaManager())
                {
                    SchemaBO schema = schemaManager.GetObject(schemaId);
                    CountryBO country = countryManager.GetObjectsUsingBOPredicate(o => o.Code == countryCode).Single();
                    DateTime currentDate = DateTime.UtcNow;

                    List<Dictionary<string, Stream>> dictionaries = new List<Dictionary<string, Stream>>();

                    dictionaries.Add(schemaManager.GetSchemaExtracts(currentDate, schemaId, country.ObjectId));
                    dictionaries.Add(schemaManager.GetSchemaElementExtracts(currentDate, schemaId, country.ObjectId));
                    dictionaries.Add(ElementManager.GetElementExtracts(currentDate, schemaId, country.ObjectId));
                    dictionaries.Add(ValueManager.GetValuesExtracts(currentDate, schemaId, country.ObjectId));
                    dictionaries.Add(ValueManager.GetValueDetailExtracts(currentDate, schemaId, country.ObjectId));

                    Dictionary<string, Stream> extracts = dictionaries.SelectMany(o => o).ToDictionary(o => o.Key, o => o.Value);
                    MemoryStream memoryStream = new MemoryStream();

                    using (ZipArchive zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                    {
                        foreach (string key in extracts.Keys)
                        {
                            ZipArchiveEntry zipArchiveEntry = zipArchive.CreateEntry(key);

                            using (Stream stream = zipArchiveEntry.Open())
                            {
                                extracts[key].Position = 0;
                                extracts[key].CopyTo(stream);
                            }
                        }
                    }

                    memoryStream.Position = 0;

                    HttpResponseMessage response = new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StreamContent(memoryStream)
                    };

                    response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                    {
                        FileName = string.Format("{0}_{1}.zip", countryCode, schema.Name)
                    };

                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                    return response;
                }
            }
        }

        /// <summary>
        /// Gets the metadata elements with description.
        /// </summary>
        /// <param name="schemaId">The schema identifier.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/AdminMetadata/GetActiveMetadataElementsWithDescription/{schemaId?}")]
        public List<TreeElementBO> GetMetadataElementsWithDescription(Guid? schemaId)
        {
            if (!schemaId.HasValue)
            {
                return ElementManager.GetActiveElementsWithDescription();
            }
            else
            {
                return ElementManager.GetActiveElementsWithDescriptionBySchema(schemaId.Value);
            }
        }

        /// <summary>
        /// Gets source element of dependent metadata element.
        /// </summary>
        /// <param name="id">The dependent element identifier.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/AdminMetadata/getParentElement/{id}")]
        public ItemModel GetParentElement(Guid id)
        {
            KeyValuePair<Guid, string> value = ElementManager.getParentElementByChildId(id);
            return new ItemModel(value.Value, value.Key);
        }

        /// <summary>
        /// Gets the list of dependent metadata elements.
        /// </summary>
        /// <param name="id">The element identifier.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/AdminMetadata/GetDepentdentElements/{id}")]
        public List<ItemModel> GetDepentdentElements(Guid id)
        {
            return ElementManager.GetDepentdentElements(id)
            .Select(p => new ItemModel(p.Value, p.Key))
            .ToList();
        }

        /// <summary>
        /// Sets the default value of metadata elements.
        /// </summary>
        /// <param name="elementId">The element identifier.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/AdminMetadata/SetDefaultValue/{elementId}")]
        public bool SetDefaultValue(Guid elementId, [FromBody]string value)
        {
            return ElementManager.SetDefaultValue(elementId, value);
        }

        [HttpGet]
        [Route("api/AdminMetadata/GetElementValuesForDefault/{elementId}")]
        public List<ListValue> GetElementValuesForDefault(Guid elementId)
        {
            return ElementManager.GetElementValuesForDefault(elementId);
        }

        [HttpGet]
        [Route("api/AdminMetadata/GetValuesAttributes")]
        public List<ParentValueAttribute> GetValuesAttributes()
        {
            return ValueManager.GetParentValueAttributes();
        }

        [HttpPost]
        [Route("api/AdminMetadata/UpdateElementType")]
        public bool UpdateElementType(ElementBO element)
        {
            return ElementManager.UpdateElementType(element.ObjectId, element.TypeId);
        }

        [HttpPost]
        [Route("api/AdminMetadata/RemoveDerivedElement/{elementId}")]
        public bool RemoveDerivedElement(Guid elementId)
        {
            return ElementManager.RemoveDerivedElement(elementId);
        }

        [HttpPost]
        [Route("api/AdminMetadata/AddValueTags")]
        public bool AddValueTag(List<ValueTagModel> valueTagModelList)
        {
            try
            {
                MapperConfiguration config = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<ValueTagModel, ValueTagBO>();
                });
                IMapper mapper = config.CreateMapper();

                List<ValueTagBO> valueTagList = valueTagModelList.Select(model => mapper.Map<ValueTagBO>(model)).ToList();
                if (valueTagList.Any(p => string.IsNullOrWhiteSpace(p.SchemaTag)
                 && !p.TagElementId.HasValue && !p.TagValueId.HasValue && !p.CountryId.HasValue))
                {
                    throw new ArgumentNullException("Non of tag values were set");
                }
                ValueTagManager.InsertObjects(valueTagList);
            }
            catch (Exception ex)
            {
                new CMTLogger().LogError(GetType(), ex);
                return false;
            }
            return true;
        }

        [HttpGet]
        [Route("api/AdminMetadata/GetValueTags/{valueId}")]
        public List<ValueTagBO> AddValueTag(Guid valueId)
        {
            return ValueTagManager.GetValueTags(valueId);
        }

        [HttpPost]
        [Route("api/AdminMetadata/RemoveValueTag/{valueTagId}")]
        public void RemoveValueTag(Guid valueTagId)
        {
            ValueTagManager.DeleteObject(valueTagId);
        }

        [HttpDelete]
        [Route("api/AdminMetadata/ValueTags/{valueTagId}")]
        public void DeleteValueTag(Guid valueTagId)
        {
            ValueTagManager.DeleteObject(valueTagId);
        }

        [HttpGet]
        [Route("api/AdminMetadata/Elements/{elementId}/CanDelete")]
        public bool CanDeleteDependentElement(Guid elementId)
        {
            return ElementManager.CanDeleteDerivedElement(elementId);
        }
    }
}