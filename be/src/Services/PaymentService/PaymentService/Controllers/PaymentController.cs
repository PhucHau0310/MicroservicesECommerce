using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentService.GrpcServices;
using PaymentService.Models.DTOs;
using PaymentService.Models.Entities;
using PaymentService.Services.Interfaces;

namespace PaymentService.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly ILogger<PaymentController> _logger;
        private readonly IVnPayService _vnPayService;
        private readonly IPaymentService _paymentService;
        private readonly OrderGrpcClient _orderGrpcClient;
        
        public PaymentController(ILogger<PaymentController> logger, IVnPayService vnPayService, IPaymentService paymentService)
        {
            _logger = logger;
            _vnPayService = vnPayService;
            _paymentService = paymentService;
            _orderGrpcClient = new OrderGrpcClient();
        }

        [Authorize]
        [HttpPost("vnpay")]
        public async Task<IActionResult> CreateVnPayPayment([FromBody] Payment request)
        {
            try
            {
                if (request.UserId == Guid.Empty || string.IsNullOrEmpty(request.OrderId))
                {
                    _logger.LogWarning("Invalid request: UserID or OrderID is missing");
                    return BadRequest("UserID and OrderID are required");
                }

                // Set initial payment properties
                request.Method = PaymentMethod.VNPay;
                request.Status = PaymentStatus.Pending;
                request.CreatedAt = DateTime.UtcNow;

                var payment = await _paymentService.AddPaymentAsync(request);
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

                // Create VNPay payment URL
                var paymentUrl = _vnPayService.CreatePaymentUrl(new VnPayReq
                {
                    Amount = request.Amount,
                    OrderInfo = payment.Id.ToString()
                }, ipAddress);

                _logger.LogInformation("Created VNPay payment URL for Payment ID: {PaymentId}", payment.Id);

                return Ok(new { PaymentUrl = paymentUrl, PaymentId = payment.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating VNPay payment");
                return StatusCode(500, "An error occurred while processing your payment");
            }
        }

        [AllowAnonymous]
        [HttpGet("vnpay-return")]
        public async Task<IActionResult> VnPayReturn()
        {
            try
            {
                var collections = HttpContext.Request.Query; // Access direct query string from HttpContext
                _logger.LogInformation("VNPay return query: {0}", collections.ToString());

                var isValidSignature = _vnPayService.ValidateCallback(collections);
                if (!isValidSignature)
                {
                    _logger.LogError("Invalid VNPay signature");
                    return BadRequest("Invalid signature");
                }

                var orderInfo = collections["vnp_OrderInfo"].ToString();
                if (!Guid.TryParse(orderInfo, out Guid paymentId))
                {
                    _logger.LogError($"Invalid OrderInfo format: {orderInfo}");
                    return BadRequest("Invalid order info");
                }

                var payment = await _paymentService.GetPaymentByIdAsync(paymentId);
                if (payment == null)
                {
                    _logger.LogError("Payment not found: {0}", paymentId);
                    return NotFound($"Payment {paymentId} not found");
                }

                var vnPayResponseCode = collections["vnp_ResponseCode"].ToString();
                payment.Status = vnPayResponseCode == "00" ? PaymentStatus.Completed : PaymentStatus.Failed;

                await _paymentService.UpdatePaymentAsync(payment);
                _logger.LogInformation("Payment {0} status updated to {1}", paymentId, payment.Status);

                string statusOrder = vnPayResponseCode == "00" ? "Completed" : "Cancelled";
                var orderResponse = await _orderGrpcClient.UpdateOrderStatusAsync(payment.OrderId, statusOrder, default);

                return Ok(new { Success = vnPayResponseCode == "00" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing VNPay return");
                return StatusCode(500, "An error occurred while processing the payment response");
            }
        }
    }
}
