using PaymentService.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<Payment?> GetPaymentByOrderIdAsync(string oderId);
        Task<Payment?> GetPaymentByIdAsync(Guid paymentId);
        Task<Payment> AddPaymentAsync(Payment payment);
        Task<Payment> UpdatePaymentAsync(Payment payment);
    }
}
