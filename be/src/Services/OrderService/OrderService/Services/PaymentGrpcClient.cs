using Grpc.Net.Client;
using OrderService.Protos;

namespace OrderService.Services
{
    public class PaymentGrpcClient
    {
        private readonly PaymentGrpc.PaymentGrpcClient _client;

        public PaymentGrpcClient()
        {
            var httpHandler = new HttpClientHandler();

            // Skip SSL certificate validation in development enviroment
            httpHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            // Connect to gRPC Server (PaymentGrpcService)
            var channel = GrpcChannel.ForAddress("https://paymentservice.api:8081/", new GrpcChannelOptions
            {
                HttpHandler = httpHandler,
                ThrowOperationCanceledOnCancellation = true
            });

            _client = new PaymentGrpc.PaymentGrpcClient(channel);
        }

        public async Task<ProcessPaymentResponse> ProcessPaymentAsync(string orderId, double amount, string userId)
        {
            var request = new ProcessPaymentRequest
            {
                OrderId = orderId,
                Amount = amount,
                UserId = userId
            };

            return await _client.ProcessPaymentAsync(request);
        }
    }
}
