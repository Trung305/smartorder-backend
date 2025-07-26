using AutoMapper;
using InventoryService.Data;
using InventoryService.Dtos;
using InventoryService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace InventoryService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly InventoryDbContext _context;
        private readonly IMapper _mapper;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(InventoryDbContext context, IMapper mapper, IHttpClientFactory httpClientFactory, ILogger<InventoryController> logger)
        {
            _context = context;
            _mapper = mapper;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // GET: api/inventory
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryReadDto>>> GetAll()
        {
            try
            {
                var inventories = await _context.Inventories.ToListAsync();
                return Ok(_mapper.Map<IEnumerable<InventoryReadDto>>(inventories));
            }
            catch (Exception ex)
            {
                _logger.LogError("InventoryController - GetAll: {0}", ex.ToString());
                return StatusCode(500, "Internal Server Error");
            }

        }

        // GET: api/inventory/5
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<InventoryReadDto>> GetById(int id)
        {
            try
            {
                var inventory = await _context.Inventories.FindAsync(id);
                if (inventory == null) return NotFound();

                return Ok(_mapper.Map<InventoryReadDto>(inventory));
            }
            catch (Exception ex)
            {
                _logger.LogError("InventoryController - GetById: {0}", ex.ToString());
                return StatusCode(500, "Internal Server Error");
            }

        }

        // GET: api/inventory/product/{productId}
        [AllowAnonymous]
        [HttpGet("product/{productId}")]
        public async Task<ActionResult<int>> GetByProductId(Guid productId)
        {
            try
            {
                var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.ProductId == productId);
                if (inventory == null) return NotFound();

                return inventory.Quantity;
            }
            catch (Exception ex)
            {
                _logger.LogError("InventoryController - GetByProductId: {0}", ex.ToString());
                return StatusCode(500, "Internal Server Error");
            }

        }

        // POST: api/inventory
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<InventoryReadDto>> Create(InventoryCreateDto dto)
        {
            try
            {
                var inventory = _mapper.Map<Inventory>(dto);
                _context.Inventories.Add(inventory);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetById), new { id = inventory.Id }, _mapper.Map<InventoryReadDto>(inventory));
            }
            catch (Exception ex)
            {
                _logger.LogError("InventoryController - Create: {0}", ex.ToString());
                return StatusCode(500, "Internal Server Error");
            }
           
        }

        // PUT: api/inventory/5
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, InventoryUpdateDto dto)
        {
            try
            {
                if (id != dto.Id) return BadRequest();

                var inventory = await _context.Inventories.FindAsync(id);
                if (inventory == null) return NotFound();

                _mapper.Map(dto, inventory);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError("InventoryController - Update: {0}", ex.ToString());
                return StatusCode(500, "Internal Server Error");
            }

        }

        // DELETE: api/inventory/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var inventory = await _context.Inventories.FindAsync(id);
                if (inventory == null) return NotFound();

                _context.Inventories.Remove(inventory);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError("InventoryController - Delete: {0}", ex.ToString());
                return StatusCode(500, "Internal Server Error");
            }

        }
        [Authorize]
        [HttpPost("reserve")]
        public async Task<IActionResult> CheckAndReserve([FromBody] ReserveRequest request)
        {
            try
            {
                var inventory = await _context.Inventories
    .FirstOrDefaultAsync(i => i.ProductId == request.ProductId);

                if (inventory == null)
                    return NotFound("Product inventory not found");

                if (inventory.Quantity < request.Quantity)
                    return BadRequest("Not enough stock");

                inventory.Quantity -= request.Quantity;
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError("InventoryController - CheckAndReserve: {0}", ex.ToString());
                return StatusCode(500, "Internal Server Error");
            }

        }
        [Authorize]
        [HttpPost("release")]
        public async Task<IActionResult> ReleaseInventory([FromBody] ReserveRequest request)
        {
            try
            {
                var inventory = await _context.Inventories
    .FirstOrDefaultAsync(x => x.ProductId == request.ProductId);

                if (inventory == null) return NotFound();

                inventory.Quantity += request.Quantity;
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError("InventoryController - ReleaseInventory: {0}", ex.ToString());
                return StatusCode(500, "Internal Server Error");
            }

        }
        [AllowAnonymous]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllQuantities()
        {
            try
            {
                var items = await _context.Inventories.ToListAsync();
                var result = items.ToDictionary(i => i.ProductId, i => i.Quantity);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("InventoryController - GetAllQuantities: {0}", ex.ToString());
                return StatusCode(500, "Internal Server Error");
            }

        }
    }
}
