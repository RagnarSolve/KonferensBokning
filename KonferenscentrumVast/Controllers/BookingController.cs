using System;
using KonferenscentrumVast.DTO;
using KonferenscentrumVast.Models;
using KonferenscentrumVast.Repository.Interfaces;
using KonferenscentrumVast.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Cosmos;

namespace KonferenscentrumVast.Controllers
{
    /// <summary>
    /// API controller for managing facility bookings.
    /// Handles booking creation, confirmation, rescheduling, and cancellation with availability checking.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController(
        BookingService bookingService,
        IBookingRepository bookings,
        ICustomerRepository customers,
        SendGridService sendGridEmail,
        ILogger<BookingController> logger) : ControllerBase
    {
        private readonly BookingService _bookingService = bookingService;
        private readonly IBookingRepository _bookings = bookings;
        private readonly ICustomerRepository _customers = customers;
        private readonly SendGridService _sendGridEmail = sendGridEmail;
        private readonly ILogger<BookingController> _logger = logger;

        /// <summary>
        /// Retrieves a specific booking by ID
        /// </summary>
        /// <param name="id">Booking ID</param>
        /// <returns>Booking details including customer, facility, and contract information</returns>
        /// <response code="200">Returns the booking</response>
        /// <response code="404">Booking not found</response>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<BookingResponseDto>> GetById(int id)
        {
            try
            {
                var booking = await _bookings.GetByIdAsync(id);
                if (booking == null) return NotFound();
                return Ok(ToDto(booking));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error for booking {BookingId}", id);
                return StatusCode(500, "Error retrieving booking.");
            }
        }

        /// <summary>
        /// Retrieves all bookings in the system
        /// </summary>
        /// <returns>List of all bookings</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookingResponseDto>>> GetAll()
        {
            try
            {
                var data = await _bookings.GetAllAsync();
                return Ok(data.Select(ToDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching booking.");
                return StatusCode(500, "Error retrieving bookings.");
            }
        }

        /// <summary>
        /// Retrieves bookings filtered by customer, facility, or date range
        /// </summary>
        /// <param name="customerId">Filter by customer ID</param>
        /// <param name="facilityId">Filter by facility ID</param>
        /// <param name="from">Start date for date range filter</param>
        /// <param name="to">End date for date range filter</param>
        /// <returns>Filtered list of bookings</returns>
        /// <response code="200">Returns filtered bookings</response>
        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<BookingResponseDto>>> GetFiltered(
            [FromQuery] int? customerId,
            [FromQuery] int? facilityId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            try
            {
                IEnumerable<Booking> data;

                if (customerId.HasValue)
                    data = await _bookings.GetByCustomerIdAsync(customerId.Value);
                else if (facilityId.HasValue)
                    data = await _bookings.GetByFacilityIdAsync(facilityId.Value);
                else if (from.HasValue && to.HasValue)
                    data = await _bookings.GetByDateRangeAsync(from.Value, to.Value);
                else
                    data = await _bookings.GetAllAsync();

                return Ok(data.Select(ToDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtered customerId={CustomerId}, facilityId={FacilityId}, from={From}, to={To}",
                    customerId, facilityId, from, to);
                return StatusCode(500, "Error filtering bookings.");
            }
        }

        /// <summary>
        /// Creates a new booking with availability checking and automatic contract generation
        /// </summary>
        /// <param name="request">Booking details</param>
        /// <returns>Created booking</returns>
        /// <response code="201">Booking created successfully</response>
        /// <response code="400">Invalid booking data or validation failed</response>
        /// <response code="404">Customer or facility not found</response>
        /// <response code="409">Booking conflict - facility unavailable for specified dates</response>
        [HttpPost]
        [HttpPost]
        public async Task<ActionResult<BookingResponseDto>> Create([FromBody] BookingCreateDto request)
        {
            try
            {
                var booking = await _bookingService.CreateBookingAsync(
                    request.CustomerId,
                    request.FacilityId,
                    request.StartDate,
                    request.EndDate,
                    request.NumberOfParticipants,
                    request.Notes);

                return CreatedAtAction(nameof(GetById), new { id = booking.Id }, ToDto(booking));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error create booking for customer {CustomerId}, facility {FacilityId}", request.CustomerId, request.FacilityId);
                return StatusCode(500, "Error creating booking.");
            }
        }

        /// <summary>
        /// Confirms a pending booking
        /// </summary>
        /// <param name="id">Booking ID</param>
        /// <returns>Confirmed booking</returns>
        /// <response code="200">Booking confirmed</response>
        /// <response code="400">Cannot confirm cancelled or past booking</response>
        /// <response code="404">Booking not found</response>
        [HttpPost("{id:int}/confirm")]
        public async Task<ActionResult<BookingResponseDto>> Confirm(int id)
        {
            try
            {
                var updated = await _bookingService.ConfirmBookingAsync(id);

                // Added so the customer gets an email on confirmed
                var customer = await _customers.GetByIdAsync(updated.CustomerId);
                if (customer is not null && !string.IsNullOrWhiteSpace(customer.Email))
                {
                    _ = _sendGridEmail.SendBookingConfirmedAsync(
                        customer.Email,
                        $"{customer.FirstName} {customer.LastName}".Trim(),
                        updated.Id.ToString());
                }

                return Ok(ToDto(updated));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirm booking {BookingId}", id);
                return StatusCode(500, "Error confirming booking.");
            }
        }

        /// <summary>
        /// Changes booking dates with availability checking
        /// </summary>
        /// <param name="id">Booking ID</param>
        /// <param name="request">New start and end dates</param>
        /// <returns>Rescheduled booking with updated pricing</returns>
        /// <response code="200">Booking rescheduled</response>
        /// <response code="400">Invalid dates or cancelled booking</response>
        /// <response code="404">Booking not found</response>
        /// <response code="409">New dates conflict with existing bookings</response>
        [HttpPost("{id:int}/reschedule")]
        public async Task<ActionResult<BookingResponseDto>> Reschedule(int id, [FromBody] BookingRescheduleDto request)
        {
            try
            {
                var updated = await _bookingService.RescheduleBookingAsync(id, request.StartDate, request.EndDate);
                return Ok(ToDto(updated));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reschedule booking {BookingId}", id);
                return StatusCode(500, "Error rescheduling booking.");
            }
        }

        /// <summary>
        /// Cancels a booking with optional reason (idempotent operation)
        /// </summary>
        /// <param name="id">Booking ID</param>
        /// <param name="request">Optional cancellation details</param>
        /// <returns>No content</returns>
        /// <response code="204">Booking cancelled successfully</response>
        /// <response code="404">Booking not found</response>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Cancel(int id, [FromBody] BookingCancelDto? request)
        {
            try
            {
                await _bookingService.CancelBookingAsync(id, request?.Reason);

                // Added so the customer gets an email on cancelled
                var booking = await _bookings.GetByIdAsync(id);
                if (booking is not null)
                {
                    var customer = await _customers.GetByIdAsync(booking.CustomerId);
                    if (customer is not null && !string.IsNullOrWhiteSpace(customer.Email))
                    {
                        _ = _sendGridEmail.SendBookingCancelledAsync(
                            customer.Email,
                            $"{customer.FirstName} {customer.LastName}".Trim(),
                            booking.Id.ToString(),
                            request?.Reason);
                    }
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancel booking {BookingId}", id);
                return StatusCode(500, "Error cancelling booking.");
            }
        }

        // ------- mapping helpers-------
        /// <summary>
        /// Converts domain model to response DTO, flattening related entity data for easier consumption
        /// </summary>
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