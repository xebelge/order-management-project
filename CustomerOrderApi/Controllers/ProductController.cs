using CustomerOrders.Application.DTOs;
using CustomerOrders.Application.Helpers;
using CustomerOrders.Application.Services;
using CustomerOrders.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerOrderApi.Controllers
{
    /// <summary>
    /// Provides operations for managing products.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/products")]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _productService;

        /// <summary>
        /// Initializes a new instance of the ProductController class.
        /// </summary>
        /// <param name="productService">The service that provides product-related operations.</param>
        public ProductController(ProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Retrieves all products.
        /// </summary>
        /// <returns>A list of all products.</returns>
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }


        /// <summary>
        /// Retrieves a product by its unique ID.
        /// </summary>
        /// <param name="id">The unique ID of the product.</param>
        /// <returns>The product with the specified ID, or a NotFound result if it doesn't exist.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var validationError = ValidationHelper.ValidateProductId(id);
            if (!string.IsNullOrEmpty(validationError))
            {
                return BadRequest(validationError);
            }

            var product = await _productService.GetProductByIdAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        /// <summary>
        /// Adds a new product to the inventory.
        /// </summary>
        /// <param name="products">The products to add.</param>
        /// <returns>The created product.</returns>
        [HttpPost]
        public async Task<IActionResult> AddProducts([FromBody] List<Product> products)
        {
            foreach (var product in products)
            {
                var validationError = ValidationHelper.ValidateProduct(product);
                if (!string.IsNullOrEmpty(validationError))
                {
                    return BadRequest(new ApiResponseDto<string>(400, false, validationError));
                }

                product.CreatedAt = DateTime.UtcNow;
                await _productService.AddProductAsync(product);
            }

            return Ok(new ApiResponseDto<string>(200, true, "Products successfully added."));
        }

        /// <summary>
        /// Updates an existing product.
        /// </summary>
        /// <param name="products">The updated product data.</param>
        /// <returns>No content if successful; BadRequest if the validation fails.</returns>
        [HttpPut]
        public async Task<IActionResult> UpdateProducts([FromBody] List<Product> products)
        {
            foreach (var product in products)
            {
                var validationError = ValidationHelper.ValidateProduct(product);
                if (!string.IsNullOrEmpty(validationError))
                {
                    return BadRequest(new ApiResponseDto<string>(400, false, validationError));
                }

                product.UpdatedAt = DateTime.UtcNow;
                await _productService.UpdateProductAsync(product);
            }

            return Ok(new ApiResponseDto<string>(200, true, "Products successfully updated."));
        }

        /// <summary>
        /// Deletes a product by its unique ID.
        /// </summary>
        /// <param name="id">The ID of the product to delete.</param>
        /// <returns>No content if successful.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var validationError = ValidationHelper.ValidateProductId(id);
            if (!string.IsNullOrEmpty(validationError))
            {
                return BadRequest(new ApiResponseDto<string>(400, false, validationError));
            }

            await _productService.DeleteProductAsync(id);
            return NoContent();
        }
    }
}
