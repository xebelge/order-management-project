using CustomerOrders.Application.DTOs;
using CustomerOrders.Application.Services;
using CustomerOrders.Core.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerOrderApi.Controllers
{
    /// <summary>
    /// Manages product-related operations like retrieval, creation, update, and deletion.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/products")]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _productService;
        private readonly IValidator<Product> _productValidator;
        private readonly IValidator<int> _productIdValidator;

        public ProductController(
            ProductService productService,
            IValidator<Product> productValidator,
            IValidator<int> productIdValidator)
        {
            _productService = productService;
            _productValidator = productValidator;
            _productIdValidator = productIdValidator;
        }

        /// <summary>
        /// Gets a list of all products in the system.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(new ApiResponseDto<IEnumerable<ProductDto>>(200, true, "Products successfully retrieved.", products));
        }

        /// <summary>
        /// Fetches a product by its unique ID.
        /// </summary>
        /// <param name="id">Product ID to look up.</param>
        /// <returns>Product details or an error if not found.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var validationResult = await _productIdValidator.ValidateAsync(id);
            if (!validationResult.IsValid)
            {
                return BadRequest(new ApiResponseDto<string>(400, false, validationResult.Errors[0].ErrorMessage));
            }

            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound(new ApiResponseDto<string>(404, false, "Product not found"));
            }

            return Ok(new ApiResponseDto<ProductDto>(200, true, "Product successfully retrieved.", product));
        }

        /// <summary>
        /// Adds one or more new products to inventory.
        /// </summary>
        /// <param name="requestList">Product creation details.</param>
        /// <returns>200 OK if added, or a validation error.</returns>
        [HttpPost]
        public async Task<IActionResult> AddProducts([FromBody] List<CreateProductRequest> requestList)
        {
            foreach (var request in requestList)
            {
                var product = new Product
                {
                    Barcode = request.Barcode,
                    Description = request.Description,
                    Price = request.Price,
                    Quantity = request.Quantity,
                    CreatedAt = DateTime.UtcNow
                };

                var validationResult = await _productValidator.ValidateAsync(product);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new ApiResponseDto<string>(400, false, validationResult.Errors[0].ErrorMessage));
                }

                await _productService.AddProductAsync(product);
            }

            return Ok(new ApiResponseDto<string>(200, true, "Products successfully added."));
        }

        /// <summary>
        /// Updates existing products based on their IDs.
        /// </summary>
        /// <param name="requestList">List of product update data.</param>
        /// <returns>200 OK if updated, or a validation error.</returns>
        [HttpPut]
        public async Task<IActionResult> UpdateProducts([FromBody] List<UpdateProductRequest> requestList)
        {
            foreach (var request in requestList)
            {
                var validationIdResult = await _productIdValidator.ValidateAsync(request.Id);
                if (!validationIdResult.IsValid)
                    return BadRequest(new ApiResponseDto<string>(400, false, validationIdResult.Errors[0].ErrorMessage));

                var product = new Product
                {
                    Id = request.Id,
                    Barcode = request.Barcode,
                    Description = request.Description,
                    Price = request.Price,
                    Quantity = request.Quantity,
                    UpdatedAt = DateTime.UtcNow
                };

                var validationResult = await _productValidator.ValidateAsync(product);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new ApiResponseDto<string>(400, false, validationResult.Errors[0].ErrorMessage));
                }

                await _productService.UpdateProductAsync(product);
            }

            return Ok(new ApiResponseDto<string>(200, true, "Products successfully updated."));
        }

        /// <summary>
        /// Deletes a product by its unique ID.
        /// </summary>
        /// <param name="id">The product ID to remove.</param>
        /// <returns>NoContent or a validation error.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var validationResult = await _productIdValidator.ValidateAsync(id);
            if (!validationResult.IsValid)
            {
                return BadRequest(new ApiResponseDto<string>(400, false, validationResult.Errors[0].ErrorMessage));
            }

            await _productService.DeleteProductAsync(id);
            return NoContent();
        }
    }
}
