using ProductService.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Repositories.Interfaces
{
    public interface IStockRepository
    {
        Task<Stock> GetStockByProductId(Guid productId);
        Task<Stock> GetStockByWarehouseId(Guid warehouseId);
        Task<Stock> GetStockByProductIdAndWarehouseId(Guid productId, Guid warehouseId);
        Task AddStock(Stock stock);
        Task UpdateStock(Stock stock);
        Task DeleteStock(Guid id);
        Task<bool> CheckStockAvailabilityAsync(Guid productId, int quantityReq);
    }
}
