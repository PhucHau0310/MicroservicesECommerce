
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Models.Entities
{
    public class Stock
    {
        public Guid Id { get; set; }
        public required int Quantity { get; set; }
        public required Guid ProductId { get; set; }
        public required Guid WarehouseId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(7);
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow.AddHours(7);

        // Relationships
        public Product? Products { get; set; }
        public Warehouse? Warehouse { get; set; }
    }
}
