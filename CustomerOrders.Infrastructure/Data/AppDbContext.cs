using CustomerOrders.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CustomerOrders.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<CustomerOrder> CustomerOrders { get; set; }
        public DbSet<CustomerOrderProduct> CustomerOrderProducts { get; set; }
        public DbSet<RequestLog> RequestLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CustomerOrder>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CustomerOrderProduct>()
                .HasKey(cop => cop.Id);

            modelBuilder.Entity<CustomerOrderProduct>()
                .HasOne(cop => cop.Product)
                .WithMany(p => p.CustomerOrderProducts)
                .HasForeignKey(cop => cop.ProductId);

            modelBuilder.Entity<CustomerOrderProduct>()
                .HasOne<CustomerOrder>() 
                .WithMany(o => o.CustomerOrderProducts)
                .HasForeignKey(cop => cop.CustomerOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.Username)
                .IsUnique();

            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.Email)
                .IsUnique();

            modelBuilder.Entity<Customer>()
                .Property(c => c.PasswordHash)
                .HasMaxLength(256);

            modelBuilder.Entity<CustomerOrder>()
                .Property(o => o.Address)
                .HasMaxLength(512);
        }
    }
}
