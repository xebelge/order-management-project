using CustomerOrders.Application.Features.CustomerOrders.Commands;
using CustomerOrders.Core.Entities;
using CustomerOrders.Core.Interfaces;
using MediatR;

namespace CustomerOrders.Application.Features.CustomerOrders.Handlers
{
    public class UpdateOrderAddressCommandHandler : IRequestHandler<UpdateOrderAddressCommand, bool>
    {
        private readonly IRepository<CustomerOrder> _repository;

        public UpdateOrderAddressCommandHandler(IRepository<CustomerOrder> repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(UpdateOrderAddressCommand request, CancellationToken cancellationToken)
        {
            var order = await _repository.GetByIdAsync(request.OrderId);
            if (order == null) return false;

            order.Address = request.Address;
            await _repository.UpdateAsync(order);
            return true;
        }
    }

}
