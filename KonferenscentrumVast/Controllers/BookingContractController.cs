using System;
using System.Diagnostics.Contracts;
using KonferenscentrumVast.DTO;
using KonferenscentrumVast.Models;
using KonferenscentrumVast.Repository.Interfaces;
using KonferenscentrumVast.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Cosmos;

namespace KonferenscentrumVast.Controllers
{
    /// <summary>
    /// API controller for managing booking contracts.
    /// Handles contract lifecycle from creation through signing or cancellation.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class BookingContractController(BookingContractService service, BlobService blobService, IBookingContractRepository contracts, ILogger<BookingContractController> logger) : ControllerBase
    {
        private readonly BookingContractService _service = service;
        private readonly BlobService _blobService = blobService;
        private readonly IBookingContractRepository _contracts = contracts;
        private readonly ILogger<BookingContractController> _logger = logger;

        /// <summary>
        /// Retrieves a specific contract by ID
        /// </summary>
        /// <param name="id">Contract ID</param>
        /// <returns>Contract details</returns>
        /// <response code="200">Returns the contract</response>
        /// <response code="404">Contract not found</response>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<BookingContractResponseDto>> GetById(int id)
        {
            try
            {
                var entity = await _service.GetByIdAsync(id);
                return Ok(ToDto(entity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching contract {Id}", id);
                return StatusCode(500, "Error retrieving contract.");
            }
        }

        /// <summary>
        /// Retrieves all contracts in the system
        /// </summary>
        /// <returns>List of all contracts</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookingContractResponseDto>>> GetAll()
        {
            try
            {
                var data = await _contracts.GetAllAsync();
                return Ok(data.Select(ToDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all contracts");
                return StatusCode(500, "Error retrieving contracts.");
            }
        }

        /// <summary>
        /// Retrieves the contract associated with a specific booking
        /// </summary>
        /// <param name="bookingId">Booking ID</param>
        /// <returns>Contract for the booking</returns>
        /// <response code="200">Returns the contract</response>
        /// <response code="404">Contract not found for booking</response>
        [HttpGet("booking/{bookingId:int}")]
        public async Task<ActionResult<BookingContractResponseDto>> GetByBookingId(int bookingId)
        {
            try
            {
                var entity = await _service.GetByBookingIdAsync(bookingId);
                return Ok(ToDto(entity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching contract for booking {BookingId}", bookingId);
                return StatusCode(500, "Error retrieving the contract for booking.");
            }
        }

        /// create a basic contract for a booking (if not auto-created)
        // <summary>
        /// Manually creates a basic contract for a booking if auto-creation failed
        /// </summary>
        /// <param name="bookingId">Booking ID</param>
        /// <param name="dto">Optional contract terms and payment due date</param>
        /// <returns>Created contract</returns>
        /// <response code="201">Contract created successfully</response>
        /// <response code="400">Invalid booking or contract data</response>
        /// <response code="409">Contract already exists for booking</response>
        [HttpPost("booking/{bookingId:int}")]
        public async Task<ActionResult<BookingContractResponseDto>> CreateContract(
            int bookingId,
            [FromBody] BookingContractCreateDto dto)
        {
            try
            {
                var entity = await _service.CreateBasicForBookingAsync(
                    bookingId,
                    dto.Terms,
                    dto.PaymentDueDate
                );

                return CreatedAtAction(
                    nameof(GetByBookingId),
                    new { bookingId = entity.BookingId },
                    ToDto(entity)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating contract for booking {BookingId}", bookingId);
                return StatusCode(500, "Error while creating the contract.");
            }
        }

        /// <summary>
        /// Updates contract terms, amount, or payment due date
        /// </summary>
        /// <param name="id">Contract ID</param>
        /// <param name="dto">Contract updates</param>
        /// <returns>Updated contract</returns>
        /// <response code="200">Contract updated successfully</response>
        /// <response code="400">Cannot modify signed or cancelled contract</response>
        /// <response code="404">Contract not found</response>
        [HttpPatch("{id:int}")]
        public async Task<ActionResult<BookingContractResponseDto>> Patch(
            int id,
            [FromBody] BookingContractPatchDto dto)
        {
            try
            {
                var entity = await _service.PatchAsync(id, dto);
                return Ok(ToDto(entity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating contract {Id}", id);
                return StatusCode(500, "Error while updating contract.");
            }
        }

        /// <summary>
        /// Marks contract as sent to customer
        /// </summary>
        /// <param name="id">Contract ID</param>
        /// <returns>Updated contract with Sent status</returns>
        /// <response code="200">Contract marked as sent</response>
        /// <response code="400">Booking must be confirmed first</response>
        /// <response code="404">Contract not found</response>
        [HttpPost("{id:int}/send")]
        public async Task<ActionResult<BookingContractResponseDto>> MarkSent(int id)
        {
            try
            {
                var entity = await _service.MarkSentAsync(id);
                return Ok(ToDto(entity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking contract {Id} as sent", id);
                return StatusCode(500, "Error while updating contract as sent.");
            }
        }

        /// <summary>
        /// Records contract signature
        /// </summary>
        /// <param name="id">Contract ID</param>
        /// <returns>Updated contract with Signed status</returns>
        /// <response code="200">Contract marked as signed</response>
        /// <response code="400">Booking must be confirmed first</response>
        /// <response code="404">Contract not found</response>
        [HttpPost("{id:int}/sign")]
        public async Task<ActionResult<BookingContractResponseDto>> MarkSigned(int id)
        {
            try
            {
                var entity = await _service.MarkSignedAsync(id);
                return Ok(ToDto(entity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking contract {Id} as signed", id);
                return StatusCode(500, "Error while updating contract as signed.");
            }
        }

        /// <summary>
        /// Cancels a contract with optional reason
        /// </summary>
        /// <param name="id">Contract ID</param>
        /// <param name="reason">Cancellation reason</param>
        /// <returns>Cancelled contract</returns>
        /// <response code="200">Contract cancelled</response>
        /// <response code="404">Contract not found</response>

        [HttpPost("{id:int}/cancel")]
        public async Task<ActionResult<BookingContractResponseDto>> Cancel(
            int id,
            [FromBody] string? reason)
        {
            try
            {
                var entity = await _service.CancelAsync(id, reason);
                return Ok(ToDto(entity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling contract {Id}", id);
                return StatusCode(500, "Error cancelling the contract.");
            }
        }

        [HttpPost("upload-contract-pdf")]   //this should be in booking-endpoint instead of upload-contract-pdf endpoint
        public async Task<IActionResult> UploadBlob(IFormFile blobfile)
        {
            var result = await _blobService.UploadAsync(blobfile);
            return Ok(result);
        }

        /// <summary>
        /// Converts domain model to response DTO with flattened data for API consumers
        /// </summary>
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