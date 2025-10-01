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
    /// API controller for managing customers.
    /// Handles customer lifecycle operations with email uniqueness enforcement and booking safety checks.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController(
        CustomerService customerService,
        ICustomerRepository customers,
        ILogger<CustomerController> logger) : ControllerBase
    {
        private readonly CustomerService _customerService = customerService;
        private readonly ICustomerRepository _customers = customers;
        private readonly ILogger<CustomerController> _logger = logger;

        /// <summary>
        /// Retrieves all customers with booking statistics
        /// </summary>
        /// <returns>List of all customers</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerResponseDto>>> GetAll()
        {
            try
            {
                var list = await _customers.GetAllAsync();
                return Ok(list.Select(ToDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error get all customers");
                return StatusCode(500, "Error retrieving all customers.");
            }
        }

        /// <summary>
        /// Retrieves a specific customer by ID
        /// </summary>
        /// <param name="id">Customer ID</param>
        /// <returns>Customer details with booking statistics</returns>
        /// <response code="200">Returns the customer</response>
        /// <response code="404">Customer not found</response>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CustomerResponseDto>> GetById(int id)
        {
            try
            {
                var entity = await _customers.GetByIdAsync(id);
                if (entity == null) return NotFound();
                return Ok(ToDto(entity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error get by id customer {CustomerId}", id);
                return StatusCode(500, "Error retrieving customer.");
            }
        }

        /// <summary>
        /// Finds a customer by email address (case-insensitive)
        /// </summary>
        /// <param name="email">Customer email address</param>
        /// <returns>Customer details if found</returns>
        /// <response code="200">Returns the customer</response>
        /// <response code="400">Email parameter is required</response>
        /// <response code="404">Customer not found</response>
        [HttpGet("by-email")]
        public async Task<ActionResult<CustomerResponseDto>> GetByEmail([FromQuery] string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return BadRequest(new { message = "Email is required." });

                var entity = await _customerService.GetByEmailAsync(email);
                if (entity == null) return NotFound();
                return Ok(ToDto(entity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error get email {Email}", email);
                return StatusCode(500, "Error retrieving customer by email.");
            }
        }

        /// <summary>
        /// Creates a new customer with email uniqueness validation
        /// </summary>
        /// <param name="dto">Customer details</param>
        /// <returns>Created customer</returns>
        /// <response code="201">Customer created successfully</response>
        /// <response code="400">Invalid customer data or validation failed</response>
        /// <response code="409">Customer with email already exists</response>
        [HttpPost]
        public async Task<ActionResult<CustomerResponseDto>> Create([FromBody] CustomerCreateDto dto)
        {
            try
            {
                var created = await _customerService.CreateAsync(
                    dto.FirstName,
                    dto.LastName,
                    dto.Email,
                    dto.Phone,
                    dto.CompanyName,
                    dto.Address,
                    dto.PostalCode,
                    dto.City
                );

                return CreatedAtAction(nameof(GetById), new { id = created.Id }, ToDto(created));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error create customer with email {Email}", dto.Email);
                return StatusCode(500, "Error creating customer email.");
            }
        }

        /// <summary>
        /// Updates customer information with email conflict checking
        /// </summary>
        /// <param name="id">Customer ID</param>
        /// <param name="dto">Updated customer details</param>
        /// <returns>Updated customer</returns>
        /// <response code="200">Customer updated successfully</response>
        /// <response code="400">Invalid customer data</response>
        /// <response code="404">Customer not found</response>
        /// <response code="409">Email already used by another customer</response>
        [HttpPut("{id:int}")]
        public async Task<ActionResult<CustomerResponseDto>> Update(int id, [FromBody] CustomerUpdateDto dto)
        {
            try
            {
                var updated = await _customerService.UpdateAsync(
                    id,
                    dto.FirstName,
                    dto.LastName,
                    dto.Email,
                    dto.Phone,
                    dto.CompanyName,
                    dto.Address,
                    dto.PostalCode,
                    dto.City
                );

                return Ok(ToDto(updated));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error update customer {CustomerId}", id);
                return StatusCode(500, "Error updating customer.");
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _customerService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error delete customer {CustomerId}", id);
                return StatusCode(500, "Error deleting customer.");
            }
        }

        // ------- Mapping helpers -------

        private static CustomerResponseDto ToDto(Customer c)
        {
            var total = c.Bookings?.Count ?? 0;
            var active = c.Bookings?.Count(b => b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed) ?? 0;

            return new CustomerResponseDto
            {
                Id = c.Id,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                Phone = c.Phone,
                CompanyName = c.CompanyName,
                Address = c.Address,
                PostalCode = c.PostalCode,
                City = c.City,
                CreatedDate = c.CreatedDate,
                TotalBookings = total,
                ActiveBookings = active
            };
        }
    }
}