using ProductService.Data;
using ProductService.Models.Entities;
using ProductService.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Repositories
{ 
    public class StockRepository : IStockRepository
    {
        private readonly ProductDbContext _dbContext;

        public StockRepository(ProductDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddStock(Stock stock)
        {
            await _dbContext.Stocks.AddAsync(stock);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> CheckStockAvailabilityAsync(Guid productId, int quantityReq)
        {
            var stock = await _dbContext.Stocks.FindAsync(productId);
            return stock.Quantity > quantityReq;
        }

        public async Task DeleteStock(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(id));
            }

            var stock = await _dbContext.Stocks.FindAsync(id);
            if (stock == null)
            {
                throw new ArgumentNullException(nameof(stock));
            }

            _dbContext.Stocks.Remove(stock);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Stock> GetStockByProductId(Guid productId)
        {
           if (productId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(productId));
            }
            var stock = await _dbContext.Stocks.FindAsync(productId);
            if (stock == null)
            {
                throw new ArgumentNullException(nameof(stock));
            }
            return stock;
        }

        public async Task<Stock> GetStockByProductIdAndWarehouseId(Guid productId, Guid warehouseId)
        {
            if (productId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(productId));
            }

            if (warehouseId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(warehouseId));
            }

            var stock = await _dbContext.Stocks.FindAsync(productId, warehouseId);
            if (stock == null)
            {
                throw new ArgumentNullException(nameof(stock));
            }

            return stock;
        }

        public async Task<Stock> GetStockByWarehouseId(Guid warehouseId)
        {
            if (warehouseId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(warehouseId));
            }

            var stock = await _dbContext.Stocks.FindAsync(warehouseId);
            if (stock == null)
            {
                throw new ArgumentNullException(nameof(stock));
            }

            return stock;
        }

        public async Task UpdateStock(Stock stock)
        {
            _dbContext.Stocks.Update(stock);
            await _dbContext.SaveChangesAsync();
        }
    }
}
