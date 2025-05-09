using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartService.Domain.Entities
{
    public class Cart
    {
        public required Guid UserId { get; set; }
        public List<CartItem> Items { get; set; } = new();
        public decimal Total => Items.Sum(item => item.Price * item.Quantity);
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
