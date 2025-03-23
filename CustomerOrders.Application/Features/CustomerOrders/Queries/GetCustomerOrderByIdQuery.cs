using AutoMapper;
using CustomerOrders.Core.Entities;
using CustomerOrders.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CustomerOrders.Application.CustomerOrders.Queries.CustomerOrderQueries
{
    public class GetCustomerOrderByIdQuery : IRequest<CustomerOrderDto?>
    {
        public int Id { get; set; }

        public GetCustomerOrderByIdQuery(int id)
        {
            Id = id;
        }
    }

    public class GetCustomerOrderByIdQueryHandler : IRequestHandler<GetCustomerOrderByIdQuery, CustomerOrderDto?>
    {
        private readonly IRepository<CustomerOrder> _repository;
        private readonly IMapper _mapper;

        public GetCustomerOrderByIdQueryHandler(IRepository<CustomerOrder> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<CustomerOrderDto?> Handle(GetCustomerOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var order = await _repository
                .Query()
                .Include(o => o.CustomerOrderProducts)
                    .ThenInclude(p => p.Product)
                .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

            return order == null ? null : _mapper.Map<CustomerOrderDto>(order);
        }
    }
}
