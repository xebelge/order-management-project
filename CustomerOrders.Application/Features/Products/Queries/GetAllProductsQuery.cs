using AutoMapper;
using CustomerOrders.Core.Entities;
using CustomerOrders.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CustomerOrders.Application.Features.Products.Queries
{
    public class GetAllProductsQuery : IRequest<IEnumerable<ProductDto>> { }

    public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, IEnumerable<ProductDto>>
    {
        private readonly IRepository<Product> _repository;
        private readonly IMapper _mapper;

        public GetAllProductsQueryHandler(IRepository<Product> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
        {
            var products = await _repository.Query().ToListAsync(cancellationToken);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }
    }
}
