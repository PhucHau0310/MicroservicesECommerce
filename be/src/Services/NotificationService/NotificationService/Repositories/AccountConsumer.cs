using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotificationService.Domain.Interfaces;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using NotificationService.Application.Events;

namespace NotificationService.Infrastructure.Messaging
{
    public class AccountConsumer : IAccountConsumer
    {
        private readonly IRabbitMqConnection _connection;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountConsumer> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private IChannel? _channel;

        public AccountConsumer(
            IRabbitMqConnection connection,
            IConfiguration configuration,
            ILogger<AccountConsumer> logger,
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
                    queue: "account_notifications",
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
                    queue: "account_notifications",
                    autoAck: false,
                    consumer: consumer);

                _logger.LogInformation("Started listening for account events");
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
                    var accountEvent = JsonSerializer.Deserialize<AccountEvent>(message);

                    // Create notification message
                    string notificationMessage = accountEvent.Message;

                    // Save notification and send through SignalR
                    await notificationRepository.SendNotificationAsync(
                        accountEvent.UserId,
                        notificationMessage,
                        "Account"
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
