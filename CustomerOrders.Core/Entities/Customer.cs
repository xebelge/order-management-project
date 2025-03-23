using System.ComponentModel.DataAnnotations;

namespace CustomerOrders.Core.Entities
{
    /// <summary>
    /// Represents an customer with personal info, address, and order history.
    /// </summary>
    public class Customer: BaseEntity
    {
        public int Id { get; set; }

        [Required]
        public required string Username { get; set; }

        [Required]
        public required string Name { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }
        [Required]
        public required string PasswordHash { get; set; }
        [Required]
        public required string Address { get; set; }

        public ICollection<CustomerOrder> Orders { get; set; } = new List<CustomerOrder>();
        public bool IsDeleted { get; set; } = false;
    }
}
