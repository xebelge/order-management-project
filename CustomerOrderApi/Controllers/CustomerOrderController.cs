using CustomerOrders.Application.DTOs;
using CustomerOrders.Application.Helpers;
using CustomerOrders.Application.Services;
using CustomerOrders.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerOrderApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/customerorders")]
    public class CustomerOrderController : ControllerBase
    {
        private readonly CustomerOrderService _customerOrderService;
        private readonly ProductService _productService;

        public CustomerOrderController(CustomerOrderService customerOrderService, ProductService productService)
        {
            _customerOrderService = customerOrderService;
            _productService = productService;
        }

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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var validationError = ValidationHelper.ValidateOrderId(id);
            if (!string.IsNullOrEmpty(validationError))
            {
                return BadRequest(new ApiResponseDto<string>(400, false, validationError));
            }

            var order = await _customerOrderService.GetOrderDtoById(id);
            if (order == null)
            {
                return NotFound(new ApiResponseDto<string>(404, false, "Order not found"));
            }

            return Ok(new ApiResponseDto<CustomerOrderDto>(200, true, "Order retrieved successfully", order));
        }

        [HttpPost]
        public async Task<IActionResult> AddOrders([FromBody] List<CreateOrderRequest> requests)
        {
            foreach (var request in requests)
            {
                var customerValidation = ValidationHelper.ValidateCustomerId(request.CustomerId);
                if (!string.IsNullOrEmpty(customerValidation))
                {
                    return BadRequest(new ApiResponseDto<string>(400, false, customerValidation));
                }

                var productItemsValidation = ValidationHelper.ValidateProductItems(request.ProductItems);
                if (!string.IsNullOrEmpty(productItemsValidation))
                {
                    return BadRequest(new ApiResponseDto<string>(400, false, productItemsValidation));
                }
            }

            var orders = requests.Select(request => new CustomerOrder
            {
                CustomerId = request.CustomerId,
                Address = request.Address,
                CustomerOrderProducts = request.ProductItems
                    .Select(item => new CustomerOrderProduct
                    {
                        ProductId = item.ProductId,
                        ProductQuantity = item.Quantity
                    })
                    .ToList(),
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
                Products = order.CustomerOrderProducts
                    .Select(p => new CustomerOrderProductDto
                    {
                        ProductId = p.ProductId,
                        ProductQuantity = p.ProductQuantity,
                    })
                    .ToList()
            }).ToList();

            return CreatedAtAction(nameof(GetOrders), null, new ApiResponseDto<IEnumerable<CustomerOrderDto>>(201, true, "Orders added successfully", orderDtos));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var validationError = ValidationHelper.ValidateOrderId(id);
            if (!string.IsNullOrEmpty(validationError))
            {
                return BadRequest(new ApiResponseDto<string>(400, false, validationError));
            }

            await _customerOrderService.DeleteOrderAsync(id);
            return NoContent();
        }

        [HttpPost("{orderId}/products")]
        public async Task<IActionResult> AddProductToOrder(int orderId, [FromBody] AddProductToOrderRequest request)
        {
            var orderValidation = ValidationHelper.ValidateOrderId(orderId);
            if (!string.IsNullOrEmpty(orderValidation))
            {
                return BadRequest(new ApiResponseDto<string>(400, false, orderValidation));
            }

            var productValidation = ValidationHelper.ValidateProductId(request.ProductId);
            if (!string.IsNullOrEmpty(productValidation))
            {
                return BadRequest(new ApiResponseDto<string>(400, false, productValidation));
            }

            var quantityValidation = ValidationHelper.ValidateQuantity(request.Quantity);
            if (!string.IsNullOrEmpty(quantityValidation))
            {
                return BadRequest(new ApiResponseDto<string>(400, false, quantityValidation));
            }

            var order = await _customerOrderService.GetOrderByIdAsync(orderId);
            if (order == null) return NotFound(new ApiResponseDto<string>(404, false, "Order not found"));

            var productExists = order.CustomerOrderProducts
                .Any(cop => cop.ProductId == request.ProductId);

            if (productExists) return Conflict(new ApiResponseDto<string>(409, false, "Product already exists in this order"));

            var newProductLink = new CustomerOrderProduct
            {
                ProductId = request.ProductId,
                ProductQuantity = request.Quantity
            };

            order.CustomerOrderProducts.Add(newProductLink);

            await _customerOrderService.UpdateOrderAsync(order);
            return NoContent();
        }

        [HttpDelete("{orderId}/products/{productId}")]
        public async Task<IActionResult> RemoveProductFromOrder(int orderId, int productId)
        {
            var orderValidation = ValidationHelper.ValidateOrderId(orderId);
            var productValidation = ValidationHelper.ValidateProductId(productId);

            if (!string.IsNullOrEmpty(orderValidation)) return BadRequest(new ApiResponseDto<string>(400, false, orderValidation));
            if (!string.IsNullOrEmpty(productValidation)) return BadRequest(new ApiResponseDto<string>(400, false, productValidation));

            var order = await _customerOrderService.GetOrderByIdAsync(orderId);
            if (order == null) return NotFound(new ApiResponseDto<string>(404, false, "Order not found"));

            var productLink = order.CustomerOrderProducts
                .FirstOrDefault(cop => cop.ProductId == productId);

            if (productLink == null) return NotFound(new ApiResponseDto<string>(404, false, "Product not found in this order"));

            order.CustomerOrderProducts.Remove(productLink);

            await _customerOrderService.UpdateOrderAsync(order);
            return NoContent();
        }

        [HttpPut("{orderId}/address")]
        public async Task<IActionResult> UpdateOrderAddress(int orderId, [FromBody] UpdateOrderAddressRequest request)
        {
            var orderValidation = ValidationHelper.ValidateOrderId(orderId);
            if (!string.IsNullOrEmpty(orderValidation)) return BadRequest(new ApiResponseDto<string>(400, false, orderValidation));

            var order = await _customerOrderService.GetOrderByIdAsync(orderId);
            if (order == null) return NotFound(new ApiResponseDto<string>(404, false, "Order not found"));

            order.Address = request.Address;

            await _customerOrderService.UpdateOrderAsync(order);
            return NoContent();
        }

        [HttpPatch("{orderId}/products/{productId}/quantity")]
        public async Task<IActionResult> UpdateProductQuantityInOrder(int orderId, int productId, [FromBody] UpdateOrderProductQuantityRequest request)
        {
            var orderValidation = ValidationHelper.ValidateOrderId(orderId);
            var productValidation = ValidationHelper.ValidateProductId(productId);
            var quantityValidation = ValidationHelper.ValidateQuantity(request.Quantity);

            if (!string.IsNullOrEmpty(orderValidation)) return BadRequest(new ApiResponseDto<string>(400, false, orderValidation));
            if (!string.IsNullOrEmpty(productValidation)) return BadRequest(new ApiResponseDto<string>(400, false, productValidation));
            if (!string.IsNullOrEmpty(quantityValidation)) return BadRequest(new ApiResponseDto<string>(400, false, quantityValidation));

            var order = await _customerOrderService.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                return NotFound(new ApiResponseDto<string>(404, false, "Order not found"));
            }

            var productLink = order.CustomerOrderProducts.FirstOrDefault(cop => cop.ProductId == productId);
            if (productLink == null)
            {
                return NotFound(new ApiResponseDto<string>(404, false, "Product not found in this order"));
            }

            var product = await _customerOrderService.GetProductByIdAsync(productId);
            if (product == null)
            {
                return NotFound(new ApiResponseDto<string>(404, false, "Product not found"));
            }

            if (request.Quantity > product.Quantity)
            {
                return BadRequest(new ApiResponseDto<string>(400, false, $"Insufficient stock. Available: {product.Quantity}"));
            }

            productLink.ProductQuantity = request.Quantity;

            await _customerOrderService.UpdateOrderAsync(order);
            return NoContent();
        }
    }
}
