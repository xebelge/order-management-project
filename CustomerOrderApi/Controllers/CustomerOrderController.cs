using CustomerOrders.Application.DTOs;
using CustomerOrders.Application.Services;
using CustomerOrders.Core.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerOrderApi.Controllers
{
    /// <summary>
    /// Manages operations related to customer orders and associated products.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/customerorders")]
    public class CustomerOrderController : ControllerBase
    {
        private readonly CustomerOrderService _customerOrderService;
        private readonly ProductService _productService;
        private readonly IValidator<int> _idValidator;
        private readonly IValidator<AddOrderProductItemRequest> _orderItemValidator;
        private readonly IValidator<List<AddOrderProductItemRequest>> _orderItemsValidator;
        private readonly IValidator<int> _quantityValidator;

        public CustomerOrderController(
            CustomerOrderService customerOrderService,
            ProductService productService,
            IValidator<int> idValidator,
            IValidator<AddOrderProductItemRequest> orderItemValidator,
            IValidator<List<AddOrderProductItemRequest>> orderItemsValidator,
            IValidator<int> quantityValidator)
        {
            _customerOrderService = customerOrderService;
            _productService = productService;
            _idValidator = idValidator;
            _orderItemValidator = orderItemValidator;
            _orderItemsValidator = orderItemsValidator;
            _quantityValidator = quantityValidator;
        }

        /// <summary>
        /// Retrieves all existing orders along with their product details.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await _customerOrderService.GetAllOrdersAsync();

            var orderDtos = orders.Select(order => new CustomerOrderDto
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                Address = order.Address,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                TotalAmount = order.TotalAmount,
                Products = order.Products.Select(cop =>
                {
                    var product = _productService.GetProductByIdAsync(cop.ProductId).Result;
                    return new CustomerOrderProductDto
                    {
                        ProductId = cop.ProductId,
                        ProductQuantity = cop.ProductQuantity,
                        Barcode = product?.Barcode,
                        Description = product?.Description,
                        Price = product?.Price ?? 0
                    };
                }).ToList()
            }).ToList();

            return Ok(new ApiResponseDto<IEnumerable<CustomerOrderDto>>(200, true, "Orders retrieved successfully", orderDtos));
        }

        /// <summary>
        /// Retrieves a specific order by its ID.
        /// </summary>
        /// <param name="id">Unique order ID.</param>
        /// <returns>The order details or an error if not found.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var result = _idValidator.Validate(id);
            if (!result.IsValid)
                return BadRequest(new ApiResponseDto<string>(400, false, result.Errors[0].ErrorMessage));

            var order = await _customerOrderService.GetOrderDtoById(id);
            if (order == null)
                return NotFound(new ApiResponseDto<string>(404, false, "Order not found"));

            return Ok(new ApiResponseDto<CustomerOrderDto>(200, true, "Order retrieved successfully", order));
        }

        /// <summary>
        /// Creates new orders for one or more customers.
        /// </summary>
        /// <param name="requests">List of CreateOrderRequest data.</param>
        /// <returns>Details of added orders or validation errors.</returns>
        [HttpPost]
        public async Task<IActionResult> AddOrders([FromBody] List<CreateOrderRequest> requests)
        {
            foreach (var request in requests)
            {
                var idResult = _idValidator.Validate(request.CustomerId);
                if (!idResult.IsValid)
                    return BadRequest(new ApiResponseDto<string>(400, false, idResult.Errors[0].ErrorMessage));

                var itemsResult = _orderItemsValidator.Validate(request.ProductItems);
                if (!itemsResult.IsValid)
                    return BadRequest(new ApiResponseDto<string>(400, false, itemsResult.Errors[0].ErrorMessage));
            }

            var orders = requests.Select(r => new CustomerOrder
            {
                CustomerId = r.CustomerId,
                Address = r.Address,
                CustomerOrderProducts = r.ProductItems.Select(item => new CustomerOrderProduct
                {
                    ProductId = item.ProductId,
                    ProductQuantity = item.Quantity
                }).ToList(),
                CreatedAt = DateTime.UtcNow
            }).ToList();

            foreach (var order in orders)
            {
                await _customerOrderService.AddOrderAsync(order);
            }

            var orderDtos = orders.Select(order => new CustomerOrderDto
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                Address = order.Address,
                CreatedAt = order.CreatedAt,
                Products = order.CustomerOrderProducts.Select(p => new CustomerOrderProductDto
                {
                    ProductId = p.ProductId,
                    ProductQuantity = p.ProductQuantity,
                }).ToList()
            }).ToList();

            return CreatedAtAction(nameof(GetOrders), null, new ApiResponseDto<IEnumerable<CustomerOrderDto>>(201, true, "Orders added successfully", orderDtos));
        }

        /// <summary>
        /// Removes an order by its ID.
        /// </summary>
        /// <param name="id">Order ID to remove.</param>
        /// <returns>NoContent or a validation error.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var result = _idValidator.Validate(id);
            if (!result.IsValid)
                return BadRequest(new ApiResponseDto<string>(400, false, result.Errors[0].ErrorMessage));

            await _customerOrderService.DeleteOrderAsync(id);
            return NoContent();
        }

        /// <summary>
        /// Adds a product to an existing order.
        /// </summary>
        /// <param name="orderId">The order to update.</param>
        /// <param name="request">Details of the product being added.</param>
        /// <returns>NoContent if successful, or error if invalid.</returns>
        [HttpPost("{orderId}/products")]
        public async Task<IActionResult> AddProductToOrder(int orderId, [FromBody] AddProductToOrderRequest request)
        {
            if (!_idValidator.Validate(orderId).IsValid ||
                !_idValidator.Validate(request.ProductId).IsValid ||
                !_quantityValidator.Validate(request.Quantity).IsValid)
            {
                return BadRequest(new ApiResponseDto<string>(400, false, "Invalid input."));
            }

            var order = await _customerOrderService.GetOrderByIdAsync(orderId);
            if (order == null)
                return NotFound(new ApiResponseDto<string>(404, false, "Order not found"));

            if (order.CustomerOrderProducts.Any(cop => cop.ProductId == request.ProductId))
                return Conflict(new ApiResponseDto<string>(409, false, "Product already exists in this order"));

            order.CustomerOrderProducts.Add(new CustomerOrderProduct
            {
                ProductId = request.ProductId,
                ProductQuantity = request.Quantity
            });

            await _customerOrderService.UpdateOrderAsync(order);
            return NoContent();
        }

        /// <summary>
        /// Removes a product from an existing order.
        /// </summary>
        /// <param name="orderId">Order ID.</param>
        /// <param name="productId">Product ID to remove.</param>
        /// <returns>NoContent if successful, or error if invalid.</returns>
        [HttpDelete("{orderId}/products/{productId}")]
        public async Task<IActionResult> RemoveProductFromOrder(int orderId, int productId)
        {
            if (!_idValidator.Validate(orderId).IsValid || !_idValidator.Validate(productId).IsValid)
                return BadRequest(new ApiResponseDto<string>(400, false, "Invalid IDs"));

            var order = await _customerOrderService.GetOrderByIdAsync(orderId);
            if (order == null)
                return NotFound(new ApiResponseDto<string>(404, false, "Order not found"));

            var productLink = order.CustomerOrderProducts.FirstOrDefault(cop => cop.ProductId == productId);
            if (productLink == null)
                return NotFound(new ApiResponseDto<string>(404, false, "Product not found in this order"));

            order.CustomerOrderProducts.Remove(productLink);
            await _customerOrderService.UpdateOrderAsync(order);
            return NoContent();
        }

        /// <summary>
        /// Updates the delivery address of a specific order.
        /// </summary>
        /// <param name="orderId">Order ID.</param>
        /// <param name="request">New address data.</param>
        /// <returns>NoContent on success, error if invalid.</returns>
        [HttpPut("{orderId}/address")]
        public async Task<IActionResult> UpdateOrderAddress(int orderId, [FromBody] UpdateOrderAddressRequest request)
        {
            if (!_idValidator.Validate(orderId).IsValid)
                return BadRequest(new ApiResponseDto<string>(400, false, "Invalid order ID"));

            var order = await _customerOrderService.GetOrderByIdAsync(orderId);
            if (order == null)
                return NotFound(new ApiResponseDto<string>(404, false, "Order not found"));

            order.Address = request.Address;
            await _customerOrderService.UpdateOrderAsync(order);
            return NoContent();
        }

        /// <summary>
        /// Updates the quantity of a product within an existing order.
        /// </summary>
        /// <param name="orderId">Order ID.</param>
        /// <param name="productId">Product ID to update.</param>
        /// <param name="request">New quantity value.</param>
        /// <returns>NoContent if updated, error if invalid or insufficient stock.</returns>
        [HttpPatch("{orderId}/products/{productId}/quantity")]
        public async Task<IActionResult> UpdateProductQuantityInOrder(int orderId, int productId, [FromBody] UpdateOrderProductQuantityRequest request)
        {
            if (!_idValidator.Validate(orderId).IsValid ||
                !_idValidator.Validate(productId).IsValid ||
                !_quantityValidator.Validate(request.Quantity).IsValid)
            {
                return BadRequest(new ApiResponseDto<string>(400, false, "Invalid input"));
            }

            var order = await _customerOrderService.GetOrderByIdAsync(orderId);
            if (order == null)
                return NotFound(new ApiResponseDto<string>(404, false, "Order not found"));

            var productLink = order.CustomerOrderProducts.FirstOrDefault(cop => cop.ProductId == productId);
            if (productLink == null)
                return NotFound(new ApiResponseDto<string>(404, false, "Product not found in this order"));

            var product = await _customerOrderService.GetProductByIdAsync(productId);
            if (product == null)
                return NotFound(new ApiResponseDto<string>(404, false, "Product not found"));

            if (request.Quantity > product.Quantity)
                return BadRequest(new ApiResponseDto<string>(400, false, $"Insufficient stock. Available: {product.Quantity}"));

            productLink.ProductQuantity = request.Quantity;
            await _customerOrderService.UpdateOrderAsync(order);
            return NoContent();
        }
    }
}
