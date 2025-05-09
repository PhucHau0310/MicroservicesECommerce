using Microsoft.Extensions.Configuration;
using OrderService.Events;
using OrderService.Repositories.Interfaces;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OrderService.Repositories
{
    public class OrderPlacedProducer : IOrderPlacedProducer
    {
        private readonly IRabbitMqConnection _connection;
        private readonly IConfiguration _configuration;

        public OrderPlacedProducer(IRabbitMqConnection connection, IConfiguration configuration)
        {
            _connection = connection;
            _configuration = configuration;
        }

        public async Task SendMessage<T>(T message)
        {
            var queueName = _configuration["RabbitMQ:QueueName"] ?? "order_notifications";
            using var channel = await _connection.Connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() }
            };

            var jsonMessage = JsonSerializer.Serialize(message, options);
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
