using AutoMapper;
using CustomerOrders.Application.DTOs.RequestDtos;
using CustomerOrders.Core.Entities;
using CustomerOrders.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CustomerOrders.Application.Features.CustomerOrders.Handlers
{
    public class AddCustomerOrdersCommandHandler : IRequestHandler<AddCustomerOrdersCommand, AddCustomerOrdersResult>
    {
        private readonly IMapper _mapper;
        private readonly IRepository<CustomerOrder> _orderRepository;
        private readonly IRepository<Product> _productRepository;

        public AddCustomerOrdersCommandHandler(IMapper mapper, IRepository<CustomerOrder> orderRepository, IRepository<Product> productRepository)
        {
            _mapper = mapper;
            _orderRepository = orderRepository;
            _productRepository = productRepository;
        }

        public async Task<AddCustomerOrdersResult> Handle(AddCustomerOrdersCommand request, CancellationToken cancellationToken)
        {
            var validOrders = new List<CustomerOrder>();

            foreach (var orderRequest in request.Orders)
            {
                var productIds = orderRequest.ProductItems.Select(x => x.ProductId).ToList();

                var existingProductIds = await _productRepository.Query()
                    .Where(p => productIds.Contains(p.Id))
                    .Select(p => p.Id)
                    .ToListAsync(cancellationToken);

                var invalidProductIds = productIds.Except(existingProductIds).ToList();

                if (invalidProductIds.Any())
                {
                    var invalidList = string.Join(", ", invalidProductIds);
                    return AddCustomerOrdersResult.Fail($"The following product IDs are invalid: {invalidList}");
                }

                var newOrder = new CustomerOrder
                {
                    CustomerId = orderRequest.CustomerId,
                    Address = orderRequest.Address,
                    CreatedAt = DateTime.UtcNow,
                    CustomerOrderProducts = orderRequest.ProductItems.Select(i => new CustomerOrderProduct
                    {
                        ProductId = i.ProductId,
                        ProductQuantity = i.Quantity
                    }).ToList()
                };

                await _orderRepository.AddAsync(newOrder);
                validOrders.Add(newOrder);
            }

            var orderDtos = _mapper.Map<List<CustomerOrderDto>>(validOrders);
            return AddCustomerOrdersResult.SuccessResult(orderDtos);
        }
    }
}
