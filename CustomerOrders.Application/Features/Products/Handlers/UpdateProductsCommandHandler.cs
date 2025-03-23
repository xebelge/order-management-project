using AutoMapper;
using CustomerOrders.Application.DTOs;
using CustomerOrders.Application.Services;
using CustomerOrders.Core.Entities;
using FluentValidation;
using MediatR;

namespace CustomerOrders.Application.Features.Products.Handlers
{
    public class UpdateProductsCommandHandler : IRequestHandler<UpdateProductsCommand, bool>
    {
        private readonly ProductService _productService;
        private readonly IValidator<UpdateProductRequest> _updateValidator;
        private readonly IMapper _mapper;

        public UpdateProductsCommandHandler(
            ProductService productService,
            IValidator<UpdateProductRequest> updateValidator,
            IMapper mapper)
        {
            _productService = productService;
            _updateValidator = updateValidator;
            _mapper = mapper;
        }

        public async Task<bool> Handle(UpdateProductsCommand request, CancellationToken cancellationToken)
        {
            foreach (var update in request.Products)
            {
                var existing = await _productService.GetProductByIdAsync(update.Id);
                if (existing == null)
                    return false;

                var validationResult = await _updateValidator.ValidateAsync(update, cancellationToken);
                if (!validationResult.IsValid)
                    return false;

                var updated = new Product
                {
                    Id = update.Id,
                    Barcode = string.IsNullOrWhiteSpace(update.Barcode) ? existing.Barcode : update.Barcode,
                    Description = string.IsNullOrWhiteSpace(update.Description) ? existing.Description : update.Description,
                    Price = update.Price ?? existing.Price,
                    Quantity = update.Quantity ?? existing.Quantity,
                    UpdatedAt = DateTime.UtcNow
                };

                if (updated.Barcode == existing.Barcode &&
                    updated.Description == existing.Description &&
                    updated.Price == existing.Price &&
                    updated.Quantity == existing.Quantity)
                {
                    continue;
                }

                await _productService.UpdateProductAsync(updated);
            }

            return true;
        }
    }
}
