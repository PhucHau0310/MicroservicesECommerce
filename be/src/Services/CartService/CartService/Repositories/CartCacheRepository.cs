using CartService.Application.Interfaces;
using CartService.Domain.Entities;
using CartService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CartService.Infrastructure.Persistence
{
    public class CartCacheRepository : ICartCacheRepository
    {
        private readonly IDatabase _database;
        private readonly IProductHttpClient _productClient;
        private readonly ILogger<CartCacheRepository> _logger;
        private const string CART_KEY_PREFIX = "cart:";
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(24); // Automation delete after 24 hours


        public CartCacheRepository(IConnectionMultiplexer redis,
                 IProductHttpClient productClient,
                 ILogger<CartCacheRepository> logger)
        {
            _database = redis.GetDatabase();
            _productClient = productClient;
            _logger = logger;
        }

        private string GetCartKey(Guid userId)
        {
            return $"{CART_KEY_PREFIX}{userId}";
        }

        public IEnumerable<string> GetAllCartKeys()
        {
            var server = _database.Multiplexer.GetServer(_database.Multiplexer.GetEndPoints().First());
            var keys = server.Keys(pattern: $"{CART_KEY_PREFIX}*");
            return keys.Select(k => k.ToString());
        }

        public async Task<bool> AddItemToCartAsync(Guid userId, CartItem item)
        {
            try
            {
                // Get latest product info
                var product = await _productClient.GetProductAsync(item.ProductId);
                if (product == null)
                {
                    throw new InvalidOperationException("Product not found");
                }

                // Verify product is active
                if (!product.IsActive)
                {
                    throw new InvalidOperationException("Product is not available");
                }

                // Check stock availability
                var isStockAvailable = await _productClient.CheckStockAvailabilityAsync(
                    item.ProductId,
                    item.Quantity
                );

                if (!isStockAvailable)
                {
                    throw new InvalidOperationException("Insufficient stock available");
                }

                // Update cart item with latest product info
                item.ProductName = product.Name;
                item.Price = product.Price;
                item.ImageUrls = product.ImageUrl;

                var cart = await GetCartAsync(userId) ?? new Cart { UserId = userId };

                var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == item.ProductId);
                if (existingItem != null)
                {
                    // Check stock for combined quantity
                    isStockAvailable = await _productClient.CheckStockAvailabilityAsync(
                        item.ProductId,
                        existingItem.Quantity + item.Quantity
                    );

                    if (!isStockAvailable)
                    {
                        throw new InvalidOperationException("Insufficient stock available for combined quantity");
                    }

                    existingItem.Quantity += item.Quantity;
                    existingItem.Price = product.Price; // Update with latest price
                }
                else
                {
                    cart.Items.Add(item);
                }

                cart.LastUpdated = DateTime.UtcNow;

                var serializedCart = JsonSerializer.Serialize(cart);
                return await _database.StringSetAsync(
                    GetCartKey(userId),
                    serializedCart,
                    _cacheExpiration
                );
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error communicating with Product Service");
                throw new Exception("Unable to verify product availability", ex);
            }
        }

        public async Task<bool> UpdateItemQuantityAsync(Guid userId, Guid productId, int quantity)
        {
            if (quantity < 0)
                throw new ArgumentException("Quantity cannot be negative", nameof(quantity));

            var cart = await GetCartAsync(userId);
            if (cart == null)
                return false;

            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item == null)
                return false;

            try
            {
                if (quantity > 0)
                {
                    // Check stock availability
                    var isStockAvailable = await _productClient.CheckStockAvailabilityAsync(
                        productId,
                        quantity
                    );

                    if (!isStockAvailable)
                    {
                        throw new InvalidOperationException("Insufficient stock available");
                    }

                    // Get latest product info
                    var product = await _productClient.GetProductAsync(productId);
                    if (product == null)
                    {
                        throw new InvalidOperationException("Product not found");
                    }

                    if (!product.IsActive)
                    {
                        throw new InvalidOperationException("Product is not available");
                    }

                    item.Quantity = quantity;
                    item.Price = product.Price; // Update with latest price
                }
                else
                {
                    cart.Items.Remove(item);
                }

                cart.LastUpdated = DateTime.UtcNow;

                var serializedCart = JsonSerializer.Serialize(cart);
                return await _database.StringSetAsync(
                    GetCartKey(userId),
                    serializedCart,
                    _cacheExpiration
                );
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error communicating with Product Service");
                throw new Exception("Unable to verify product availability", ex);
            }
        }

        public async Task<Cart?> GetCartAsync(Guid userId)
        {
            var key = GetCartKey(userId);
            var data = await _database.StringGetAsync(key);
            if (data.IsNullOrEmpty)
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<Cart>(data!);
            }
            catch (JsonException)
            {
                await _database.KeyDeleteAsync(key); // Delete key if invalid
                return null;
            }
        }

        public async Task<bool> RemoveItemFromCartAsync(Guid userId, Guid productId)
        {
            var key = GetCartKey(userId);
            return await _database.HashDeleteAsync(key, productId.ToString());
        }

        public async Task<bool> ClearCartAsync(Guid userId)
        {
            var key = GetCartKey(userId);
            return await _database.KeyDeleteAsync(key);
        }
    }
}
