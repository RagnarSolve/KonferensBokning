using System.Net;
using KonferenscentrumVast.DTO;
using KonferenscentrumVast.Models;
using KonferenscentrumVast.Repository.Interfaces;
using KonferenscentrumVast.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace KonferenscentrumVast.Functions
{
    public class BookingContractFunction
    {
        private readonly BookingContractService _service;
        private readonly IBookingContractRepository _repo;
        private readonly ILogger<BookingContractFunction> _logger;

        public BookingContractFunction(BookingContractService service, IBookingContractRepository repo, ILogger<BookingContractFunction> logger)
        {
            _service = service;
            _repo = repo;
            _logger = logger;
        }

        [Function("GetAllContracts")]
        public async Task<HttpResponseData> GetAllContracts(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "bookingcontract")] HttpRequestData req)
        {
            var list = await _repo.GetAllAsync();
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(list.Select(ToDto));
            return response;
        }

        [Function("GetContractById")]
        public async Task<HttpResponseData> GetContractById(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "bookingcontract/{id:int}")] HttpRequestData req,
            int id)
        {
            var contract = await _service.GetByIdAsync(id);
            var response = req.CreateResponse(contract == null ? HttpStatusCode.NotFound : HttpStatusCode.OK);
            if (contract != null) await response.WriteAsJsonAsync(ToDto(contract));
            return response;
        }

        [Function("CreateContract")]
        public async Task<HttpResponseData> CreateContract(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "bookingcontract/booking/{bookingId:int}")] HttpRequestData req,
            int bookingId)
        {
            var dto = await req.ReadFromJsonAsync<BookingContractCreateDto>();
            var contract = await _service.CreateBasicForBookingAsync(bookingId, dto.Terms, dto.PaymentDueDate);

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(ToDto(contract));
            return response;
        }

        [Function("PatchContract")]
        public async Task<HttpResponseData> PatchContract(
            [HttpTrigger(AuthorizationLevel.Function, "patch", Route = "bookingcontract/{id:int}")] HttpRequestData req,
            int id)
        {
            var dto = await req.ReadFromJsonAsync<BookingContractPatchDto>();
            var updated = await _service.PatchAsync(id, dto);
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(ToDto(updated));
            return response;
        }

        [Function("MarkContractSent")]
        public async Task<HttpResponseData> MarkContractSent(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "bookingcontract/{id:int}/send")] HttpRequestData req,
            int id)
        {
            var updated = await _service.MarkSentAsync(id);
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(ToDto(updated));
            return response;
        }

        [Function("MarkContractSigned")]
        public async Task<HttpResponseData> MarkContractSigned(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "bookingcontract/{id:int}/sign")] HttpRequestData req,
            int id)
        {
            var updated = await _service.MarkSignedAsync(id);
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(ToDto(updated));
            return response;
        }

        [Function("CancelContract")]
        public async Task<HttpResponseData> CancelContract(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "bookingcontract/{id:int}/cancel")] HttpRequestData req,
            int id)
        {
            var reason = await req.ReadFromJsonAsync<string?>();
            var updated = await _service.CancelAsync(id, reason);
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(ToDto(updated));
            return response;
        }

        private static BookingContractResponseDto ToDto(BookingContract c)
        {
            return new BookingContractResponseDto
            {
                Id = c.Id,
                BookingId = c.BookingId,
                ContractNumber = c.ContractNumber,
                Version = c.Version,
                Status = c.Status.ToString(),
                Terms = c.Terms,
                TotalAmount = c.TotalAmount,
                Currency = c.Currency,
                PaymentDueDate = c.PaymentDueDate,
                CustomerName = c.CustomerName,
                CustomerEmail = c.CustomerEmail,
                FacilityName = c.FacilityName,
                CreatedDate = c.CreatedDate,
                LastUpdated = c.LastUpdated,
                SignedAt = c.SignedAt,
                CancelledAt = c.CancelledAt,
                CancelReason = c.CancelReason
            };
        }
    }
}
