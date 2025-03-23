using AutoMapper;
using CustomerOrders.Application.DTOs;
using CustomerOrders.Core.Entities;

namespace CustomerOrders.Application.Helpers.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Product relating ProductDto
            CreateMap<Product, ProductDto>().ReverseMap();

            // CreateProductRequest relating Product
            CreateMap<CreateProductRequest, Product>();

            // UpdateProductRequest relating Product
            CreateMap<UpdateProductRequest, Product>();

            // Customer relating CustomerDto
            CreateMap<Customer, CustomerDto>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
                .ForSourceMember(src => src.IsDeleted, opt => opt.DoNotValidate());

            // CustomerOrderProduct relating CustomerOrderProductDto
            CreateMap<CustomerOrderProduct, CustomerOrderProductDto>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
                .ForMember(dest => dest.ProductQuantity, opt => opt.MapFrom(src => src.ProductQuantity))
                .ForMember(dest => dest.Barcode, opt => opt.MapFrom(src => src.Product.Barcode))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Product.Description))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Product.Price));

            // CustomerOrder relating CustomerOrderDto
            CreateMap<CustomerOrder, CustomerOrderDto>()
               .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.CustomerOrderProducts))
               .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src =>
                   src.CustomerOrderProducts
                       .Where(p => p.Product != null)
                       .Sum(p => p.Product.Price * p.ProductQuantity)));

        }
    }
}
