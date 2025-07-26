using OrderService.Dtos;

namespace OrderService.Services.Product
{
    public interface IProductClient
    {
        Task<ProductDto?> GetProductByIdAsync(Guid productId);
    }
}
