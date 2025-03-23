using CustomerOrderApi.Helpers;
using CustomerOrders.Application.DTOs;
using CustomerOrders.Application.Features.Products.Command;
using CustomerOrders.Application.Features.Products.Commands;
using CustomerOrders.Application.Features.Products.Queries;
using CustomerOrders.Application.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerOrderApi.Controllers
{
    /// <summary>
    /// Retrieves a list of all products.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/products")]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _productService;
        private readonly IValidator<CreateProductRequest> _createProductValidator;
        private readonly IValidator<UpdateProductRequest> _updateProductValidator;
        private readonly IValidator<int> _productIdValidator;
        private readonly IMediator _mediator;
        private readonly ILogger<ProductController> _logger;

        public ProductController(
            ProductService productService,
            IValidator<CreateProductRequest> createProductValidator,
            IValidator<UpdateProductRequest> updateProductValidator,
            IValidator<int> productIdValidator,
            IMediator mediator,
            ILogger<ProductController> logger)
        {
            _productService = productService;
            _createProductValidator = createProductValidator;
            _updateProductValidator = updateProductValidator;
            _productIdValidator = productIdValidator;
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a list of all products.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _mediator.Send(new GetAllProductsQuery());
            return Ok(new ApiResponseDto<IEnumerable<ProductDto>>(200, true, "Products successfully retrieved.", products));
        }

        /// <summary>
        /// Retrieves a single product by ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var validation = await ValidationHelper.ValidateId(id, _productIdValidator);
            if (validation != null)
                return validation;

            var product = await _mediator.Send(new GetProductByIdQuery { Id = id });
            if (product == null)
                return NotFound(new ApiResponseDto<string>(404, false, "Product not found"));

            return Ok(new ApiResponseDto<ProductDto>(200, true, "Product successfully retrieved.", product));
        }

        /// <summary>
        /// Adds one or more new products to the system.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddProducts([FromBody] List<CreateProductRequest> requestList)
        {
            foreach (var request in requestList)
            {
                var validation = await ValidationHelper.ValidateRequest(_createProductValidator, request);
                if (validation != null)
                    return validation;
            }

            var result = await _mediator.Send(new AddProductsCommand { Products = requestList });
            if (!result)
                return BadRequest(new ApiResponseDto<string>(400, false, "Validation failed for one or more products."));

            _logger.LogInformation("Products added successfully.");
            return Ok(new ApiResponseDto<string>(200, true, "Products successfully added."));
        }

        /// <summary>
        /// Updates one or more existing products.
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> UpdateProducts([FromBody] List<UpdateProductRequest> requestList)
        {
            foreach (var request in requestList)
            {
                var validation = await ValidationHelper.ValidateRequest(_updateProductValidator, request);
                if (validation != null)
                    return validation;
            }

            var result = await _mediator.Send(new UpdateProductsCommand { Products = requestList });
            if (!result)
                return BadRequest(new ApiResponseDto<string>(400, false, "Product update failed. Please check inputs."));

            _logger.LogInformation("Products updated successfully.");
            return Ok(new ApiResponseDto<string>(200, true, "Products successfully updated."));
        }

        /// <summary>
        /// Deletes a product by ID.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var validation = await ValidationHelper.ValidateId(id, _productIdValidator);
            if (validation != null)
                return validation;

            var result = await _mediator.Send(new DeleteProductCommand { Id = id });
            if (!result)
                return NotFound(new ApiResponseDto<string>(404, false, $"Product with ID {id} not found."));

            _logger.LogInformation("Product with ID {ProductId} deleted.", id);
            return NoContent();
        }
    }
}
