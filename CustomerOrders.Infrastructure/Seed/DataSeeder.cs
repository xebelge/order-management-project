using CustomerOrders.Core.Entities;
using CustomerOrders.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;

namespace CustomerOrders.Infrastructure.Seed
{
    /// <summary>
    /// Populates the database with initial customers, products, and orders if none exist.
    /// </summary>
    public static class DataSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            var passwordHasher = new PasswordHasher<Customer>();

            if (!context.Customers.Any())
            {
                var customers = new List<Customer>
                {
                    new Customer { Username = "johndoe", Name = "John Doe", Email = "john.doe@example.com", Address = "123 Main Street", PasswordHash = passwordHasher.HashPassword(null, "John123!") },
                    new Customer { Username = "janesmith", Name = "Jane Smith", Email = "jane.smith@example.com", Address = "456 Elm Street", PasswordHash = passwordHasher.HashPassword(null, "Jane123!") },
                    new Customer { Username = "michaelbrown", Name = "Michael Brown", Email = "michael.brown@example.com", Address = "789 Oak Street", PasswordHash = passwordHasher.HashPassword(null, "Michael123!") },
                    new Customer { Username = "emilyjohnson", Name = "Emily Johnson", Email = "emily.johnson@example.com", Address = "101 Pine Street", PasswordHash = passwordHasher.HashPassword(null, "Emily123!") }
                };

                context.Customers.AddRange(customers);
                await context.SaveChangesAsync();
            }

            if (!context.Products.Any())
            {
                var products = new List<Product>
                {
                    new Product { Barcode = "ABC123", Description = "Laptop", Quantity = 10, Price = 999.99m },
                    new Product { Barcode = "DEF456", Description = "Smartphone", Quantity = 20, Price = 699.99m },
                    new Product { Barcode = "GHI789", Description = "Tablet", Quantity = 15, Price = 499.99m },
                    new Product { Barcode = "JKL012", Description = "Smartwatch", Quantity = 30, Price = 199.99m },
                    new Product { Barcode = "MNO345", Description = "Headphones", Quantity = 25, Price = 149.99m },
                    new Product { Barcode = "PQR678", Description = "Keyboard", Quantity = 40, Price = 79.99m },
                    new Product { Barcode = "STU901", Description = "Mouse", Quantity = 50, Price = 49.99m }
                };

                context.Products.AddRange(products);
                await context.SaveChangesAsync();
            }

            if (!context.CustomerOrders.Any())
            {
                var customers = context.Customers.ToList();
                var products = context.Products.ToList();

                var orders = new List<CustomerOrder>
                {
                    new CustomerOrder
                    {
                        CustomerId = customers[0].Id,
                        Address = "123 Main Street",
                        CustomerOrderProducts = new List<CustomerOrderProduct>
                        {
                            new CustomerOrderProduct { ProductId = products[0].Id, ProductQuantity = 2 },
                            new CustomerOrderProduct { ProductId = products[1].Id, ProductQuantity = 3 }
                        }
                    },
                    new CustomerOrder
                    {
                        CustomerId = customers[3].Id,
                        Address = "456 Elm Street",
                        CustomerOrderProducts = new List<CustomerOrderProduct>
                        {
                            new CustomerOrderProduct { ProductId = products[4].Id, ProductQuantity = 1 }
                        }
                    },
                    new CustomerOrder
                    {
                        CustomerId = customers[2].Id,
                        Address = "789 Oak Street",
                        CustomerOrderProducts = new List<CustomerOrderProduct>
                        {
                            new CustomerOrderProduct { ProductId = products[2].Id, ProductQuantity = 2 }
                        }
                    },
                    new CustomerOrder
                    {
                        CustomerId = customers[1].Id,
                        Address = "101 Pine Street",
                        CustomerOrderProducts = new List<CustomerOrderProduct>
                        {
                            new CustomerOrderProduct { ProductId = products[5].Id, ProductQuantity = 1 },
                            new CustomerOrderProduct { ProductId = products[6].Id, ProductQuantity = 2 }
                        }
                    }
                };

                context.CustomerOrders.AddRange(orders);
                await context.SaveChangesAsync();
            }
        }
    }
}
