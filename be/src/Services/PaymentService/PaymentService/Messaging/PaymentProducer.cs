using Microsoft.Extensions.Configuration;
using PaymentService.Messaging.Interfaces;
using PaymentService.Repositories.Interfaces;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PaymentService.Messaging
{
    public class PaymentProducer : IPaymentProducer
    {
        private readonly IRabbitMqConnection _connection;
        private readonly IConfiguration _configuration;

        public PaymentProducer(IRabbitMqConnection connection, IConfiguration configuration)
        {
            _connection = connection;
            _configuration = configuration;
        }

        public async Task SendMessage<T>(T message)
        {
            var queueName = _configuration["RabbitMQ:QueueName"] ?? "payment_notifications";
            using var channel = await _connection.Connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var jsonMessage = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(jsonMessage);

            var properties = new BasicProperties();
            properties.Persistent = true;

            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: queueName,
                mandatory: true,
                basicProperties: properties,
                body: body);
        }
    }
}
