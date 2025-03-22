using CustomerOrders.Core.Entities;

namespace CustomerOrders.Application.Helpers.Mappers
{
    public static class CustomerMapper
    {
        /// <summary>
        /// Converts a Customer entity into a DTO representation.
        /// </summa
        public static CustomerDto ToDto(this Customer c)
        {
            return new CustomerDto
            {
                Id = c.Id,
                Username = c.Username,
                Name = c.Name,
                Email = c.Email,
                Address = c.Address,
                IsDeleted = c.IsDeleted
            };
        }
    }
}
