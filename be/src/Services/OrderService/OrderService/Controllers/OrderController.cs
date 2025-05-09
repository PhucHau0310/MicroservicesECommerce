using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderService.Services;
using OrderService.Models.Entities;

namespace OrderService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly OrderService.Services.OrderService _orderService;
        private readonly ILogger<OrderController> _logger;
        private readonly PaymentGrpcClient _paymentGrpcClient;

        public OrderController(OrderService.Services.OrderService orderService, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
            _paymentGrpcClient = new PaymentGrpcClient();
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetOrdersAsync()
        {
            var orders = await _orderService.GetOrdersAsync();
            return Ok(orders);
        }

        [HttpGet("detail")]
        public async Task<IActionResult> GetOrderByIdAsync(string orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                return NotFound();
            }
            return Ok(order);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateOrderAsync([FromBody] Order order)
        {
            if (order.OrderItems == null || order.OrderItems.Count == 0)
            {
                return BadRequest("Order must have at least one item");
            }

            await _orderService.CreateOrderAsync(order);

            // Call payment service
            var paymentResponse = await _paymentGrpcClient.ProcessPaymentAsync(
                orderId: order.Id.ToString(),
                amount: order.Total,
                userId: order.UserId.ToString()
            );

            // Return URL checkout VNPay
            return Ok(new
            {
                OrderId = order.Id,
                PaymentId = paymentResponse.PaymentId,
                PaymentUrl = paymentResponse.PaymentUrl
            });
            //return Ok("Create order successfully");
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateOrderAsync([FromBody] Order order)
        {
            if (order == null)
            {
                return BadRequest();
            }
            await _orderService.UpdateOrderAsync(order);
            return NoContent();
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteOrderAsync(string orderId)
        {
            await _orderService.DeleteOrderAsync(orderId);
            return NoContent();
        }

        [HttpPut("cancel")]
        public async Task<IActionResult> CancelOrderAsync(string orderId)
        {
            await _orderService.CancelOrderAsync(orderId);
            return NoContent();
        }

        [HttpPut("complete")]
        public async Task<IActionResult> CompleteOrderAsync(string orderId)
        {
            await _orderService.CompleteOrderAsync(orderId);
            return NoContent();
        }
    }
}
