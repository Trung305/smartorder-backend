using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Dtos;
using OrderService.Models;
using AutoMapper;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using OrderService.Services.Inventory;
using OrderService.Services.Product;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDistributedCache _cache;
        private readonly IInventoryClient _inventoryClient;
        private readonly IProductClient _productClient;
        private readonly ILogger<OrderController> _logger;
        public OrderController(ILogger<OrderController> logger, ApplicationDbContext context, IMapper mapper, IDistributedCache cache, IInventoryClient inventoryClient, IProductClient productClient)
        {
            _context = context;
            _mapper = mapper;
            _cache = cache;
            _inventoryClient = inventoryClient;
            _productClient = productClient;
            _logger = logger;
        }
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.Items)
                .ToListAsync();

            return Ok(orders);
        }
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderReadDto>> GetOrder(Guid id)
        {
            try
            {
                var cacheKey = $"order:{id}";
                var cached = await _cache.GetStringAsync(cacheKey);

                if (cached != null)
                {
                    var result = JsonSerializer.Deserialize<OrderReadDto>(cached);
                    return Ok(result);
                }

                var order = await _context.Orders
                    .Include(o => o.Items)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null) return NotFound();

                var orderDto = _mapper.Map<OrderReadDto>(order);

                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(orderDto),
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                    });

                return Ok(orderDto);
            }
            catch (Exception ex)
            {
                _logger.LogError("OrderController - GetOrder: {0}", ex.ToString());
                return StatusCode(500, "Internal Server Error");
            }
            
        }
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<OrderReadDto>> CreateOrder(OrderCreateDto dto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var token = HttpContext.Request.Headers["Authorization"].ToString();
                _logger.LogInformation("🔍 UserId từ JWT: {UserId}", userId);

                if (userId == null)
                {
                    _logger.LogWarning("⚠️ Không lấy được thông tin người dùng từ token.");
                    return Unauthorized("Không lấy được thông tin người dùng.");
                }

                var order = new Order
                {
                    CustomerName = dto.CustomerName,
                    UserId = userId,
                    OrderDate = dto.OrderDate,
                    Items = new List<OrderItem>()
                };

                foreach (var item in dto.Items)
                {
                    // Gọi ProductService để lấy thông tin sản phẩm
                    var product = await _productClient.GetProductByIdAsync(item.ProductId);
                    if (product == null)
                    {
                        return BadRequest($"Không tìm thấy sản phẩm với ID: {item.ProductId}");
                    }

                    // Gọi InventoryService để kiểm tra và giữ hàng
                    var success = await _inventoryClient.CheckAndReserveInventoryAsync(product.Id, item.Quantity, token);
                    if (!success)
                    {
                        return BadRequest($"Không đủ tồn kho cho sản phẩm {product.Name}");
                    }

                    // Thêm vào danh sách Items của đơn hàng
                    order.Items.Add(new OrderItem
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    });
                }

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                var result = _mapper.Map<OrderReadDto>(order);
                return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError("OrderController - CreateOrder: {0}", ex.ToString());
                return StatusCode(500, "Internal Server Error");
            }
            
        }
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(Guid id, Order updatedOrder)
        {
            try
            {
                if (id != updatedOrder.Id) return BadRequest();

                var existingOrder = await _context.Orders
                    .Include(o => o.Items)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (existingOrder == null) return NotFound();

                existingOrder.CustomerName = updatedOrder.CustomerName;
                existingOrder.OrderDate = updatedOrder.OrderDate;
                existingOrder.Items = updatedOrder.Items;

                await _context.SaveChangesAsync();
                await _cache.SetStringAsync($"order:{id}", JsonSerializer.Serialize(updatedOrder));
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError("OrderController - UpdateOrder: {0}", ex.ToString());
                return StatusCode(500, "Internal Server Error");
            }
            
        }
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(Guid id)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order == null) return NotFound();

                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError("OrderController - DeleteOrder: {0}", ex.ToString());
                return StatusCode(500, "Internal Server Error");
            }

        }
        [Authorize]
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(Guid id)
        {
            try
            {
                var order = await _context.Orders
    .Include(o => o.Items)
    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null) return NotFound();

                if (order.IsCanceled) return BadRequest("Đơn hàng đã bị huỷ trước đó.");
                var token = HttpContext.Request.Headers["Authorization"].ToString();
                // Gọi InventoryService để cộng lại tồn kho
                foreach (var item in order.Items)
                {
                    await _inventoryClient.ReleaseInventoryAsync(item.ProductId, item.Quantity, token);
                }

                order.IsCanceled = true;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError("OrderController - CancelOrder: {0}", ex.ToString());
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}
