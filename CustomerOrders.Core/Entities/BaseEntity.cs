namespace CustomerOrders.Core.Entities
{
    /// <summary>
    /// Serves as the base for all entities, providing common audit fields.
    /// </summary>
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
