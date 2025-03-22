using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CustomerOrders.Application.DTOs;
using CustomerOrders.Application.DTOs.EntityDtos;
using CustomerOrders.Application.DTOs.RequestDtos;
using CustomerOrders.Application.Helpers;
using CustomerOrders.Application.Interfaces;
using CustomerOrders.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace CustomerOrderApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly PasswordHasher<Customer> _passwordHasher;

        public AuthController(IConfiguration configuration, IUserService userService)
        {
            _configuration = configuration;
            _userService = userService;
            _passwordHasher = new PasswordHasher<Customer>();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var usernameValidation = ValidationHelper.ValidateUsername(request.Username);
            if (!string.IsNullOrEmpty(usernameValidation))
                return BadRequest(new ApiResponseDto<string>(400, false, usernameValidation));

            var emailValidation = ValidationHelper.ValidateEmail(request.Email);
            if (!string.IsNullOrEmpty(emailValidation))
                return BadRequest(new ApiResponseDto<string>(400, false, emailValidation));

            var existingCustomerByUsername = await _userService.GetUserByUsernameAsync(request.Username);
            if (existingCustomerByUsername != null)
            {
                return BadRequest(new ApiResponseDto<string>(400, false, "This username is already taken."));
            }

            var existingCustomerByEmail = await _userService.GetUserByEmailAsync(request.Email);
            if (existingCustomerByEmail != null)
            {
                return BadRequest(new ApiResponseDto<string>(400, false, "This email is already registered."));
            }

            var customer = new Customer
            {
                Username = request.Username,
                Email = request.Email,
                Name = request.Username,
                Address = "Address Information will be overwritten",
                PasswordHash = "will be overwritten"
            };

            await _userService.RegisterAsync(customer, request.Password);
            return Ok(new ApiResponseDto<string>(200, true, "Registration is successful."));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var valid = await _userService.ValidateUserAsync(request.Username, request.Password);
            if (!valid)
            {
                return Unauthorized(new ApiResponseDto<string>(401, false, "Invalid username or password."));
            }

            var token = GenerateJwtToken(request.Username);
            return Ok(new ApiResponseDto<string>(200, true, "Login successful.", token));
        }

        private string GenerateJwtToken(string username)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Convert.FromBase64String(jwtSettings["Key"]!);
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, username) }),
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["DurationInMinutes"])),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        [Authorize]
        [HttpPut("update-password")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequest request)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return Unauthorized(new ApiResponseDto<string>(401, false, "User identity not found."));

            var user = await _userService.GetUserByUsernameAsync(username);
            if (user == null) return NotFound(new ApiResponseDto<string>(404, false, "User not found."));

            var currentPasswordCheck = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);
            if (currentPasswordCheck == PasswordVerificationResult.Failed)
                return BadRequest(new ApiResponseDto<string>(400, false, "Current password is incorrect."));

            var newPasswordCheck = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.NewPassword);
            if (newPasswordCheck == PasswordVerificationResult.Success)
                return BadRequest(new ApiResponseDto<string>(400, false, "New password cannot be the same as the current password."));

            user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
            await _userService.UpdateUserAsync(user);

            return Ok(new ApiResponseDto<string>(200, true, "Password updated successfully."));
        }
    }
}
