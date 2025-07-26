namespace OrderService.Services.Inventory
{
    public class InventoryClient : IInventoryClient
    {
        private readonly HttpClient _http;

        public InventoryClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<bool> CheckAndReserveInventoryAsync(string productName, int quantity)
        {
            var response = await _http.PostAsJsonAsync("/api/inventory/reserve", new
            {
                ProductName = productName,
                Quantity = quantity
            });

            return response.IsSuccessStatusCode;
        }
    }
}
