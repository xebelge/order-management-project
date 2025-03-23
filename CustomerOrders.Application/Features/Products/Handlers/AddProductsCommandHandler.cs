using AutoMapper;
using CustomerOrders.Application.Features.Products.Command;
using CustomerOrders.Application.Services;
using CustomerOrders.Core.Entities;
using FluentValidation;
using MediatR;

namespace CustomerOrders.Application.Features.Products.Handlers
{
    public class AddProductsCommandHandler : IRequestHandler<AddProductsCommand, bool>
    {
        private readonly IMapper _mapper;
        private readonly ProductService _productService;
        private readonly IValidator<CreateProductRequest> _createValidator;

        public AddProductsCommandHandler(
            IMapper mapper,
            ProductService productService,
            IValidator<CreateProductRequest> createValidator)
        {
            _mapper = mapper;
            _productService = productService;
            _createValidator = createValidator;
        }

        public async Task<bool> Handle(AddProductsCommand request, CancellationToken cancellationToken)
        {
            foreach (var createRequest in request.Products)
            {
                var validationResult = await _createValidator.ValidateAsync(createRequest, cancellationToken);
                if (!validationResult.IsValid)
                    return false;

                var product = _mapper.Map<Product>(createRequest);
                product.CreatedAt = DateTime.UtcNow;

                await _productService.AddProductAsync(product);
            }

            return true;
        }
    }
}
