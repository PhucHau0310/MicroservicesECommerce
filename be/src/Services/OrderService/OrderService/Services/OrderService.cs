using Microsoft.Extensions.Logging;
using OrderService.Models.Entities;
using OrderService.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Services
{
    public class OrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrderService> _logger;

        public OrderService(IOrderRepository orderRepository, ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository;
            _logger = logger;
        }

        public async Task<bool> CreateOrderAsync(Order order)
        {
            // Ensure orderitem have orderid when create
            foreach (var item in order.OrderItems)
            {
                item.OrderId = order.Id;
            }

            return await _orderRepository.CreateOrderAsync(order);
        }

        public async Task<bool> DeleteOrderAsync(string orderId)
        {
            if (orderId == string.Empty)
            {
                _logger.LogError("Invalid order id");
                throw new ArgumentException("Invalid order id", nameof(orderId));
            }

            return await _orderRepository.DeleteOrderAsync(orderId);
        }

        public async Task<Order?> GetOrderByIdAsync(string orderId)
        {
            if (orderId == string.Empty)
            {
                _logger.LogError("Invalid order id");
                throw new ArgumentException("Invalid order id", nameof(orderId));
            }

            return await _orderRepository.GetOrderByIdAsync(orderId);
        }

        public async Task<IEnumerable<Order>> GetOrdersAsync()
        {
            return await _orderRepository.GetOrdersAsync();
        }

        public async Task<bool> UpdateOrderAsync(Order order)
        {
            return await _orderRepository.UpdateOrderAsync(order);
        }

        public async Task<bool> CancelOrderAsync(string orderId)
        {
            if (orderId == string.Empty)
            {
                _logger.LogError("Invalid order id");
                throw new ArgumentException("Invalid order id", nameof(orderId));
            }

            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                throw new ArgumentException("Invalid order id", nameof(orderId));
            }
            order.Status = OrderStatus.Cancelled;
            return await _orderRepository.UpdateOrderAsync(order);
        }

        public async Task<bool> CompleteOrderAsync(string orderId)
        {
            if (orderId == string.Empty)
            {
                _logger.LogError("Invalid order id");
                throw new ArgumentException("Invalid order id", nameof(orderId));
            }

            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                throw new ArgumentException("Invalid order id", nameof(orderId));
            }
            order.Status = OrderStatus.Completed;
            return await _orderRepository.UpdateOrderAsync(order);
        }

        public async Task<bool> ProcessOrderAsync(string orderId)
        {
            if (orderId == string.Empty)
            {
                _logger.LogError("Invalid order id");
                throw new ArgumentException("Invalid order id", nameof(orderId));
            }

            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                throw new ArgumentException("Invalid order id", nameof(orderId));
            }
            order.Status = OrderStatus.Processing;
            return await _orderRepository.UpdateOrderAsync(order);
        }

        public async Task<bool> RevertOrderAsync(string orderId)
        {
            if (orderId == string.Empty)
            {
                _logger.LogError("Invalid order id");
                throw new ArgumentException("Invalid order id", nameof(orderId));
            }

            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                throw new ArgumentException("Invalid order id", nameof(orderId));
            }
            order.Status = OrderStatus.Pending;
            return await _orderRepository.UpdateOrderAsync(order);
        }

        public async Task<bool> AddOrderItemAsync(string orderId, OrderItem orderItem)
        {
            if (orderId == string.Empty)
            {
                _logger.LogError("Invalid order id");
                throw new ArgumentException("Invalid order id", nameof(orderId));
            }

            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                throw new ArgumentException("Invalid order id", nameof(orderId));
            }
            order.OrderItems.Add(orderItem);
            return await _orderRepository.UpdateOrderAsync(order);
        }
    }
}
