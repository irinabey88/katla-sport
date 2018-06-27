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
    [RoutePrefix("api/hives")]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [CustomExceptionFilter]
    [SwaggerResponseRemoveDefaults]
    public class HivesController : ApiController
    {
        private readonly IHiveService _hiveService;
        private readonly IHiveSectionService _hiveSectionService;

        public HivesController(IHiveService hiveService, IHiveSectionService hiveSectionService)
        {
            _hiveService = hiveService ?? throw new ArgumentNullException(nameof(hiveService));
            _hiveSectionService = hiveSectionService ?? throw new ArgumentNullException(nameof(hiveSectionService));
        }

        [HttpGet]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Returns a list of hives.", Type = typeof(HiveListItem[]))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal server error.", Type = typeof(string))]
        public async Task<IHttpActionResult> GetHives()
        {
            try
            {
                var hives = await _hiveService.GetHivesAsync();
                return Ok(hives);
            }
            catch (Exception e)
            {
                return InternalServerError();
            }
        }

        [HttpGet]
        [Route("{hiveId:int:min(1)}")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Returns a hive.", Type = typeof(Hive))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Invalid input data", Type = typeof(string))]
        [SwaggerResponse(HttpStatusCode.NotFound, Description = "Hive with hiveId isn't found" , Type = typeof(string))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal server error.", Type = typeof(string))]
        public async Task<IHttpActionResult> GetHive(int hiveId)
        {
            if (hiveId < 1)
            {
                return BadRequest($"{Resources.INVALI_ID_VALUE}{hiveId}");
            }

            try
            {
                var hive = await _hiveService.GetHiveAsync(hiveId);
                return Ok(hive);
            }          
            catch (Exception)
            {
                return InternalServerError();
            }
        }

        [HttpGet]
        [Route("{hiveId:int:min(1)}/sections")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Returns a list of hive sections for specified hive.", Type = typeof(HiveSectionListItem))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Invalid input data", Type = typeof(string))]
        [SwaggerResponse(HttpStatusCode.NotFound, Description = "Hive with hiveId isn't found", Type = typeof(string))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal server error.", Type = typeof(string))]
        public async Task<IHttpActionResult> GetHiveSections(int hiveId)
        {
            if (hiveId < 1)
            {
                return BadRequest($"{Resources.INVALI_ID_VALUE}{hiveId}");
            }

            try
            {
                var hive = await _hiveSectionService.GetHiveSectionsAsync(hiveId);
                return Ok(hive);
            }
            catch (RequestedResourceNotFoundException)
            {
                return NotFound();
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }

        [HttpPut]
        [Route("{hiveId:int:min(1)}/status/{deletedStatus:bool}")]
        [SwaggerResponse(HttpStatusCode.NoContent, Description = "Sets deleted status for an existed hive.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Invalid input data", Type = typeof(string))]
        [SwaggerResponse(HttpStatusCode.NotFound, Description = "Hive with hiveId isn't found", Type = typeof(string))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal server error.", Type = typeof(string))]
        public async Task<IHttpActionResult> SetStatus([FromUri] int hiveId, [FromUri] bool deletedStatus)
        {
            if (hiveId < 1)
            {
                return BadRequest($"{Resources.INVALI_ID_VALUE}{hiveId}");
            }

            try
            {
                await _hiveService.SetStatusAsync(hiveId, deletedStatus);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NoContent));
            }
            catch (RequestedResourceHasConflictException e)
            {
                return Conflict();
            }
            catch (Exception e)
            {
                return InternalServerError();
            }
        }

        [HttpPost]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.Created, Description = "Creates a new hive.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Invalid input data", Type = typeof(string))]
        [SwaggerResponse(HttpStatusCode.Conflict, Description = "Incorrect input data. Hive with such code already exists", Type = typeof(string))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal server error.", Type = typeof(string))]
        public async Task<IHttpActionResult> AddHive([FromBody] UpdateHiveRequest createRequest)
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
                var hive = await _hiveService.CreateHiveAsync(createRequest);
                var location = $"/api/hives/{hive.Id}";
                return Created<Hive>(location, hive);
            }
            catch (RequestedResourceHasConflictException)
            {
                return Conflict();
            }
            catch (Exception e)
            {
                return InternalServerError();
            }
        }

        [HttpPut]
        [Route("{hiveId:int:min(1)}")]
        [SwaggerResponse(HttpStatusCode.NoContent, Description = "Updates an existed hive.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Invalid input data", Type = typeof(string))]
        [SwaggerResponse(HttpStatusCode.Conflict, Description = "Incorrect input data. Hive with such code already exists", Type = typeof(string))]
        [SwaggerResponse(HttpStatusCode.NotFound, Description = "Hive with hiveId isn't found", Type = typeof(string))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal server error.", Type = typeof(string))]
        public async Task<IHttpActionResult> UpdateHive([FromUri] int hiveId, [FromBody] UpdateHiveRequest updateRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (hiveId < 1)
            {
                return BadRequest($"{Resources.INVALI_ID_VALUE}{hiveId}");
            }

            try
            {
                await _hiveService.UpdateHiveAsync(hiveId, updateRequest);
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
        [Route("{hiveId:int:min(1)}")]
        [SwaggerResponse(HttpStatusCode.NoContent, Description = "Deletes an existed hive.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Invalid input data", Type = typeof(string))]
        [SwaggerResponse(HttpStatusCode.Conflict, Description = "Incorrect input data. Hive with such code already exists", Type = typeof(string))]
        [SwaggerResponse(HttpStatusCode.NotFound, Description = "Hive with hiveId isn't found", Type = typeof(string))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal server error.", Type = typeof(string))]
        public async Task<IHttpActionResult> DeleteHive([FromUri] int hiveId)
        {
            if (hiveId < 1)
            {
                return BadRequest($"{Resources.INVALI_ID_VALUE}{hiveId}");
            }

            try
            {
                await _hiveService.DeleteHiveAsync(hiveId);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NoContent));
            }
            catch (Exception e)
            {
                return InternalServerError();
            }
        }
    }
}
