using Microsoft.Extensions.Configuration;
using PaymentService.Repositories.Interfaces;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Repositories
{
    public class RabbitMqConnection : IRabbitMqConnection, IDisposable
    {
        private IConnection? _connection;
        private readonly IConfiguration _configuration;
        private readonly object _lockObject = new object();

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

        public RabbitMqConnection(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private async Task InitializeConnection()
        {
            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQ:HostName"] ?? "rabbitmq",
                UserName = _configuration["RabbitMQ:UserName"] ?? "admin",
                Password = _configuration["RabbitMQ:Password"] ?? "admin",
            };

            _connection = await factory.CreateConnectionAsync();
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
