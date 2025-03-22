using CustomerOrders.Application.DTOs;
using CustomerOrders.Application.Services;
using CustomerOrders.Core.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CustomerOrderApi.Controllers
{
    /// <summary>
    /// Provides operations to manage customer data and personal information.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/customers")]
    public class CustomerController : ControllerBase
    {
        private readonly CustomerService _customerService;
        private readonly PasswordHasher<Customer> _passwordHasher;
        private readonly IValidator<string> _nameValidator;
        private readonly IValidator<string> _emailValidator;

        public CustomerController(
            CustomerService customerService,
            IValidator<string> nameValidator,
            IValidator<string> emailValidator)
        {
            _customerService = customerService;
            _passwordHasher = new PasswordHasher<Customer>();
            _nameValidator = nameValidator;
            _emailValidator = emailValidator;
        }

        /// <summary>
        /// Lists all active (not deleted) customers.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCustomers()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            var activeCustomers = customers.Where(c => !c.IsDeleted);
            return Ok(activeCustomers);
        }

        /// <summary>
        /// Retrieves a single customer by ID, if not deleted.
        /// </summary>
        /// <param name="id">Customer ID</param>
        /// <returns>Customer data or 404 if not found/deleted.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomerById(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null || customer.IsDeleted)
                return NotFound(new ApiResponseDto<string>(404, false, "Customer not found."));

            return Ok(customer);
        }

        /// <summary>
        /// Updates personal info (name, address, email) for the logged-in customer.
        /// </summary>
        /// <param name="request">New details to update.</param>
        /// <returns>NoContent on success, or error status if failed.</returns>
        [Authorize]
        [HttpPut("update-info")]
        public async Task<IActionResult> UpdateCustomerInfo([FromBody] UpdateCustomerInfoRequest request)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized(new ApiResponseDto<string>(401, false, "User identity not found."));
            }

            var customer = await _customerService.GetUserByUsernameAsync(username);
            if (customer == null || customer.IsDeleted)
            {
                return NotFound(new ApiResponseDto<string>(404, false, "Customer not found."));
            }

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                var nameValidation = _nameValidator.Validate(request.Name);
                if (!nameValidation.IsValid)
                    return BadRequest(new ApiResponseDto<string>(400, false, nameValidation.Errors[0].ErrorMessage));

                customer.Name = request.Name;
            }

            if (!string.IsNullOrWhiteSpace(request.Address))
            {
                customer.Address = request.Address;
            }

            if (!string.IsNullOrWhiteSpace(request.OldEmail) && !string.IsNullOrWhiteSpace(request.NewEmail))
            {
                var currentEmail = customer.Email.Trim().ToLowerInvariant();
                var oldEmail = request.OldEmail.Trim().ToLowerInvariant();
                var newEmail = request.NewEmail.Trim().ToLowerInvariant();

                if (currentEmail != oldEmail)
                {
                    return BadRequest(new ApiResponseDto<string>(400, false, "Old email does not match your current email."));
                }

                var emailOwner = await _customerService.GetUserByEmailAsync(newEmail);
                if (emailOwner != null && emailOwner.Id != customer.Id)
                {
                    return BadRequest(new ApiResponseDto<string>(400, false, "This new email is already registered to another user."));
                }

                var emailValidation = _emailValidator.Validate(newEmail);
                if (!emailValidation.IsValid)
                {
                    return BadRequest(new ApiResponseDto<string>(400, false, emailValidation.Errors[0].ErrorMessage));
                }

                customer.Email = newEmail;
            }

            await _customerService.UpdateUserAsync(customer);
            return NoContent();
        }

        /// <summary>
        /// Marks the current user's account as deleted so it can't be used.
        /// </summary>
        /// <remarks>
        /// This action sets IsDeleted to true. Future login attempts will be blocked.
        /// </remarks>
        /// <returns>NoContent if successful, 404 if user not found.</returns>
        [Authorize]
        [HttpDelete("delete-account")]
        public async Task<IActionResult> DeleteCustomer()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized(new ApiResponseDto<string>(401, false, "User identity not found."));

            var customer = await _customerService.GetUserByUsernameAsync(username);
            if (customer == null || customer.IsDeleted)
                return NotFound(new ApiResponseDto<string>(404, false, "Customer not found."));

            await _customerService.DeleteUserAsync(customer.Id);
            return NoContent();
        }
    }
}
