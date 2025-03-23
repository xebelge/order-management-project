using CustomerOrderApi.Helpers;
using CustomerOrders.Application.DTOs;
using CustomerOrders.Application.Queries.CustomerQueries;
using CustomerOrders.Application.Services;
using CustomerOrders.Core.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CustomerOrderApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/customers")]
    public class CustomerController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly PasswordHasher<Customer> _passwordHasher;
        private readonly IValidator<UpdateCustomerInfoRequest> _updateInfoValidator;
        private readonly IValidator<int> _idValidator;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(
            IMediator mediator,
            CustomerService customerService,
            IValidator<UpdateCustomerInfoRequest> updateInfoValidator,
            IValidator<int> idValidator,
            ILogger<CustomerController> logger)
        {
            _mediator = mediator;
            _passwordHasher = new PasswordHasher<Customer>();
            _updateInfoValidator = updateInfoValidator;
            _idValidator = idValidator;
            _logger = logger;
        }

        /// <summary>
        /// Returns all active customers.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCustomers()
        {
            var customers = await _mediator.Send(new GetAllCustomersQuery());
            _logger.LogInformation("Fetched {Count} customers.", customers.Count());
            return Ok(customers);
        }

        /// <summary>
        /// Retrieves a customer with a specific ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomerById(int id)
        {
            var validationResult = await ValidationHelper.ValidateId(id, _idValidator);
            if (validationResult != null)
                return validationResult;

            var customerDto = await _mediator.Send(new GetCustomerByIdQuery { Id = id });
            if (customerDto == null)
            {
                _logger.LogWarning("Customer with ID {CustomerId} not found.", id);
                return NotFound(new ApiResponseDto<string>(404, false, "Customer not found."));
            }

            _logger.LogInformation("Customer with ID {CustomerId} retrieved.", id);
            return Ok(customerDto);
        }

        /// <summary>
        /// Updates the name, address, or email of the logged-in customer.
        /// </summary>
        [HttpPut("update-info")]
        public async Task<IActionResult> UpdateCustomerInfo([FromBody] UpdateCustomerInfoRequest request)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized(new ApiResponseDto<string>(401, false, "User identity not found."));

            var validationResult = await ValidationHelper.ValidateRequest(_updateInfoValidator, request);
            if (validationResult != null)
                return validationResult;

            var result = await _mediator.Send(new UpdateCustomerInfoCommand
            {
                Username = username,
                Request = request
            });

            if (!result)
            {
                _logger.LogWarning("Customer not found for update: {Username}", username);
                return NotFound(new ApiResponseDto<string>(404, false, "Customer not found."));
            }

            _logger.LogInformation("Customer info updated: {Username}", username);
            return NoContent();
        }

        /// <summary>
        /// Deletes the currently logged-in customer's account.
        /// </summary>
        [HttpDelete("delete-account")]
        public async Task<IActionResult> DeleteCustomer()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized(new ApiResponseDto<string>(401, false, "User identity not found."));

            var result = await _mediator.Send(new DeleteCustomerCommand { Username = username });
            if (!result)
            {
                _logger.LogWarning("Customer {Username} not found for deletion.", username);
                return NotFound(new ApiResponseDto<string>(404, false, "Customer not found."));
            }

            _logger.LogInformation("Customer {Username} account deleted.", username);
            return NoContent();
        }
    }
}
