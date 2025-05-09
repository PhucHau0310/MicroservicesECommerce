using Grpc.Core;
using OrderService.Protos;
using OrderService.Repositories.Interfaces;

namespace OrderService.Services
{
    public class OrderGrpcService : OrderGrpc.OrderGrpcBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrderGrpcService> _logger;

        public OrderGrpcService(IOrderRepository orderRepository, ILogger<OrderGrpcService> logger)
        {
            _orderRepository = orderRepository;
            _logger = logger;
        }

        public enum OrderStatus
        {
            Pending,
            Processing,
            Completed,
            Cancelled
        }

        public override async Task<UpdateOrderStatusResponse> UpdateOrderStatus(UpdateOrderStatusRequest request, ServerCallContext context)
        {
            try
            {
                var order = await _orderRepository.GetOrderByIdAsync(request.OrderId);
                if (order == null)
                {
                    _logger.LogWarning("Order {OrderId} not found", request.OrderId);
                    return new UpdateOrderStatusResponse { Success = false };
                }

                switch (request.Status)
                {
                    case "Completed":
                        order.Status = (Models.Entities.OrderStatus)OrderStatus.Completed;
                        break;

                    case "Processing":
                        order.Status = (Models.Entities.OrderStatus)OrderStatus.Processing;
                        break;

                    case "Cancelled":
                        order.Status = (Models.Entities.OrderStatus)OrderStatus.Cancelled;
                        break;
                }

                await _orderRepository.UpdateOrderAsync(order);
                _logger.LogInformation("Updated order {OrderId} status to {Status}", order.Id, order.Status);

                return new UpdateOrderStatusResponse { Success = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order {OrderId} status", request.OrderId);
                return new UpdateOrderStatusResponse { Success = false };
            }
        }
    }
}
