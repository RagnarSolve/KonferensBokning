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
    public class BookingFunction
    {
        private readonly BookingService _bookingService;
        private readonly IBookingRepository _bookingRepo;
        private readonly ILogger<BookingFunction> _logger;

        public BookingFunction(BookingService bookingService, IBookingRepository bookingRepo, ILogger<BookingFunction> logger)
        {
            _bookingService = bookingService;
            _bookingRepo = bookingRepo;
            _logger = logger;
        }

        [Function("GetAllBookings")]
        public async Task<HttpResponseData> GetAllBookings(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "booking")] HttpRequestData req)
        {
            var list = await _bookingRepo.GetAllAsync();
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(list.Select(ToDto));
            return response;
        }

        [Function("GetBookingById")]
        public async Task<HttpResponseData> GetBookingById(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "booking/{id:int}")] HttpRequestData req,
            int id)
        {
            var booking = await _bookingService.GetByIdAsync(id);
            var response = req.CreateResponse(booking == null ? HttpStatusCode.NotFound : HttpStatusCode.OK);
            if (booking != null) await response.WriteAsJsonAsync(ToDto((Booking)booking));
            return response;
        }

        [Function("CreateBooking")]
        public async Task<HttpResponseData> CreateBooking(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "booking")] HttpRequestData req)
        {
            var dto = await req.ReadFromJsonAsync<BookingCreateDto>();
            var booking = await _bookingService.CreateBookingAsync(
                dto.CustomerId, dto.FacilityId, dto.StartDate, dto.EndDate, dto.NumberOfParticipants, dto.Notes);

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(ToDto(booking));
            return response;
        }

        [Function("ConfirmBooking")]
        public async Task<HttpResponseData> ConfirmBooking(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "booking/{id:int}/confirm")] HttpRequestData req,
            int id)
        {
            var updated = await _bookingService.ConfirmBookingAsync(id);
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(ToDto(updated));
            return response;
        }

        [Function("RescheduleBooking")]
        public async Task<HttpResponseData> RescheduleBooking(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "booking/{id:int}/reschedule")] HttpRequestData req,
            int id)
        {
            var dto = await req.ReadFromJsonAsync<BookingRescheduleDto>();
            var updated = await _bookingService.RescheduleBookingAsync(id, dto.StartDate, dto.EndDate);
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(ToDto(updated));
            return response;
        }

        [Function("CancelBooking")]
        public async Task<HttpResponseData> CancelBooking(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "booking/{id:int}")] HttpRequestData req,
            int id)
        {
            var dto = await req.ReadFromJsonAsync<BookingCancelDto?>();
            await _bookingService.CancelBookingAsync(id, dto?.Reason);
            var response = req.CreateResponse(HttpStatusCode.NoContent);
            return response;
        }

        private static BookingResponseDto ToDto(Booking b)
        {
            return new BookingResponseDto
            {
                Id = b.Id,
                CustomerId = b.CustomerId,
                FacilityId = b.FacilityId,
                StartDate = b.StartDate,
                EndDate = b.EndDate,
                NumberOfParticipants = b.NumberOfParticipants,
                Notes = b.Notes ?? string.Empty,
                Status = b.Status.ToString(),
                TotalPrice = b.TotalPrice,
                CreatedDate = b.CreatedDate,
                ConfirmedDate = b.ConfirmedDate,
                CancelledDate = b.CancelledDate,
                CustomerName = b.Customer != null ? $"{b.Customer.FirstName} {b.Customer.LastName}".Trim() : null,
                CustomerEmail = b.Customer?.Email,
                FacilityName = b.Facility?.Name,
                ContractId = b.Contract?.Id
            };
        }
    }
}
