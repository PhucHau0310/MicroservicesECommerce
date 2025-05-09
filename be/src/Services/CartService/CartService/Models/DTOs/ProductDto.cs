using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartService.Application.DTOs
{
    public enum Size
    {
        Small,
        Medium,
        Large,
        XLarge
    }

    public class ProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> ImageUrl { get; set; } = new();
        public decimal Price { get; set; }
        public List<string> Colors { get; set; } = new();
        public Size Size { get; set; }
        public bool IsActive { get; set; }
        public List<StockDto> Stocks { get; set; } = new();
    }
}
