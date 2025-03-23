using CustomerOrders.Application.Features.CustomerOrders.Commands;
using CustomerOrders.Core.Entities;
using CustomerOrders.Core.Interfaces;
using MediatR;

namespace CustomerOrders.Application.Features.CustomerOrders.Handlers
{
    public class DeleteCustomerOrderCommandHandler : IRequestHandler<DeleteCustomerOrderCommand, bool>
    {
        private readonly IRepository<CustomerOrder> _repository;

        public DeleteCustomerOrderCommandHandler(IRepository<CustomerOrder> repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(DeleteCustomerOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _repository.GetByIdAsync(request.OrderId);
            if (order == null)
                return false;

            await _repository.DeleteAsync(order);
            return true;
        }
    }
}
