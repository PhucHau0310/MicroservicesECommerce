using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Models.DTOs
{
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> ImageUrl { get; set; } = new List<string>();
        public decimal Price { get; set; }
        public List<string> Colors { get; set; } = new List<string>();
        public string Size { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public Guid CategoryId { get; set; }
    }
}
