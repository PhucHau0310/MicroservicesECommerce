using Microsoft.EntityFrameworkCore;
using PaymentService.Events;
using PaymentService.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PaymentService.Repositories.Interfaces;
using PaymentService.Messaging.Interfaces;
using PaymentService.Data;

namespace PaymentService.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly PaymentDbContext _paymentDbContext;
        private readonly IPaymentProducer _paymentProducer;

        public PaymentRepository(PaymentDbContext paymentDbContext, IPaymentProducer paymentProducer)
        {
            _paymentDbContext = paymentDbContext;
            _paymentProducer = paymentProducer;
        }

        public async Task<Payment> AddPaymentAsync(Payment payment)
        {
            _paymentDbContext.Payments.Add(payment);
            await _paymentDbContext.SaveChangesAsync();

            return payment;
        }

        public async Task<Payment?> GetPaymentByIdAsync(Guid paymentId)
        {
            return await _paymentDbContext.Payments.FirstOrDefaultAsync(p => p.Id == paymentId);
        }

        public async Task<Payment?> GetPaymentByOrderIdAsync(string oderId)
        {
            return await _paymentDbContext.Payments.FirstOrDefaultAsync(p => p.OrderId == oderId);
        }

        public async Task<Payment> UpdatePaymentAsync(Payment payment)
        {
            _paymentDbContext.Payments.Update(payment);
            await _paymentDbContext.SaveChangesAsync();

            var message = new PaymentEvent
            {
                Id = payment.Id,
                OrderId = payment.OrderId,
                Amount = payment.Amount,
                UserId = payment.UserId,
                CreatedAt = DateTime.UtcNow,
            };

            // Send to RabbitMQ
            await _paymentProducer.SendMessage(message);

            return payment;
        }
    }
}
