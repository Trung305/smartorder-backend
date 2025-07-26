using ProductService.Dtos;
using ProductService.Models;
using AutoMapper;

namespace ProductService.Mapping
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            // Thêm dòng này
            CreateMap<CreateProductDto, Product>();

            // Nếu có DTO khác
            CreateMap<Product, ProductDto>();
        }
    }
}
