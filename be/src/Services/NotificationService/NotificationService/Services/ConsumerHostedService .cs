using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
//using NotificationService.Application.Interfaces;
using NotificationService.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationService.Application.Services
{
    public class ConsumerHostedService : IHostedService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ConsumerHostedService> _logger;
        private IServiceScope _scope;
        private IOrderPlacedConsumer _orderPlacedConsumer;
        private IPaymentConsumer _paymentConsumer;
        private IAccountConsumer _accountConsumer;

        public ConsumerHostedService(
            IServiceScopeFactory scopeFactory,
            ILogger<ConsumerHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting RabbitMQ consumers...");
            // Create a long-lived scope
            _scope = _scopeFactory.CreateScope();

            _orderPlacedConsumer = _scope.ServiceProvider.GetRequiredService<IOrderPlacedConsumer>();
            _paymentConsumer = _scope.ServiceProvider.GetRequiredService<IPaymentConsumer>();
            _accountConsumer = _scope.ServiceProvider.GetRequiredService<IAccountConsumer>();

            try
            {
                // Start consumers
                await Task.WhenAll(
                    _orderPlacedConsumer.StartAsync(cancellationToken),
                    _paymentConsumer.StartAsync(cancellationToken),
                    _accountConsumer.StartAsync(cancellationToken)
                );

                _logger.LogInformation("RabbitMQ consumer started successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start RabbitMQ consumer");

                // Cleanup on failure
                if (_scope != null)
                {
                    _scope.Dispose();
                    _scope = null;
                }

                throw;
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping RabbitMQ consumers...");

            try
            {
                if (_orderPlacedConsumer != null && _paymentConsumer != null && _accountConsumer != null)
                {
                    // Stop all consumers
                    await Task.WhenAll(
                        _orderPlacedConsumer.StopAsync(cancellationToken),
                        _paymentConsumer.StopAsync(cancellationToken),
                        _accountConsumer.StopAsync(cancellationToken)
                    );
                }

                _logger.LogInformation("RabbitMQ consumers stopped successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping RabbitMQ consumers");
                throw;
            }
            finally
            {
                // Dispose scope
                if (_scope != null)
                {
                    _scope.Dispose();
                    _scope = null;
                }
            }
        }
    }
}
