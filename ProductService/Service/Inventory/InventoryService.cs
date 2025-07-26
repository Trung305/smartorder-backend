using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Text.Json;

namespace ProductService.Service.Inventory
{
    public class InventoryService : IInventoryService
    {
        private readonly HttpClient _httpClient;
        private readonly string _inventoryBaseUrl = "";
        private readonly ILogger<InventoryService> _logger;
        public InventoryService(HttpClient httpClient, IConfiguration config, ILogger<InventoryService> logger)
        {
            _httpClient = httpClient;
            _inventoryBaseUrl = config["InventoryService:BaseUrl"];
            _logger = logger;
        }
        public async Task<int> GetQuantityAsync(Guid productId)
        {
            var response = await _httpClient.GetAsync($"{_inventoryBaseUrl}/api/inventory/product/{productId}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Không thể lấy tồn kho. Status: {response.StatusCode}");
                return -1;
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<int>(json);
        }

        public async Task<Dictionary<Guid, int>> GetAllQuantitiesAsync()
        {
            var response = await _httpClient.GetAsync($"{_inventoryBaseUrl}/api/inventory/all");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Không thể lấy tồn kho. Status: {response.StatusCode}");
            }
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Dictionary<Guid, int>>(json);
        }
    }
}
public class InventoryDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
