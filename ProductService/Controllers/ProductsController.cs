using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using ProductService.Data;
using ProductService.Dtos;
using ProductService.Models;
using ProductService.Service.Inventory;
using System.Net.Http;
using System.Text.Json;

namespace ProductService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : Controller
    {
        private readonly ProductDbContext _context;
        private readonly IMapper _mapper;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDistributedCache _cache;
        private readonly string _inventoryBaseUrl = "";
        private readonly IInventoryService _iventoryService;
        private readonly ILogger<ProductsController> _logger;
        public ProductsController(ProductDbContext context, IMapper mapper, IDistributedCache cache, IHttpClientFactory httpClientFactory, IConfiguration config, IInventoryService iventoryService, ILogger<ProductsController> logger)
        {
            _context = context;
            _mapper = mapper;
            _cache = cache;
            _httpClientFactory = httpClientFactory;
            _inventoryBaseUrl = config["InventoryService:BaseUrl"];
            _iventoryService = iventoryService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll()
        {
            try
            {
                var products = await _context.Products.ToListAsync();
                var quantities = await _iventoryService.GetAllQuantitiesAsync();

                var result = products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Quantity = quantities.ContainsKey(p.Id) ? quantities[p.Id] : 0
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("ProductsController - GetAll: {0}", ex.ToString());
                return StatusCode(500, "Internal Server Error");
            }

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> Get(Guid id)
        {
            try
            {
                string cacheKey = $"product:{id}";

                var cachedProduct = await _cache.GetStringAsync(cacheKey);
                if (cachedProduct != null)
                {
                    var cachedDto = JsonSerializer.Deserialize<ProductDto>(cachedProduct);
                    return Ok(cachedDto);
                }
                var product = await _context.Products.FindAsync(id);
                if (product == null) return NotFound();


                var dto = _mapper.Map<ProductDto>(product);
                dto.Quantity = await _iventoryService.GetQuantityAsync(id);
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                };
                var serialized = JsonSerializer.Serialize(dto);
                await _cache.SetStringAsync(cacheKey, serialized, cacheOptions);

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError("ProductsController - Get: {0}", ex.ToString());
                return StatusCode(500, "Internal Server Error");
            }
            
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<ProductDto>> Create(CreateProductDto dto)
        {
            try
            {
                var product = _mapper.Map<Product>(dto);
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                // Gửi yêu cầu tạo tồn kho sang InventoryService
                var inventoryPayload = new
                {
                    ProductId = product.Id,
                    Quantity = dto.Quantity
                };

                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.PostAsJsonAsync($"{_inventoryBaseUrl}/api/Inventory", inventoryPayload);

                if (!response.IsSuccessStatusCode)
                {
                    // Optional: rollback Product nếu tạo tồn kho thất bại
                    return StatusCode(500, "Failed to create inventory for the product.");
                }

                var result = _mapper.Map<ProductDto>(product);
                return CreatedAtAction(nameof(Get), new { id = product.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError("ProductsController - Create: {0}", ex.ToString());
                return StatusCode(500, "Internal Server Error");
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CreateProductDto dto)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null) return NotFound();

                _mapper.Map(dto, product);
                await _context.SaveChangesAsync();

                // ❌ Xoá cache cũ
                await _cache.RemoveAsync($"product:{id}");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError("ProductsController - Update: {0}", ex.ToString());
                return StatusCode(500, "Internal Server Error");
            }

        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null) return NotFound();

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError("ProductsController - Delete: {0}", ex.ToString());
                return StatusCode(500, "Internal Server Error");
            }

        }
    }
}
