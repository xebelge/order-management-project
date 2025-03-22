using CustomerOrders.Application.Helpers.Mappers;
using CustomerOrders.Application.Services.RabbitMQ;
using CustomerOrders.Core.Entities;
using CustomerOrders.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CustomerOrders.Application.Services
{
    /// <summary>
    /// Manages customer data, updates, and notifications.
    /// </summary>
    public class CustomerService
    {
        private readonly IRepository<Customer> _repository;
        private readonly RabbitMqService _rabbitMqService;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(
            IRepository<Customer> repository,
            RabbitMqService rabbitMqService,
            ILogger<CustomerService> logger)
        {
            _repository = repository;
            _rabbitMqService = rabbitMqService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all customers, active or not.
        /// </summary>
        public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
        {
            try
            {
                var customers = await _repository.Query().ToListAsync();
                _logger.LogInformation("Retrieved all customers. Count: {Count}", customers.Count);
                return customers.Select(c => c.ToDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching all customers.");
                return Enumerable.Empty<CustomerDto>();
            }
        }

        /// <summary>
        /// Fetches a single customer by ID.
        /// </summary>
        public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
        {
            try
            {
                var customer = await _repository.Query().FirstOrDefaultAsync(c => c.Id == id);
                if (customer == null)
                {
                    _logger.LogWarning("Customer not found. ID: {CustomerId}", id);
                    return null;
                }
                return customer.ToDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving customer ID {CustomerId}", id);
                return null;
            }
        }

        /// <summary>
        /// Fetches a customer directly by email.
        /// </summary>
        public async Task<Customer?> GetCustomerByEmailAsync(string email)
        {
            try
            {
                return await _repository.Query().FirstOrDefaultAsync(c => c.Email == email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching customer by email: {Email}", email);
                return null;
            }
        }

        /// <summary>
        /// Finds a customer by username and returns a DTO.
        /// </summary>
        public async Task<CustomerDto> GetUserByUsernameAsync(string username)
        {
            try
            {
                var customer = await _repository.Query().FirstOrDefaultAsync(c => c.Username == username);
                if (customer == null)
                {
                    _logger.LogWarning("Customer with username {Username} not found.", username);
                    return null;
                }
                return customer.ToDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer by username {Username}", username);
                return null;
            }
        }

        /// <summary>
        /// Updates an existing customer's fields and broadcasts an update message.
        /// </summary>
        public async Task UpdateUserAsync(CustomerDto customerDto)
        {
            try
            {
                var customer = await _repository.GetByIdAsync(customerDto.Id);
                if (customer == null)
                {
                    _logger.LogWarning("Customer ID {CustomerId} not found for update.", customerDto.Id);
                    throw new KeyNotFoundException("Customer not found.");
                }

                customer.Name = customerDto.Name;
                customer.Address = customerDto.Address;
                customer.Email = customerDto.Email;

                await _repository.UpdateAsync(customer);

                var message = $"[UPDATE] Customer updated: {customer.Name} (ID: {customer.Id}, Email: {customer.Email})";
                _rabbitMqService.SendMessage(message);
                _logger.LogInformation(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer ID {CustomerId}", customerDto.Id);
                throw;
            }
        }

        /// <summary>
        /// Marks a customer as deleted so it can no longer be used.
        /// </summary>
        public async Task DeleteUserAsync(int customerId)
        {
            var customer = await _repository.GetByIdAsync(customerId);
            if (customer != null)
            {
                customer.IsDeleted = true;
                await _repository.UpdateAsync(customer);
            }
        }

        /// <summary>
        /// Looks up a user by email among all customers in the repo.
        /// </summary>
        public async Task<Customer?> GetUserByEmailAsync(string email)
        {
            var customers = await _repository.GetAllAsync();
            return customers.FirstOrDefault(c => c.Email.ToLower() == email.ToLower());
        }
    }
}
