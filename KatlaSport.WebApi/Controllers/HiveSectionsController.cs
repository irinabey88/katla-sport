using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using KatlaSport.Services;
using KatlaSport.Services.HiveManagement;
using KatlaSport.WebApi.CustomFilters;
using KatlaSport.WebApi.Properties;
using Microsoft.Web.Http;
using Swashbuckle.Swagger.Annotations;

namespace KatlaSport.WebApi.Controllers
{
    [ApiVersion("1.0")]
    [RoutePrefix("api/sections")]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [CustomExceptionFilter]
    [SwaggerResponseRemoveDefaults]
    public class HiveSectionsController : ApiController
    {
        private readonly IHiveSectionService _hiveSectionService;

        public HiveSectionsController(IHiveSectionService hiveSectionService)
        {
            _hiveSectionService = hiveSectionService ?? throw new ArgumentNullException(nameof(hiveSectionService));
        }

        [HttpGet]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Returns a list of hive sections.", Type = typeof(HiveSectionListItem[]))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal server error.", Type = typeof(string))]
        public async Task<IHttpActionResult> GetHiveSections()
        {
            try
            {
                var hives = await _hiveSectionService.GetHiveSectionsAsync();
                return Ok(hives);
            }
            catch (Exception e)
            {
                return InternalServerError();
            }
        }

        [HttpGet]
        [Route("{hiveSectionId:int:min(1)}")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Returns a hive section.", Type = typeof(HiveSection))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Invalid input data", Type = typeof(string))]
        [SwaggerResponse(HttpStatusCode.NotFound, Description = "HiveSection with hiveSectionId isn't found", Type = typeof(string))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal server error.", Type = typeof(string))]
        public async Task<IHttpActionResult> GetHiveSection(int hiveSectionId)
        {
            if (hiveSectionId < 1)
            {
                return BadRequest($"{Resources.INVALI_ID_VALUE}{hiveSectionId}");
            }

            try
            {
                var hive = await _hiveSectionService.GetHiveSectionAsync(hiveSectionId);
                return Ok(hive);
            }
            catch (RequestedResourceNotFoundException)
            {
                return NotFound();
            }
            catch (Exception e)
            {
                return InternalServerError();
            }
        }

        [HttpPut]
        [Route("{hiveSectionId:int:min(1)}/status/{deletedStatus:bool}")]
        [SwaggerResponse(HttpStatusCode.NoContent, Description = "Sets deleted status for an existed hive section.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Invalid input data", Type = typeof(string))]
        [SwaggerResponse(HttpStatusCode.NotFound, Description = "HiveSection with hiveSectionId isn't found", Type = typeof(string))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal server error.", Type = typeof(string))]
        public async Task<IHttpActionResult> SetStatus([FromUri] int hiveSectionId, [FromUri] bool deletedStatus)
        {
            if (hiveSectionId < 1)
            {
                return BadRequest($"{Resources.INVALI_ID_VALUE}{hiveSectionId}");
            }

            try
            {
                await _hiveSectionService.SetStatusAsync(hiveSectionId, deletedStatus);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NoContent));
            }
            catch (RequestedResourceNotFoundException)
            {
                return NotFound();
            }
            catch (Exception e)
            {
                return InternalServerError();
            }
        }

        [HttpPost]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.Created, Description = "Creates a new hive section.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Invalid input data", Type = typeof(string))]
        [SwaggerResponse(HttpStatusCode.Conflict, Description = "Incorrect input data. HiveSection with such code already exists", Type = typeof(string))]
        [SwaggerResponse(HttpStatusCode.NotFound, Description = "Hive with hiveId isn't found", Type = typeof(string))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal server error.", Type = typeof(string))]
        public async Task<IHttpActionResult> AddHiveSection([FromBody] UpdateHiveSectionRequest createRequest)
        {
            if (createRequest == null)
            {
                return BadRequest(Resources.NULL_OBJECT_VALUE);
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var hiveSection = await _hiveSectionService.CreateHiveSectionAsync(createRequest);
                var location = string.Format("/api/hives/{0}", hiveSection.Id);
                return Created<HiveSection>(location, hiveSection);
            }
            catch (RequestedResourceHasConflictException)
            {
                return Conflict();
            }
            catch (RequestedResourceNotFoundException)
            {
                return NotFound();
            }
            catch (Exception e)
            {
                return InternalServerError();
            }
        }

        [HttpPut]
        [Route("{hiveSectionId:int:min(1)}")]
        [SwaggerResponse(HttpStatusCode.NoContent, Description = "Updates an existed hive section.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Invalid input data", Type = typeof(string))]
        [SwaggerResponse(HttpStatusCode.Conflict, Description = "Incorrect input data. HiveSection with such code already exists", Type = typeof(string))]
        [SwaggerResponse(HttpStatusCode.NotFound, Description = "HiveSection with hiveSectionId isn't found", Type = typeof(string))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal server error.", Type = typeof(string))]
        public async Task<IHttpActionResult> UpdateHiveSection([FromUri] int hiveSectionId, [FromBody] UpdateHiveSectionRequest updateRequest)
        {
            if (hiveSectionId < 1)
            {
                return BadRequest($"{Resources.INVALI_ID_VALUE}{hiveSectionId}");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _hiveSectionService.UpdeteHiveSectionAsync(hiveSectionId, updateRequest);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NoContent));
            }
            catch (RequestedResourceHasConflictException)
            {
                return Conflict();
            }
            catch (RequestedResourceNotFoundException)
            {
                return NotFound();
            }
            catch (Exception e)
            {
                return InternalServerError();
            }
        }

        [HttpDelete]
        [Route("{hiveSectionId:int:min(1)}")]
        [SwaggerResponse(HttpStatusCode.NoContent, Description = "Deletes an existed hive section.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Invalid input data", Type = typeof(string))]
        [SwaggerResponse(HttpStatusCode.Conflict, Description = "Incorrect input data. HiveSection with such code already exists", Type = typeof(string))]
        [SwaggerResponse(HttpStatusCode.NotFound, Description = "HiveSection with hiveSectionId isn't found", Type = typeof(string))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal server error.", Type = typeof(string))]
        public async Task<IHttpActionResult> DeleteHiveSection([FromUri] int hiveSectionId)
        {
            if (hiveSectionId < 1)
            {
                return BadRequest($"{Resources.INVALI_ID_VALUE}{hiveSectionId}");
            }

            try
            {
                await _hiveSectionService.DeleteHiveSectionAsync(hiveSectionId);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NoContent));
            }
            catch (RequestedResourceHasConflictException)
            {
                return Conflict();
            }
            catch (RequestedResourceNotFoundException)
            {
                return NotFound();
            }
            catch (Exception e)
            {
                return InternalServerError();
            }
        }
    }
}
