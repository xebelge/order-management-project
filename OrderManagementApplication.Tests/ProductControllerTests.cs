using CustomerOrderApi.Controllers;
using CustomerOrders.Application.DTOs;
using CustomerOrders.Application.Features.Products.Command;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace CustomerOrderApi.Tests.Controllers
{
    public class ProductControllerTests
    {
        private readonly Mock<IValidator<CreateProductRequest>> _validatorMock;
        private readonly Mock<IValidator<UpdateProductRequest>> _updateValidatorMock;
        private readonly Mock<IValidator<int>> _idValidatorMock;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ILogger<ProductController>> _loggerMock;

        private readonly ProductController _controller;

        public ProductControllerTests()
        {
            _validatorMock = new Mock<IValidator<CreateProductRequest>>();
            _updateValidatorMock = new Mock<IValidator<UpdateProductRequest>>();
            _idValidatorMock = new Mock<IValidator<int>>();
            _mediatorMock = new Mock<IMediator>();
            _loggerMock = new Mock<ILogger<ProductController>>();

            _controller = new ProductController(
                productService: null,
                _validatorMock.Object,
                _updateValidatorMock.Object,
                _idValidatorMock.Object,
                _mediatorMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task AddProducts_ShouldReturnOk_WhenValidRequests()
        {
            // Arrange
            var requestList = new List<CreateProductRequest>
            {
                new() { Barcode = "123", Description = "Test", Price = 10, Quantity = 5 },
                new() { Barcode = "456", Description = "Test 2", Price = 20, Quantity = 10 }
            };

            _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateProductRequest>(), default))
                .ReturnsAsync(new ValidationResult());

            _mediatorMock.Setup(m => m.Send(It.IsAny<AddProductsCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.AddProducts(requestList);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var response = okResult.Value as ApiResponseDto<string>;
            response!.Success.Should().BeTrue();
            response.Message.Should().Be("Products successfully added.");
        }

        [Fact]
        public async Task AddProducts_ShouldReturnBadRequest_WhenValidationFails()
        {
            // Arrange
            var requestList = new List<CreateProductRequest>
            {
                new() { Barcode = "123", Description = "test", Price = -1, Quantity = 5 }
            };

            var validationFailure = new ValidationFailure("Price", "Price must be greater than 0");
            _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateProductRequest>(), default))
                .ReturnsAsync(new ValidationResult(new List<ValidationFailure> { validationFailure }));

            // Act
            var result = await _controller.AddProducts(requestList);

            // Assert
            var badRequest = result as BadRequestObjectResult;
            badRequest.Should().NotBeNull();
            badRequest!.StatusCode.Should().Be(400);

            var response = badRequest.Value as ApiResponseDto<string>;
            response!.Success.Should().BeFalse();
            response.Message.Should().Be("Price must be greater than 0");
        }
    }
}
