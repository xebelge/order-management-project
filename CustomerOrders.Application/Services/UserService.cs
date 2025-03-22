using CustomerOrders.Application.Interfaces;
using CustomerOrders.Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace CustomerOrders.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<Customer> _customerRepository;
        private readonly PasswordHasher<Customer> _passwordHasher;

        public UserService(IRepository<Customer> customerRepository)
        {
            _customerRepository = customerRepository;
            _passwordHasher = new PasswordHasher<Customer>();
        }

        public async Task<Customer?> GetUserByUsernameAsync(string username)
        {
            var customers = await _customerRepository.GetAllAsync();
            return customers.FirstOrDefault(c => c.Username.ToLower() == username.ToLower());
        }

        public async Task<Customer?> GetUserByEmailAsync(string email)
        {
            var customers = await _customerRepository.GetAllAsync();
            return customers.FirstOrDefault(c => c.Email.ToLower() == email.ToLower());
        }

        public async Task<Customer> RegisterAsync(Customer customer, string password)
        {
            customer.PasswordHash = _passwordHasher.HashPassword(customer, password);
            await _customerRepository.AddAsync(customer);
            return customer;
        }

        public async Task<bool> ValidateUserAsync(string username, string password)
        {
            var customer = await GetUserByUsernameAsync(username);
            if (customer == null)
                return false;

            var result = _passwordHasher.VerifyHashedPassword(customer, customer.PasswordHash, password);
            return result == PasswordVerificationResult.Success;
        }

        public async Task UpdateUserAsync(Customer customer)
        {
            await _customerRepository.UpdateAsync(customer);
        }

    }
}
