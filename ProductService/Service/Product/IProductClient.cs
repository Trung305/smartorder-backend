using ProductService.Dtos;

namespace ProductService.Service.Product
{
    public interface IProductClient
    {
        Task<ProductDto?> GetProductByIdAsync(Guid productId);
    }
}
