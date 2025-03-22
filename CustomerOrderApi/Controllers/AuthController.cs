using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CustomerOrders.Application.DTOs;
using CustomerOrders.Application.DTOs.EntityDtos;
using CustomerOrders.Application.DTOs.RequestDtos;
using CustomerOrders.Application.Interfaces;
using CustomerOrders.Core.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace CustomerOrderApi.Controllers
{
    /// <summary>
    /// Handles user authentication and authorization processes.
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly PasswordHasher<Customer> _passwordHasher;
        private readonly IValidator<RegisterRequest> _registerValidator;
        private readonly IValidator<LoginRequest> _loginValidator;

        public AuthController(
            IConfiguration configuration,
            IUserService userService,
            IValidator<RegisterRequest> registerValidator,
            IValidator<LoginRequest> loginValidator)
        {
            _configuration = configuration;
            _userService = userService;
            _passwordHasher = new PasswordHasher<Customer>();
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
        }

        /// <summary>
        /// Registers a new user with the given username and email.
        /// </summary>
        /// <remarks>
        /// Returns 400 if username/email is taken or invalid. 
        /// Email is lowered, password is hashed before saving.
        /// </remarks>
        /// <param name="request">The user's registration data.</param>
        /// <returns>API response indicating success or error.</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var validation = _registerValidator.Validate(request);
            if (!validation.IsValid)
                return BadRequest(new ApiResponseDto<string>(400, false, validation.Errors[0].ErrorMessage));

            request.Email = request.Email.ToLowerInvariant();

            var existingCustomerByUsername = await _userService.GetUserByUsernameAsync(request.Username);
            if (existingCustomerByUsername != null)
                return BadRequest(new ApiResponseDto<string>(400, false, "This username is already taken."));

            var existingCustomerByEmail = await _userService.GetUserByEmailAsync(request.Email);
            if (existingCustomerByEmail is { IsDeleted: false })
                return BadRequest(new ApiResponseDto<string>(400, false, "This email is already registered."));

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

        /// <summary>
        /// Authenticates a user by validating their credentials.
        /// </summary>
        /// <remarks>
        /// Returns 401 if credentials are invalid or user is deleted. 
        /// Creates and returns a JWT if successful.
        /// </remarks>
        /// <param name="request">The login data (username, password).</param>
        /// <returns>API response with JWT token if valid.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var validation = _loginValidator.Validate(request);
            if (!validation.IsValid)
                return BadRequest(new ApiResponseDto<string>(400, false, validation.Errors[0].ErrorMessage));

            var user = await _userService.GetUserByUsernameAsync(request.Username);
            if (user == null || user.IsDeleted)
                return Unauthorized(new ApiResponseDto<string>(401, false, "Invalid username or password."));

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (result != PasswordVerificationResult.Success)
                return Unauthorized(new ApiResponseDto<string>(401, false, "Invalid username or password."));

            var token = GenerateJwtToken(user.Username);
            return Ok(new ApiResponseDto<string>(200, true, "Login successful.", token));
        }

        /// <summary>
        /// Updates the user's current password with a new one.
        /// </summary>
        /// <remarks>
        /// Requires a valid JWT. Checks old password before setting the new one.
        /// </remarks>
        /// <param name="request">The old and new password data.</param>
        /// <returns>200 if successfully updated, otherwise appropriate error response.</returns>
        [Authorize]
        [HttpPut("update-password")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequest request)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized(new ApiResponseDto<string>(401, false, "User identity not found."));

            var user = await _userService.GetUserByUsernameAsync(username);
            if (user == null)
                return NotFound(new ApiResponseDto<string>(404, false, "User not found."));

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

        /// <summary>
        /// Generates a JWT for the given username.
        /// </summary>
        /// <remarks>
        /// Used internally by the controller.
        /// </remarks>
        /// <param name="username">The username to embed in the token.</param>
        /// <returns>A JWT string.</returns>
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
    }
}
