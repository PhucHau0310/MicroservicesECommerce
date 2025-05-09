using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Models.Entities
{
    public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed,
        Refunded,
    }

    public enum PaymentMethod
    {
        Momo,
        VNPay,
        CashOnDelivery
    }

    public class Payment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required Guid UserId { get; set; }
        public required string OrderId { get; set; }
        public PaymentMethod Method { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
    }
}
