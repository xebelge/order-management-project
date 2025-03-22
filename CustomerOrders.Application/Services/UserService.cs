using CustomerOrders.Application.Interfaces;
using CustomerOrders.Core.Entities;
using CustomerOrders.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace CustomerOrders.Application.Services
{
    /// <summary>
    /// Handles user registration, validation, and profile updates.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IRepository<Customer> _customerRepository;
        private readonly PasswordHasher<Customer> _passwordHasher;
        private readonly ILogger<UserService> _logger;

        public UserService(IRepository<Customer> customerRepository, ILogger<UserService> logger)
        {
            _customerRepository = customerRepository;
            _passwordHasher = new PasswordHasher<Customer>();
            _logger = logger;
        }

        /// <summary>
        /// Finds a user by username, ignoring case.
        /// </summary>
        /// <param name="username">The username to match.</param>
        /// <returns>A matching customer or null.</returns>
        public async Task<Customer?> GetUserByUsernameAsync(string username)
        {
            var customers = await _customerRepository.GetAllAsync();
            var user = customers.FirstOrDefault(c => c.Username.ToLower() == username.ToLower());

            if (user != null)
                _logger.LogInformation("User found by username: {Username}", username);
            else
                _logger.LogWarning("User not found by username: {Username}", username);

            return user;
        }

        /// <summary>
        /// Finds a user by email, ignoring case.
        /// </summary>
        /// <param name="email">The email address to match.</param>
        /// <returns>A matching customer or null.</returns>
        public async Task<Customer?> GetUserByEmailAsync(string email)
        {
            var customers = await _customerRepository.GetAllAsync();
            var user = customers.FirstOrDefault(c => c.Email.ToLower() == email.ToLower());

            if (user != null)
                _logger.LogInformation("User found by email: {Email}", email);
            else
                _logger.LogWarning("User not found by email: {Email}", email);

            return user;
        }

        /// <summary>
        /// Registers a new customer with a hashed password.
        /// </summary>
        /// <param name="customer">The customer entity to create.</param>
        /// <param name="password">Plain-text password to hash.</param>
        /// <returns>The newly registered customer entity.</returns>
        public async Task<Customer> RegisterAsync(Customer customer, string password)
        {
            customer.PasswordHash = _passwordHasher.HashPassword(customer, password);
            await _customerRepository.AddAsync(customer);
            _logger.LogInformation("New user registered: {Username}", customer.Username);
            return customer;
        }

        /// <summary>
        /// Checks if the provided username/password combo is valid.
        /// </summary>
        /// <param name="username">Username to validate.</param>
        /// <param name="password">Plain-text password.</param>
        /// <returns>True if valid, false otherwise.</returns>
        public async Task<bool> ValidateUserAsync(string username, string password)
        {
            var customer = await GetUserByUsernameAsync(username);
            if (customer == null)
            {
                _logger.LogWarning("Validation failed: user not found ({Username})", username);
                return false;
            }

            var result = _passwordHasher.VerifyHashedPassword(customer, customer.PasswordHash, password);
            var success = result == PasswordVerificationResult.Success;

            if (success)
                _logger.LogInformation("User validated successfully: {Username}", username);
            else
                _logger.LogWarning("Password validation failed for user: {Username}", username);

            return success;
        }

        /// <summary>
        /// Persists changes to an existing user's data.
        /// </summary>
        /// <param name="customer">The modified customer entity.</param>
        public async Task UpdateUserAsync(Customer customer)
        {
            await _customerRepository.UpdateAsync(customer);
            _logger.LogInformation("User updated: {Username}", customer.Username);
        }
    }
}
