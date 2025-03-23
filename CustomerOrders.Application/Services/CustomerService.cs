using AutoMapper;
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
        private readonly IMapper _mapper;

        public CustomerService(
            IRepository<Customer> repository,
            RabbitMqService rabbitMqService,
            ILogger<CustomerService> logger,
            IMapper mapper)
        {
            _repository = repository;
            _rabbitMqService = rabbitMqService;
            _logger = logger;
            _mapper = mapper;
        }

        /// </summary>
        public async Task<CustomerDto?> GetUserByUsernameAsync(string username)
        {
            try
            {
                var customer = await _repository.Query()
                    .FirstOrDefaultAsync(c => c.Username == username && !c.IsDeleted);

                return customer == null ? null : _mapper.Map<CustomerDto>(customer);
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
        /// Delete and marks a customer as deleted so it can no longer be used.
        /// </summary>
        public async Task DeleteUserAsync(int customerId)
        {
            var customer = await _repository.GetByIdAsync(customerId);

            if (customer == null)
            {
                _logger.LogWarning("Delete failed. Customer ID {CustomerId} not found.", customerId);
                return;
            }

            if (customer.IsDeleted)
            {
                _logger.LogInformation("Customer ID {CustomerId} is already marked as deleted.", customerId);
                return;
            }

            customer.IsDeleted = true;
            await _repository.UpdateAsync(customer);

            var message = $"[DELETE] Customer marked as deleted: {customer.Username} (ID: {customer.Id})";
            _rabbitMqService.SendMessage(message);
            _logger.LogInformation(message);
        }

        /// <summary>
        /// Looks up a customer by email among all customers in the repo.
        /// </summary>
        public async Task<Customer?> GetUserByEmailAsync(string email)
        {
            var customers = await _repository.GetAllAsync();
            return customers.FirstOrDefault(c => c.Email.ToLower() == email.ToLower());
        }
    }
}
