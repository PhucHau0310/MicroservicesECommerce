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
    public class StockService : IStockService
    {
        private readonly IStockRepository _stockRepository;
        private readonly ILogger<StockService> _logger;

        public StockService(IStockRepository stockRepository, ILogger<StockService> logger)
        {
            _stockRepository = stockRepository;
            _logger = logger;
        }

        public async Task<bool> CheckStockAvailabilityAsync(Guid productId, int quantityReq)
        {
            return await _stockRepository.CheckStockAvailabilityAsync(productId, quantityReq);
        }

        public async Task<bool> AddStock(Stock stockDto)
        {
            try
            {
                await _stockRepository.AddStock(stockDto);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return false;
            }
        }

        public async Task<bool> DeleteStock(Guid stockId)
        {
           if (stockId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(stockId));
            }
            try
            {
                await _stockRepository.DeleteStock(stockId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return false;
            }

        }

        public Task<Stock> GetStock(Guid productId)
        {
            try
            {
                if (productId == Guid.Empty)
                {
                    throw new ArgumentNullException(nameof(productId));
                }

                return _stockRepository.GetStockByProductId(productId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return null;
            }
        }

        public async Task<bool> UpdateStock(Stock stockDto)
        {
            try
            {
                if (stockDto == null)
                {
                    throw new ArgumentNullException(nameof(stockDto));
                }

                await _stockRepository.UpdateStock(stockDto);
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
