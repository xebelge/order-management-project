using CustomerOrders.Application.Features.Products.Commands;
using CustomerOrders.Application.Services;
using MediatR;

namespace CustomerOrders.Application.Features.Products.Handlers
{
    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
    {
        private readonly ProductService _productService;

        public DeleteProductCommandHandler(ProductService productService)
        {
            _productService = productService;
        }

        public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _productService.GetProductByIdAsync(request.Id);
            if (product == null)
                return false;

            await _productService.DeleteProductAsync(request.Id);
            return true;
        }
    }

}
