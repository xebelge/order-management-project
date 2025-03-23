using CustomerOrders.Application.DTOs;
using CustomerOrders.Application.DTOs.RequestDtos;
using CustomerOrders.Application.Interfaces;
using CustomerOrders.Core.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CustomerOrderApi.Controllers
{
    /// <summary>
    /// Manages customer registration, login, and password updates.
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
        private readonly IValidator<UpdatePasswordRequest> _updatePasswordValidator;
        private readonly ILogger<AuthController> _logger;
        private readonly TokenService _tokenService;

        public AuthController(
            IConfiguration configuration,
            IUserService userService,
            IValidator<RegisterRequest> registerValidator,
            IValidator<LoginRequest> loginValidator,
            IValidator<UpdatePasswordRequest> updatePasswordValidator,
            ILogger<AuthController> logger,
            TokenService tokenService)
        {
            _configuration = configuration;
            _userService = userService;
            _passwordHasher = new PasswordHasher<Customer>();
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
            _updatePasswordValidator = updatePasswordValidator;
            _logger = logger;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Registers a new user if username and email are unique (and not marked deleted).
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var validation = await _registerValidator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                var firstError = validation.Errors.First().ErrorMessage;
                _logger.LogWarning("Registration validation failed: {Error}", firstError);
                return BadRequest(new ApiResponseDto<string>(400, false, firstError));
            }

            request.Email = request.Email.ToLowerInvariant();

            var usernameExists = await _userService.GetUserByUsernameAsync(request.Username);
            if (usernameExists is { IsDeleted: false })
            {
                return BadRequest(new ApiResponseDto<string>(400, false, "This username is already taken."));
            }

            var emailExists = await _userService.GetUserByEmailAsync(request.Email);
            if (emailExists is { IsDeleted: false })
            {
                return BadRequest(new ApiResponseDto<string>(400, false, "This email is already registered."));
            }

            var customer = new Customer
            {
                Username = request.Username,
                Email = request.Email,
                Name = request.Username,
                Address = "Address will be updated later",
                PasswordHash = "placeholder"
            };

            await _userService.RegisterAsync(customer, request.Password);
            _logger.LogInformation("User registered successfully: {Username}", request.Username);

            return Ok(new ApiResponseDto<string>(200, true, "Registration successful."));
        }

        /// <summary>
        /// Logs in a valid customer and returns JWT token.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var validation = await _loginValidator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                var firstError = validation.Errors.First().ErrorMessage;
                _logger.LogWarning("Login validation failed: {Error}", firstError);
                return BadRequest(new ApiResponseDto<string>(400, false, firstError));
            }

            var user = await _userService.GetUserByUsernameAsync(request.Username);
            if (user == null || user.IsDeleted)
            {
                return Unauthorized(new ApiResponseDto<string>(401, false, "Invalid username or password."));
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (result != PasswordVerificationResult.Success)
            {
                return Unauthorized(new ApiResponseDto<string>(401, false, "Invalid username or password."));
            }

            var token = _tokenService.GenerateJwtToken(user.Username);
            return Ok(new ApiResponseDto<string>(200, true, "Login successful.", token));
        }

        /// <summary>
        /// Updates the password for the logged-in user after verifying current password.
        /// </summary>
        [Authorize]
        [HttpPut("update-password")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequest request)
        {
            var validation = await _updatePasswordValidator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                var firstError = validation.Errors.First().ErrorMessage;
                return BadRequest(new ApiResponseDto<string>(400, false, firstError));
            }

            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized(new ApiResponseDto<string>(401, false, "User identity not found."));

            var user = await _userService.GetUserByUsernameAsync(username);
            if (user == null)
                return NotFound(new ApiResponseDto<string>(404, false, "User not found."));

            var currentPasswordValid = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);
            if (currentPasswordValid == PasswordVerificationResult.Failed)
            {
                return BadRequest(new ApiResponseDto<string>(400, false, "Current password is incorrect."));
            }

            var newPasswordSame = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.NewPassword);
            if (newPasswordSame == PasswordVerificationResult.Success)
            {
                return BadRequest(new ApiResponseDto<string>(400, false, "New password must be different."));
            }

            user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
            await _userService.UpdateUserAsync(user);

            return Ok(new ApiResponseDto<string>(200, true, "Password updated successfully."));
        }
    }
}
