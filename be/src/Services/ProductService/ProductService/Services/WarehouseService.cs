using Microsoft.Extensions.Logging;
using ProductService.Repositories.Interfaces;
using ProductService.Models.Entities;
using ProductService.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Services
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly ILogger<WarehouseService> _logger;

        public WarehouseService(IWarehouseRepository warehouseRepository, ILogger<WarehouseService> logger)
        {
            _warehouseRepository = warehouseRepository;
            _logger = logger;
        }

        public async Task<bool> CreateWarehouseAsync(Warehouse warehouse)
        {
            try
            {
                await _warehouseRepository.CreateWarehouseAsync(warehouse);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return false;
            }
        }

        public async Task<bool> DeleteWarehouseAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
               throw new Exception("Id is empty");  
            }

            try
            {
                await _warehouseRepository.DeleteWarehouseAsync(id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return false;
            }
        }

        public async Task<Warehouse> GetWarehouseByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new Exception("Id is empty");
            }

            return await _warehouseRepository.GetWarehouseByIdAsync(id);
        }

        public async Task<IEnumerable<Warehouse>> GetWarehousesAsync()
        {
            return await _warehouseRepository.GetWarehousesAsync();
        }

        public async Task<bool> UpdateWarehouseAsync(Warehouse warehouse)
        {
            try
            {
                await _warehouseRepository.UpdateWarehouseAsync(warehouse);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return false;
            }
        }
    }
}
