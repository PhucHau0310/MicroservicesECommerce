
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Models.Entities
{
    public enum Size
    {
        Small,
        Medium,
        Large,
        XLarge
    }

    public class Product
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required List<string> ImageUrl { get; set; }
        public required decimal Price { get; set; }
        public required List<string> Colors { get; set; } = new List<string>();
        public required Size Size { get; set; }
        public bool IsActive { get; set; } = true;
        public required Guid CategoryId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(7);
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow.AddHours(7);

        // Relationships
        public Category? Category { get; set; }
        public ICollection<Stock> Stocks { get; set; } = new List<Stock>();
    }
}
