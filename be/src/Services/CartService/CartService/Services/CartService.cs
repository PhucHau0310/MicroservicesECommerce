using CartService.Domain.Entities;
using CartService.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartService.Application.Services
{
    public class CartService
    {
        private readonly ICartCacheRepository _cartCacheRepository;

        public CartService(ICartCacheRepository cartCacheRepository)
        {
            _cartCacheRepository = cartCacheRepository;
        }

        public async Task<Cart?> GetCartAsync(Guid userId)
        {
            return await _cartCacheRepository.GetCartAsync(userId);
        }

        public async Task<bool> AddItemToCartAsync(Guid userId, CartItem item)
        {
            return await _cartCacheRepository.AddItemToCartAsync(userId, item);
        }

        public async Task<bool> UpdateItemQuantityAsync(Guid userId, Guid productId, int quantity)
        {
            return await _cartCacheRepository.UpdateItemQuantityAsync(userId, productId, quantity);
        }

        public async Task<bool> RemoveItemFromCartAsync(Guid userId, Guid productId)
        {
            return await _cartCacheRepository.RemoveItemFromCartAsync(userId, productId);
        }

        public async Task<bool> ClearCartAsync(Guid userId)
        {
            return await _cartCacheRepository.ClearCartAsync(userId);
        }
    }
}
