using System.ComponentModel.DataAnnotations;

namespace CustomerOrders.Core.Entities
{
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
    }
}
