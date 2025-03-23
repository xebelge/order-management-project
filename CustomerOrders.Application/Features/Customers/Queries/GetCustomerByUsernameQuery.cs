using AutoMapper;
using CustomerOrders.Core.Entities;
using CustomerOrders.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CustomerOrders.Application.Queries.CustomerQueries
{
    public class GetCustomerByUsernameQuery : IRequest<CustomerDto?>
    {
        public string Username { get; set; }
    }

    public class GetCustomerByUsernameQueryHandler : IRequestHandler<GetCustomerByUsernameQuery, CustomerDto?>
    {
        private readonly IRepository<Customer> _repository;
        private readonly IMapper _mapper;

        public GetCustomerByUsernameQueryHandler(IRepository<Customer> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<CustomerDto?> Handle(GetCustomerByUsernameQuery request, CancellationToken cancellationToken)
        {
            var customer = await _repository.Query()
                .FirstOrDefaultAsync(c => c.Username == request.Username, cancellationToken);

            if (customer == null || customer.IsDeleted)
                return null;

            return _mapper.Map<CustomerDto>(customer);
        }
    }
}