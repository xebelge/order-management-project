using CustomerOrders.Application.Interfaces;
using CustomerOrders.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CustomerOrders.Application.Services
{
    public class CustomerService
    {
        private readonly IRepository<Customer> _repository;

        public CustomerService(IRepository<Customer> repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
        {
            var customers = await _repository.Query().ToListAsync();

            return customers.Select(c => new CustomerDto
            {
                Id = c.Id,
                Username = c.Username,
                Name = c.Name,
                Email = c.Email,
                Address = c.Address,
            });
        }

        public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
        {
            var c = await _repository.Query().FirstOrDefaultAsync(c => c.Id == id);

            if (c == null) return null;

            return new CustomerDto
            {
                Id = c.Id,
                Username = c.Username,
                Name = c.Name,
                Email = c.Email,
                Address = c.Address,
            };
        }

        public async Task<Customer?> GetCustomerByEmailAsync(string email)
        {
            return await _repository.Query()
                .FirstOrDefaultAsync(c => c.Email == email);
        }

        public async Task<CustomerDto> GetUserByUsernameAsync(string username)
        {
            var customer = await _repository.Query()
                .FirstOrDefaultAsync(c => c.Username == username);

            if (customer == null)
                return null;

            return new CustomerDto
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                Address = customer.Address,
                Username = customer.Username
            };
        }

        public async Task UpdateUserAsync(CustomerDto customerDto)
        {
            var customer = await _repository.GetByIdAsync(customerDto.Id);
            if (customer == null)
                throw new KeyNotFoundException("Customer not found.");

            customer.Name = customerDto.Name;
            customer.Address = customerDto.Address;
            customer.Email = customerDto.Email;

            await _repository.UpdateAsync(customer);
        }

    }
}
