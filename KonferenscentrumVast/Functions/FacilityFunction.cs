using System.Net;
using KonferenscentrumVast.Services;
using KonferenscentrumVast.DTO;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace KonferenscentrumVast.Functions
{
    public class FacilityFunction
    {
        private readonly FacilityService _facilityService;
        private readonly ILogger<FacilityFunction> _logger;

        public FacilityFunction(FacilityService facilityService, ILogger<FacilityFunction> logger)
        {
            _facilityService = facilityService;
            _logger = logger;
        }

        [Function("GetAllFacilities")]
        public async Task<HttpResponseData> GetAllFacilities(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "facility")] HttpRequestData req)
        {
            var list = await _facilityService.GetAllAsync();
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(list);
            return response;
        }

        [Function("GetFacilityById")]
        public async Task<HttpResponseData> GetFacilityById(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "facility/{id:int}")] HttpRequestData req,
            int id)
        {
            var facility = await _facilityService.GetByIdAsync(id);
            var response = req.CreateResponse(facility == null ? HttpStatusCode.NotFound : HttpStatusCode.OK);
            if (facility != null) await response.WriteAsJsonAsync(facility);
            return response;
        }

        [Function("CreateFacility")]
        public async Task<HttpResponseData> CreateFacility(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "facility")] HttpRequestData req)
        {
            var dto = await req.ReadFromJsonAsync<FacilityCreateDto>();
            var facility = await _facilityService.CreateAsync(
                dto.Name, dto.Description, dto.Address, dto.PostalCode, dto.City,
                dto.MaxCapacity, dto.PricePerDay, dto.IsActive);

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(facility);
            return response;
        }

        [Function("UpdateFacility")]
        public async Task<HttpResponseData> UpdateFacility(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "facility/{id:int}")] HttpRequestData req,
            int id)
        {
            var dto = await req.ReadFromJsonAsync<FacilityUpdateDto>();
            var facility = await _facilityService.UpdateAsync(
                id, dto.Name, dto.Description, dto.Address, dto.PostalCode, dto.City,
                dto.MaxCapacity, dto.PricePerDay, dto.IsActive);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(facility);
            return response;
        }

        [Function("DeleteFacility")]
        public async Task<HttpResponseData> DeleteFacility(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "facility/{id:int}")] HttpRequestData req,
            int id)
        {
            await _facilityService.DeleteAsync(id);
            return req.CreateResponse(HttpStatusCode.NoContent);
        }

        [Function("SetActiveFacility")]
        public async Task<HttpResponseData> SetActiveFacility(
            [HttpTrigger(AuthorizationLevel.Function, "patch", Route = "facility/{id:int}/active")] HttpRequestData req,
            int id)
        {
            var dto = await req.ReadFromJsonAsync<FacilitySetActiveDto>();
            var facility = await _facilityService.SetActiveAsync(id, dto.IsActive);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(facility);
            return response;
        }
    }
}
