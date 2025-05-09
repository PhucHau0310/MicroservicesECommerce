using CartService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartService.Domain.Interfaces
{
    public interface ICartCacheRepository
    {
        IEnumerable<string> GetAllCartKeys();
        Task<Cart?> GetCartAsync(Guid userId);
        Task<bool> AddItemToCartAsync(Guid userId, CartItem item);
        Task<bool> UpdateItemQuantityAsync(Guid userId, Guid productId, int quantity);
        Task<bool> RemoveItemFromCartAsync(Guid userId, Guid productId);
        Task<bool> ClearCartAsync(Guid userId);
    }
}
