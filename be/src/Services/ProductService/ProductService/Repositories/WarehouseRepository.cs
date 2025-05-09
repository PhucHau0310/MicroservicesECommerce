using Microsoft.EntityFrameworkCore;
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
    public class WarehouseRepository : IWarehouseRepository
    {
        private readonly ProductDbContext _dbContext;

        public WarehouseRepository(ProductDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task CreateWarehouseAsync(Warehouse warehouse)
        {
            if (warehouse == null)
            {
                throw new ArgumentNullException(nameof(warehouse));
            }

            await _dbContext.Warehouses.AddAsync(warehouse);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteWarehouseAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(id));
            }

            var warehouse = await _dbContext.Warehouses.FindAsync(id);

            if (warehouse == null)
            {
                throw new ArgumentNullException(nameof(warehouse));
            }

            _dbContext.Warehouses.Remove(warehouse);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Warehouse> GetWarehouseByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(id));
            }

            var warehouse = await _dbContext.Warehouses.FindAsync(id);
            if (warehouse == null)
            {
                throw new ArgumentNullException(nameof(warehouse));
            }

            return warehouse;
        }

        public async Task<IEnumerable<Warehouse>> GetWarehousesAsync()
        {
            return await _dbContext.Warehouses.Include(s => s.Stocks)
                                              .ToListAsync();
        }

        public async Task UpdateWarehouseAsync(Warehouse warehouse)
        {
            if (warehouse == null)
            {
                throw new ArgumentNullException(nameof(warehouse));
            }

            _dbContext.Warehouses.Update(warehouse);
            await _dbContext.SaveChangesAsync();

        }
    }
}
