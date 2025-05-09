using ProductService.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Services.Interfaces
{
    public interface IStockService 
    {
        Task<Stock> GetStock(Guid productId);
        Task<bool> UpdateStock(Stock stockDto);
        Task<bool> DeleteStock(Guid stockId);
        Task<bool> AddStock(Stock stockDto);
        Task<bool> CheckStockAvailabilityAsync(Guid productId, int quantityReq);
    }
}
