using AutoMapper;
using CustomerOrders.Core.Entities;
using CustomerOrders.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CustomerOrders.Application.Queries.CustomerQueries
{

    public class GetCustomerByEmailQuery : IRequest<CustomerDto?>
    {
        public string Email { get; set; }
    }

    public class GetCustomerByEmailQueryHandler : IRequestHandler<GetCustomerByEmailQuery, CustomerDto?>
    {
        private readonly IRepository<Customer> _repository;
        private readonly IMapper _mapper;

        public GetCustomerByEmailQueryHandler(IRepository<Customer> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<CustomerDto?> Handle(GetCustomerByEmailQuery request, CancellationToken cancellationToken)
        {
            var customer = await _repository.Query()
                .FirstOrDefaultAsync(c => c.Email.ToLower() == request.Email.ToLower(), cancellationToken);

            if (customer == null || customer.IsDeleted)
                return null;

            return _mapper.Map<CustomerDto>(customer);
        }
    }
}