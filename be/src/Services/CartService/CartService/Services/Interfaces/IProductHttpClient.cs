using CartService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartService.Application.Interfaces
{
    public interface IProductHttpClient
    {
        Task<ProductDto?> GetProductAsync(Guid productId);
        Task<bool> CheckStockAvailabilityAsync(Guid productId, int requestedQuantity);
    }
}
