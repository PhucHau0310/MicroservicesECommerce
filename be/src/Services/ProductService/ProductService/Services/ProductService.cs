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
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger<ProductService> _logger;   

        public ProductService(IProductRepository productRepository, ILogger<ProductService> logger)
        {
            _productRepository = productRepository;
            _logger = logger;
        }

        public async Task<bool> AddProductAsync(Product product)
        {
            if (product == null)
            {
                _logger.LogError("Product is null");
                return false;   
            }

            var productRes = new Product
            {
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                Colors = product.Colors,
                Size = product.Size,
                CategoryId = product.CategoryId,
            };

            await _productRepository.AddAsync(productRes);
            return true;
        }

        public async Task<bool> DeleteProductAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                _logger.LogError("Id is empty");
                return false;
            }
            await _productRepository.DeleteAsync(id);
            return true;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetAllAsync();
            return products;
        }

        public async Task<Product?> GetProductByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                _logger.LogError("Id is empty");
                return null;
            }

            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return null;

            return product;
        }

        public async Task<bool> UpdateProductAsync(Product product)
        {
            if (product == null)
            {
                _logger.LogError("Product is null");
                return false;
            }

            var productResult = await _productRepository.GetByIdAsync(product.Id);
            if (productResult == null)
            {
                _logger.LogError("Product is null");
                return false;
            }

            productResult.Name = product.Name;
            productResult.Description = product.Description;
            productResult.Price = product.Price;
            productResult.CategoryId = product.CategoryId;

            await _productRepository.UpdateAsync(product);
            return true;
        }
    }
}
