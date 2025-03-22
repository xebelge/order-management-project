using CustomerOrders.Core.Entities;

namespace CustomerOrders.Application.Helpers
{
    public static class ValidationHelper
    {
        public static string? ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "Name cannot be empty.";
            }
            if (name.Length < 2 || name.Length > 50)
            {
                return "Name must be between 2 and 50 characters.";
            }
            if (!name.All(c => char.IsLetter(c) || c == ' '))
            {
                return "Name must only contain letters and spaces.";
            }
            return null;
        }

        public static string? ValidateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return "Username cannot be empty.";
            }

            return null;
        }

        public static string? ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return "Email cannot be empty.";
            }

            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!System.Text.RegularExpressions.Regex.IsMatch(email, emailPattern))
            {
                return "Invalid email format.";
            }

            return null;
        }
        public static bool IsValidUsername(string username)
        {
            return !string.IsNullOrWhiteSpace(username) && username.Length >= 3;
        }

        public static string? ValidateCustomerId(int customerId)
        {
            if (customerId < 0)
            {
                return "Invalid CustomerId. It cannot be negative.";
            }
            return null;
        }

        public static string? ValidateProductItems(List<AddOrderProductItemRequest> productItems)
        {
            foreach (var item in productItems)
            {
                if (item.ProductId < 0 || item.Quantity < 0)
                {
                    return "ProductId and Quantity cannot be negative.";
                }
            }
            return null;
        }

        public static string? ValidateProductId(int productId)
        {
            if (productId < 0)
            {
                return "Product ID cannot be negative.";
            }
            return null;
        }

        public static string? ValidateOrderId(int orderId)
        {
            if (orderId < 0)
            {
                return "Order ID cannot be negative.";
            }
            return null;
        }

        public static string? ValidateQuantity(int quantity)
        {
            if (quantity < 0)
            {
                return "Quantity cannot be negative.";
            }
            return null;
        }

        public static string? ValidateProduct(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.Barcode))
                return "Product Barcode cannot be empty.";
            if (string.IsNullOrWhiteSpace(product.Description))
                return "Product Description cannot be empty.";
            if (product.Quantity <= 0)
                return "Product Quantity must be greater than zero.";
            if (product.Price <= 0)
                return "Product Price must be greater than zero.";

            return null;
        }

    }
}
