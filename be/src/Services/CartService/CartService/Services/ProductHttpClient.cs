using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CartService.Application.Interfaces;
using CartService.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace CartService.Application.Services
{
    public class ProductHttpClient : IProductHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProductHttpClient> _logger;

        public ProductHttpClient(HttpClient httpClient, ILogger<ProductHttpClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<ProductDto?> GetProductAsync(Guid productId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/product/detail?id={productId}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ProductDto>();
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                throw new HttpRequestException($"Error getting product: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product {ProductId}", productId);
                throw;
            }
        }

        public async Task<bool> CheckStockAvailabilityAsync(Guid productId, int requestedQuantity)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/stock?productId={productId}&quantityReq={requestedQuantity}");

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Error checking stock: {response.StatusCode}");
                }

                return await response.Content.ReadFromJsonAsync<bool>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking stock for product {ProductId}", productId);
                throw;
            }
        }
    }
}
