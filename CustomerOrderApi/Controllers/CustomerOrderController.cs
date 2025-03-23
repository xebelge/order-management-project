using CustomerOrderApi.Helpers; 
using CustomerOrders.Application.CustomerOrders.Queries.CustomerOrderQueries;
using CustomerOrders.Application.DTOs;
using CustomerOrders.Application.DTOs.RequestDtos;
using CustomerOrders.Application.Features.CustomerOrders.Commands;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerOrderApi.Controllers
{
    /// <summary>
    /// Handles operations related to customer orders.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/customerorders")]
    public class CustomerOrderController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IValidator<CreateOrderRequest> _createOrderValidator;
        private readonly IValidator<AddProductToOrderRequest> _addProductValidator;
        private readonly IValidator<UpdateOrderAddressRequest> _updateAddressValidator;
        private readonly IValidator<UpdateOrderProductQuantityRequest> _updateQuantityValidator;
        private readonly IValidator<int> _idValidator;
        private readonly ILogger<CustomerOrderController> _logger;

        public CustomerOrderController(
            IMediator mediator,
            IValidator<CreateOrderRequest> createOrderValidator,
            IValidator<AddProductToOrderRequest> addProductValidator,
            IValidator<UpdateOrderAddressRequest> updateAddressValidator,
            IValidator<UpdateOrderProductQuantityRequest> updateQuantityValidator,
            IValidator<int> idValidator,
            ILogger<CustomerOrderController> logger)
        {
            _mediator = mediator;
            _createOrderValidator = createOrderValidator;
            _addProductValidator = addProductValidator;
            _updateAddressValidator = updateAddressValidator;
            _updateQuantityValidator = updateQuantityValidator;
            _idValidator = idValidator;
            _logger = logger;
        }

        /// <summary>
        /// Returns a list of all customer orders.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            _logger.LogInformation("Retrieving all customer orders.");
            var result = await _mediator.Send(new GetAllCustomerOrdersQuery());
            return Ok(new ApiResponseDto<IEnumerable<CustomerOrderDto>>(200, true, "Orders retrieved successfully", result));
        }

        /// <summary>
        /// Returns a specific customer order by ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var validationResult = await ValidationHelper.ValidateId(id, _idValidator);
            if (validationResult != null)
                return validationResult;

            var result = await _mediator.Send(new GetCustomerOrderByIdQuery(id));
            if (result == null)
                return NotFound(new ApiResponseDto<string>(404, false, "Order not found"));

            return Ok(new ApiResponseDto<CustomerOrderDto>(200, true, "Order retrieved successfully", result));
        }

        /// <summary>
        /// Creates one or more new customer orders.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddOrders([FromBody] List<CreateOrderRequest> requests)
        {
            foreach (var request in requests)
            {
                var validation = await ValidationHelper.ValidateRequest(_createOrderValidator, request);
                if (validation != null)
                    return validation;
            }

            var result = await _mediator.Send(new AddCustomerOrdersCommand { Orders = requests });

            if (result == null)
            {
                _logger.LogError("AddCustomerOrdersCommand returned null.");
                return StatusCode(500, new ApiResponseDto<string>(500, false, "Unexpected error occurred while creating orders."));
            }

            if (!result.Success)
                return BadRequest(new ApiResponseDto<string>(400, false, result.ErrorMessage ?? "Order creation failed."));

            return CreatedAtAction(nameof(GetOrders), null,
                new ApiResponseDto<IEnumerable<CustomerOrderDto>>(201, true, "Orders added successfully", result.Orders));
        }

        /// <summary>
        /// Deletes a customer order by ID.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var validationResult = await ValidationHelper.ValidateId(id, _idValidator);
            if (validationResult != null)
                return validationResult;

            var success = await _mediator.Send(new DeleteCustomerOrderCommand { OrderId = id });
            if (!success)
                return NotFound(new ApiResponseDto<string>(404, false, "Order not found."));

            _logger.LogInformation("Order deleted. ID: {Id}", id);
            return NoContent();
        }

        /// <summary>
        /// Adds a product to a specific order.
        /// </summary>
        [HttpPost("{orderId}/products")]
        public async Task<IActionResult> AddProductToOrder(int orderId, [FromBody] AddProductToOrderRequest request)
        {
            var idValidation = await ValidationHelper.ValidateId(orderId, _idValidator);
            if (idValidation != null)
                return idValidation;

            var validation = await ValidationHelper.ValidateRequest(_addProductValidator, request);
            if (validation != null)
                return validation;

            var success = await _mediator.Send(new AddProductToOrderCommand
            {
                OrderId = orderId,
                ProductId = request.ProductId,
                Quantity = request.Quantity
            });

            if (!success)
                return Conflict(new ApiResponseDto<string>(409, false, "Product already exists in this order or order not found."));

            return NoContent();
        }

        /// <summary>
        /// Removes a product from a specific order.
        /// </summary>
        [HttpDelete("{orderId}/products/{productId}")]
        public async Task<IActionResult> RemoveProductFromOrder(int orderId, int productId)
        {
            var idValidation = await ValidationHelper.ValidateId(orderId, _idValidator);
            if (idValidation != null)
                return idValidation;

            var productValidation = await ValidationHelper.ValidateId(productId, _idValidator);
            if (productValidation != null)
                return productValidation;

            var success = await _mediator.Send(new RemoveProductFromOrderCommand
            {
                OrderId = orderId,
                ProductId = productId
            });

            if (!success)
                return NotFound(new ApiResponseDto<string>(404, false, "Order or product not found."));

            return NoContent();
        }

        /// <summary>
        /// Updates the delivery address of a customer order.
        /// </summary>
        [HttpPut("{orderId}/address")]
        public async Task<IActionResult> UpdateOrderAddress(int orderId, [FromBody] UpdateOrderAddressRequest request)
        {
            var idValidation = await ValidationHelper.ValidateId(orderId, _idValidator);
            if (idValidation != null)
                return idValidation;

            var validation = await ValidationHelper.ValidateRequest(_updateAddressValidator, request);
            if (validation != null)
                return validation;

            var success = await _mediator.Send(new UpdateOrderAddressCommand
            {
                OrderId = orderId,
                Address = request.Address
            });

            if (!success)
                return NotFound(new ApiResponseDto<string>(404, false, "Order not found"));

            return NoContent();
        }

        /// <summary>
        /// Updates the quantity of a product in a specific order.
        /// </summary>
        [HttpPatch("{orderId}/products/{productId}/quantity")]
        public async Task<IActionResult> UpdateProductQuantityInOrder(int orderId, int productId, [FromBody] UpdateOrderProductQuantityRequest request)
        {
            var idValidation = await ValidationHelper.ValidateId(orderId, _idValidator);
            if (idValidation != null)
                return idValidation;

            var productValidation = await ValidationHelper.ValidateId(productId, _idValidator);
            if (productValidation != null)
                return productValidation;

            var validation = await ValidationHelper.ValidateRequest(_updateQuantityValidator, request);
            if (validation != null)
                return validation;

            await _mediator.Send(new UpdateProductQuantityInOrderCommand
            {
                OrderId = orderId,
                ProductId = productId,
                Quantity = request.Quantity
            });

            return NoContent();
        }
    }
}
