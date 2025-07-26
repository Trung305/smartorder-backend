using AutoMapper;
using OrderService.Dtos;
using OrderService.Models;

namespace OrderService.Mapping
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<OrderCreateDto, Order>();
            CreateMap<OrderItemDto, OrderItem>();

            CreateMap<Order, OrderReadDto>()
                .ForMember(dest => dest.TotalAmount,
                    opt => opt.MapFrom(src => src.Items.Sum(i => i.Quantity * i.UnitPrice)));

            CreateMap<OrderItem, OrderItemDto>();
        }
    }
}
