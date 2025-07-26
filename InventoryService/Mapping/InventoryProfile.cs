using AutoMapper;
using InventoryService.Dtos;
using InventoryService.Models;

namespace InventoryService.Mapping
{
    public class InventoryProfile : Profile

    {
        public InventoryProfile()
        {
            CreateMap<Inventory, InventoryReadDto>();
            CreateMap<InventoryCreateDto, Inventory>();
            CreateMap<InventoryUpdateDto, Inventory>();
        }
    }
}
