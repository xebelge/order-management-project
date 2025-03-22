using CustomerOrders.Application.DTOs;
using CustomerOrders.Application.Helpers;
using CustomerOrders.Application.Services;
using CustomerOrders.Core.Entities;
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
        private readonly CustomerService _customerService;
        private readonly PasswordHasher<Customer> _passwordHasher;

        public CustomerController(CustomerService customerService)
        {
            _customerService = customerService;
            _passwordHasher = new PasswordHasher<Customer>();
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomers()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            return Ok(customers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomerById(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null) return NotFound();
            return Ok(customer);
        }

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
            if (customer == null)
            {
                return NotFound(new ApiResponseDto<string>(404, false, "Customer not found."));
            }

            // Validate and update name
            if (!string.IsNullOrEmpty(request.Name))
            {
                var nameValidation = ValidationHelper.ValidateName(request.Name);
                if (!string.IsNullOrEmpty(nameValidation))
                {
                    return BadRequest(new ApiResponseDto<string>(400, false, nameValidation));
                }
                customer.Name = request.Name;
            }
            else if (request.Name == string.Empty)
            {
                return BadRequest(new ApiResponseDto<string>(400, false, "Name cannot be empty."));
            }

            // Validate and update address
            if (!string.IsNullOrEmpty(request.Address))
            {
                customer.Address = request.Address;
            }
            else if (request.Address == string.Empty)
            {
                return BadRequest(new ApiResponseDto<string>(400, false, "Address cannot be empty."));
            }

            // Validate and update email
            if (!string.IsNullOrEmpty(request.OldEmail) &&
                !string.IsNullOrEmpty(request.NewEmail) &&
                customer.Email.Equals(request.OldEmail, StringComparison.OrdinalIgnoreCase))
            {
                var emailValidation = ValidationHelper.ValidateEmail(request.NewEmail);
                if (!string.IsNullOrEmpty(emailValidation))
                {
                    return BadRequest(new ApiResponseDto<string>(400, false, emailValidation));
                }
                customer.Email = request.NewEmail;
            }
            else if (!string.IsNullOrEmpty(request.OldEmail) || !string.IsNullOrEmpty(request.NewEmail))
            {
                return BadRequest(new ApiResponseDto<string>(400, false, "Old email does not match the current email."));
            }

            await _customerService.UpdateUserAsync(customer);

            return NoContent();
        }
    }
}
