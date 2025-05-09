using Microsoft.Extensions.Logging;
using PaymentService.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PaymentService.Services.Interfaces;
using PaymentService.Repositories.Interfaces;

namespace PaymentService.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(IPaymentRepository paymentRepository, ILogger<PaymentService> logger)
        {
            _paymentRepository = paymentRepository;
            _logger = logger;
        }

        public async Task<Payment> AddPaymentAsync(Payment payment)
        {
            var res = await _paymentRepository.AddPaymentAsync(payment);
            return res;
        }

        public async Task<Payment?> GetPaymentByIdAsync(Guid paymentId)
        {
            return await _paymentRepository.GetPaymentByIdAsync(paymentId);
        }

        public async Task<Payment?> GetPaymentByOrderIdAsync(string oderId)
        {
            if (string.IsNullOrEmpty(oderId))
            {
                _logger.LogWarning("Order ID is required");
                throw new ArgumentNullException(nameof(oderId));
            }

            return await _paymentRepository.GetPaymentByOrderIdAsync(oderId);
        }

        public async Task<Payment> UpdatePaymentAsync(Payment payment)
        {
            var res = await _paymentRepository.UpdatePaymentAsync(payment);
            return res;
        }
    }
}
