using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartService.Domain.Entities
{
    public enum Size
    {
        Small,
        Medium,
        Large,
        XLarge
    }

    public class CartItem
    {
        public required Guid ProductId { get; set; }
        public required string ProductName { get; set; }
        public required decimal Price { get; set; }
        public required int Quantity { get; set; }
        public required string Color { get; set; }
        public required Size Size { get; set; }
        public List<string> ImageUrls { get; set; } = new();
    }
}
