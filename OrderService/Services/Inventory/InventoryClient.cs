using System.Net.Http;
using System.Net.Http.Headers;

namespace OrderService.Services.Inventory
{
    public class InventoryClient : IInventoryClient
    {
        private readonly HttpClient _http;
        private readonly string _inventoryBaseUrl = "";
        public InventoryClient(HttpClient http, IConfiguration config)
        {
            _http = http;
            _inventoryBaseUrl = config["InventoryService:BaseUrl"];
        }

        public async Task<bool> CheckAndReserveInventoryAsync(Guid ProductId, int quantity, string token)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_inventoryBaseUrl}/api/Inventory/reserve")
            {
                Content = JsonContent.Create(new
                {
                    ProductId = ProductId,
                    Quantity = quantity
                })
            };

            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Replace("Bearer ", ""));

            var response = await _http.SendAsync(httpRequest);
            return response.IsSuccessStatusCode;
        }
        public async Task<bool> ReleaseInventoryAsync(Guid productId, int quantity, string token)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_inventoryBaseUrl}/api/Inventory/release")
            {
                Content = JsonContent.Create(new
                {
                    ProductId = productId,
                    Quantity = quantity
                })
            };

            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Replace("Bearer ", ""));

            var response = await _http.SendAsync(httpRequest);
            return response.IsSuccessStatusCode;
        }
    }
}
