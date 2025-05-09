using ProductService.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Services.Interfaces
{
    public interface IWarehouseService
    {
        Task<IEnumerable<Warehouse>> GetWarehousesAsync();
        Task<Warehouse> GetWarehouseByIdAsync(Guid id);
        Task<bool> CreateWarehouseAsync(Warehouse warehouseDto);
        Task<bool> UpdateWarehouseAsync(Warehouse warehouseDto);
        Task<bool> DeleteWarehouseAsync(Guid id);
    }
}
