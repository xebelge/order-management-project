using AutoMapper;
using CustomerOrders.Core.Entities;
using CustomerOrders.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CustomerOrders.Application.Queries.CustomerQueries
{
    public class GetAllCustomersQuery : IRequest<IEnumerable<CustomerDto>> { }

    public class GetAllCustomersQueryHandler : IRequestHandler<GetAllCustomersQuery, IEnumerable<CustomerDto>>
    {
        private readonly IRepository<Customer> _repository;
        private readonly IMapper _mapper;

        public GetAllCustomersQueryHandler(IRepository<Customer> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CustomerDto>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
        {
            var customers = await _repository.Query()
                                             .Where(c => !c.IsDeleted)
                                             .ToListAsync(cancellationToken);

            return _mapper.Map<IEnumerable<CustomerDto>>(customers);
        }
    }
}