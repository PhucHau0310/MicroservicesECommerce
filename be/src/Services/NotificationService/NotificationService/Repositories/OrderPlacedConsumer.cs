//using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Events;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Interfaces;
using NotificationService.Infrastructure.Persistence;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NotificationService.Application.Messaging
{
    public class OrderPlacedConsumer : IOrderPlacedConsumer
    {
        private readonly IRabbitMqConnection _connection;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OrderPlacedConsumer> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private IChannel? _channel;

        public OrderPlacedConsumer(
            IRabbitMqConnection connection,
            IConfiguration configuration,
            ILogger<OrderPlacedConsumer> logger,
            IServiceScopeFactory scopeFactory)
        {
            _connection = connection;
            _configuration = configuration;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _channel = await _connection.Connection.CreateChannelAsync();

                await _channel.QueueDeclareAsync(
                    queue: _configuration["RabbitMQ:QueueName"] ?? "order_notifications",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                // Set prefetch count to control how many messages can be processed simultaneously
                await _channel.BasicQosAsync(
                    prefetchSize: 0,
                    prefetchCount: 1,
                    global: false);

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.ReceivedAsync += HandleMessageAsync;

                await _channel.BasicConsumeAsync(
                    queue: _configuration["RabbitMQ:QueueName"] ?? "order_notifications",
                    autoAck: false,
                    consumer: consumer);

                _logger.LogInformation("Started listening for order placed events");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting consumer");
                throw;
            }
        }

        private async Task HandleMessageAsync(object sender, BasicDeliverEventArgs ea)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var notificationRepository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
                try
                {
                    var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var options = new JsonSerializerOptions
                    {
                        Converters = { new JsonStringEnumConverter() }
                    };
                    var orderPlacedEvent = JsonSerializer.Deserialize<OrderPlacedEvent>(message, options);

                    _logger.LogInformation("Processing order {OrderId} for user {UserId}",
                    orderPlacedEvent?.OrderId, orderPlacedEvent?.UserId);

                    // Create notification message
                    string notificationMessage = $"Your order #{orderPlacedEvent?.OrderId} for ${orderPlacedEvent?.Total} has been placed successfully.";

                    // Save notification and send through SignalR
                    await notificationRepository.SendNotificationAsync(
                        orderPlacedEvent.UserId,
                        notificationMessage,
                        "OrderPlaced"
                    );

                    // Acknowledge the message
                    await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message");

                    // Reject the message and requeue if it's a transient error
                    await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
                }
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_channel?.IsOpen ?? false)
                {
                    await Task.Run(() => _channel?.CloseAsync(), cancellationToken);
                    await Task.Run(() => _channel?.Dispose(), cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping consumer");
                throw;
            }
        }
    }
}
