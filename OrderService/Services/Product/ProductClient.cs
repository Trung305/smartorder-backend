using OrderService.Dtos;
using System.Text.Json;

namespace OrderService.Services.Product
{
    public class ProductClient : IProductClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProductClient> _logger;

        public ProductClient(HttpClient httpClient, ILogger<ProductClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<ProductDto?> GetProductByIdAsync(Guid productId)
        {
            var response = await _httpClient.GetAsync($"/api/products/{productId}");
            if (!response.IsSuccessStatusCode) return null;

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ProductDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }

}
