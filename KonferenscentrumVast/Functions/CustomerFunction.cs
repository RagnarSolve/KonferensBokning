using System.Net;
using KonferenscentrumVast.DTO;
using KonferenscentrumVast.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace KonferenscentrumVast.Functions
{
    public class CustomerFunction
    {
        private readonly CustomerService _service;
        private readonly ILogger<CustomerFunction> _logger;

        public CustomerFunction(CustomerService service, ILogger<CustomerFunction> logger)
        {
            _service = service;
            _logger = logger;
        }

        [Function("GetAllCustomers")]
        public async Task<HttpResponseData> GetAllCustomers(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "customer")] HttpRequestData req)
        {
            var list = await _service.GetAllAsync();
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(list);
            return response;
        }

        [Function("GetCustomerById")]
        public async Task<HttpResponseData> GetCustomerById(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "customer/{id:int}")] HttpRequestData req,
            int id)
        {
            var entity = await _service.GetByIdAsync(id);
            var response = req.CreateResponse(entity == null ? HttpStatusCode.NotFound : HttpStatusCode.OK);
            if (entity != null) await response.WriteAsJsonAsync(entity);
            return response;
        }

        [Function("GetCustomerByEmail")]
        public async Task<HttpResponseData> GetCustomerByEmail(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "customer/by-email")] HttpRequestData req)
        {
            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            var email = query.Get("email");
            if (string.IsNullOrWhiteSpace(email))
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("Email is required");
                return bad;
            }

            var entity = await _service.GetByEmailAsync(email);
            var response = req.CreateResponse(entity == null ? HttpStatusCode.NotFound : HttpStatusCode.OK);
            if (entity != null) await response.WriteAsJsonAsync(entity);
            return response;
        }

        [Function("CreateCustomer")]
        public async Task<HttpResponseData> CreateCustomer(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "customer")] HttpRequestData req)
        {
            var dto = await req.ReadFromJsonAsync<CustomerCreateDto>();
            var entity = await _service.CreateAsync(
                dto.FirstName, dto.LastName, dto.Email, dto.Phone, dto.CompanyName, dto.Address, dto.PostalCode, dto.City);

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(entity);
            return response;
        }

        [Function("UpdateCustomer")]
        public async Task<HttpResponseData> UpdateCustomer(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "customer/{id:int}")] HttpRequestData req,
            int id)
        {
            var dto = await req.ReadFromJsonAsync<CustomerUpdateDto>();
            var entity = await _service.UpdateAsync(
                id, dto.FirstName, dto.LastName, dto.Email, dto.Phone, dto.CompanyName, dto.Address, dto.PostalCode, dto.City);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(entity);
            return response;
        }

        [Function("DeleteCustomer")]
        public async Task<HttpResponseData> DeleteCustomer(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "customer/{id:int}")] HttpRequestData req,
            int id)
        {
            await _service.DeleteAsync(id);
            return req.CreateResponse(HttpStatusCode.NoContent);
        }
    }
}
