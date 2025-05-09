using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductService.Services.Interfaces;
using ProductService.Models.Entities;

namespace ProductService.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class WarehouseController : ControllerBase
    {
        private readonly IWarehouseService _warehouseService;
        private readonly ILogger<WarehouseController> _logger;

        public WarehouseController(IWarehouseService warehouseService, ILogger<WarehouseController> logger)
        {
            _warehouseService = warehouseService;
            _logger = logger;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetWarehouses()
        {
            try
            {
                var warehouses = await _warehouseService.GetWarehousesAsync();
                return Ok(warehouses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all warehouses");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("detail")]
        public async Task<IActionResult> GetWarehouseById(Guid id)
        {
            try
            {
                var warehouse = await _warehouseService.GetWarehouseByIdAsync(id);
                return Ok(warehouse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching the warehouse");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateWarehouse([FromBody] Warehouse warehouseDto)
        {
            try
            {
                var result = await _warehouseService.CreateWarehouseAsync(warehouseDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the warehouse");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateWarehouse([FromBody] Warehouse warehouseDto)
        {
            try
            {
                var result = await _warehouseService.UpdateWarehouseAsync(warehouseDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the warehouse");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteWarehouse(Guid id)
        {
            try
            {
                var result = await _warehouseService.DeleteWarehouseAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the warehouse");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
