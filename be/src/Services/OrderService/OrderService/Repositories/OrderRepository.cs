using MongoDB.Driver;
using OrderService.Events;
using OrderService.Data;
using OrderService.Models.Entities;
using OrderService.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IMongoCollection<Order> _orders;
        private readonly IMongoCollection<OrderItem> _orderItems;
        private readonly IOrderPlacedProducer _producer;

        public OrderRepository(OrderDbContext dbContext, IOrderPlacedProducer producer)
        {
            _orders = dbContext.Orders;
            _orderItems = dbContext.OrderItems;
            _producer = producer;
        }

        public async Task<bool> CreateOrderAsync(Order order)
        {
            if (order == null)
            {
                throw new ArgumentNullException(nameof(order));
            }

            // Save order to database
            await _orders.InsertOneAsync(order);

            var message = new OrderPlacedEvent
            {
                UserId = order.UserId,
                OrderId = order.Id.ToString(),
                Status = order.Status,
                Total = order.Total,
                CreatedAt = DateTime.UtcNow,
            };

            // Send to RabbitMQ
            await _producer.SendMessage(message);

            // Create list orderitems
            if (order.OrderItems != null && order.OrderItems.Count > 0)
            {
                await _orderItems.InsertManyAsync(order.OrderItems);
            }
            return true;
        }

        public async Task<bool> DeleteOrderAsync(string orderId)
        {
            var result = await _orders.DeleteOneAsync(o => o.Id == orderId);
            return result.DeletedCount > 0;
        }

        public async Task<Order?> GetOrderByIdAsync(string orderId)
        {
            if (orderId == string.Empty)
            {
                throw new ArgumentException("Invalid order id", nameof(orderId));
            }

            return await _orders.Find(order => order.Id == orderId).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersAsync()
        {
            return await _orders.Find(order => true).ToListAsync();
        }

        public async Task<bool> UpdateOrderAsync(Order order)
        {
            var result = await _orders.ReplaceOneAsync(o => o.Id == order.Id, order);
            return result.ModifiedCount > 0;
        }
    }
}
