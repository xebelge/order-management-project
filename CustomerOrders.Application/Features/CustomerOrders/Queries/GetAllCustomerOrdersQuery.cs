using AutoMapper;
using CustomerOrders.Core.Entities;
using CustomerOrders.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CustomerOrders.Application.CustomerOrders.Queries.CustomerOrderQueries
{
    public class GetAllCustomerOrdersQuery : IRequest<IEnumerable<CustomerOrderDto>> { }

    public class GetAllCustomerOrdersQueryHandler : IRequestHandler<GetAllCustomerOrdersQuery, IEnumerable<CustomerOrderDto>>
    {
        private readonly IRepository<CustomerOrder> _repository;
        private readonly IMapper _mapper;

        public GetAllCustomerOrdersQueryHandler(IRepository<CustomerOrder> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CustomerOrderDto>> Handle(GetAllCustomerOrdersQuery request, CancellationToken cancellationToken)
        {
            var orders = await _repository
                .Query()
                .Include(o => o.CustomerOrderProducts)
                    .ThenInclude(p => p.Product)
                .ToListAsync(cancellationToken);

            return _mapper.Map<IEnumerable<CustomerOrderDto>>(orders);
        }
    }
}
