using OrderService.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Events
{
    public class OrderPlacedEvent
    {
        public string OrderId { get; set; } = string.Empty;
        public Guid UserId { get; set; } = Guid.Empty;
        public OrderStatus Status { get; set; }
        public double Total { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
