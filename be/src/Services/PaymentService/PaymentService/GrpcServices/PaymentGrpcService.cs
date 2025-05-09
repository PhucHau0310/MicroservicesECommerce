using Grpc.Core;
//using MassTransit.Mediator;
using PaymentService.Protos;
using PaymentService.Models.DTOs;
using PaymentService.Services.Interfaces;
using PaymentService.Models.Entities;

namespace PaymentService.GrpcServices
{
    public class PaymentGrpcService : PaymentGrpc.PaymentGrpcBase
    {
        private readonly IVnPayService _vnPayService;
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentGrpcService> _logger;

        public PaymentGrpcService(
           IVnPayService vnPayService,
           IPaymentService paymentService,
           ILogger<PaymentGrpcService> logger)
        {
            _vnPayService = vnPayService;
            _paymentService = paymentService;
            _logger = logger;
        }

        public override async Task<ProcessPaymentResponse> ProcessPayment(ProcessPaymentRequest request, ServerCallContext context)
        {

            try
            {
                // Create payment record
                var payment = new Payment
                {
                    OrderId = request.OrderId,
                    Amount = Convert.ToDecimal(request.Amount),
                    UserId = Guid.Parse(request.UserId),
                    Method = PaymentMethod.VNPay,
                    Status = PaymentStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                // Save payment to database 
                var savedPayment = await _paymentService.AddPaymentAsync(payment);

                // Create URL checkout VNPay
                var paymentUrl = _vnPayService.CreatePaymentUrl(new VnPayReq
                {
                    Amount = Convert.ToDecimal(request.Amount),
                    OrderInfo = savedPayment.Id.ToString()
                }, "127.0.0.1");

                _logger.LogInformation($"Created VNPay payment URL for Payment ID: {savedPayment.Id}");

                return new ProcessPaymentResponse
                {
                    PaymentId = savedPayment.Id.ToString(),
                    Status = "Pending",
                    PaymentUrl = paymentUrl
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment through gRPC");
                throw new RpcException(new Status(StatusCode.Internal, "Error processing payment"));
            }
        }
    }
}
