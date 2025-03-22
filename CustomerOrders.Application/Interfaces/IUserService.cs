using CustomerOrders.Core.Entities;

namespace CustomerOrders.Application.Interfaces
{
    public interface IUserService
    {
        Task<Customer?> GetUserByUsernameAsync(string username);
        Task<Customer> GetUserByEmailAsync(string email);
        Task<Customer> RegisterAsync(Customer customer, string password);
        Task<bool> ValidateUserAsync(string username, string password);
        Task UpdateUserAsync(Customer customer);
    }
}
