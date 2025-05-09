using Grpc.Core;
using Grpc.Net.Client;
using PaymentService.Protos;
using System.Threading;

namespace PaymentService.GrpcServices
{
    public class OrderGrpcClient
    {
        private readonly OrderGrpc.OrderGrpcClient _orderServiceClient;

        public OrderGrpcClient()
        {

            var httpHandler = new HttpClientHandler();

            // Skip SSL certificate validation in development enviroment
            httpHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            // Connect to gRPC Server (OrderGrpcService)
            var channel = GrpcChannel.ForAddress("https://orderservice.api:8081/", new GrpcChannelOptions
            {
                HttpHandler = httpHandler,
                ThrowOperationCanceledOnCancellation = true
            });

            _orderServiceClient = new OrderGrpc.OrderGrpcClient(channel);
        }

        public async Task<UpdateOrderStatusResponse> UpdateOrderStatusAsync(string orderId, string status, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new UpdateOrderStatusRequest
                {
                    OrderId = orderId,
                    Status = status
                };

                var response = await _orderServiceClient.UpdateOrderStatusAsync(
                request,
                    cancellationToken: cancellationToken);

                return response;
            }
            catch (RpcException ex)
            {
                return new UpdateOrderStatusResponse { Success = false };
            }
        }
    }
}
