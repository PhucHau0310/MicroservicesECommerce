using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductService.Services.Interfaces;
using ProductService.Models.Entities;

namespace ProductService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly IStockService _stockService;
        private readonly ILogger<StockController> _logger;

        public StockController(IStockService stockService, ILogger<StockController> logger)
        {
            _stockService = stockService;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet("detail")]
        public async Task<IActionResult> GetStock(Guid productId)
        {
            try
            {
                var stock = await _stockService.GetStock(productId);
                return Ok(stock);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stock");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving stock");
            }
        }

        [AllowAnonymous]
        [HttpGet("check")]
        public async Task<IActionResult> CheckQuantity(Guid productId, int quantityReq)
        {
            try
            {
                var stock = await _stockService.CheckStockAvailabilityAsync(productId, quantityReq);
                return Ok(stock);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stock");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving stock");
            }
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> AddStock([FromBody] Stock stockDto)
        {
            try
            {
                var result = await _stockService.AddStock(stockDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding stock");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error adding stock");
            }
        }

        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateStock(Stock stockDto)
        {
            try
            {
                var result = await _stockService.UpdateStock(stockDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stock");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating stock");
            }
        }

        [Authorize]
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteStock(Guid stockId)
        {
            try
            {
                var result = await _stockService.DeleteStock(stockId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting stock");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting stock");
            }
        }
    }
}
