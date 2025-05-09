using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NotificationService.Domain.Interfaces;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationService.Infrastructure.Persistence
{
    public class RabbitMqConnection : IRabbitMqConnection, IDisposable
    {
        private IConnection? _connection;
        private readonly IConfiguration _configuration;
        private readonly object _lockObject = new object();
        private readonly ILogger<RabbitMqConnection> _logger;

        public IConnection Connection
        {
            get
            {
                if (_connection == null || !_connection.IsOpen)
                {
                    lock (_lockObject)
                    {
                        if (_connection == null || !_connection.IsOpen)
                        {
                            InitializeConnection().GetAwaiter().GetResult();
                        }
                    }
                }
                return _connection!;
            }
        }

        public RabbitMqConnection(IConfiguration configuration, ILogger<RabbitMqConnection> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private async Task InitializeConnection()
        {
            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = _configuration["RabbitMQ:HostName"] ?? "rabbitmq",
                    UserName = _configuration["RabbitMQ:UserName"] ?? "admin",
                    Password = _configuration["RabbitMQ:Password"] ?? "admin",
                    // DispatchConsumersAsync = true  // Enable async consumer dispatch
                };

                factory.ConsumerDispatchConcurrency = 1;

                _connection = await factory.CreateConnectionAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing RabbitMQ connection");
                throw;
            }
        }

        public void Dispose()
        {
            if (_connection?.IsOpen ?? false)
            {
                _connection?.Dispose();
            }
        }
    }
}
