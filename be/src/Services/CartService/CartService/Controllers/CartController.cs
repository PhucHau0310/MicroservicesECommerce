using CartService.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CartService.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly CartService.Application.Services.CartService _cartService;

        public CartController(CartService.Application.Services.CartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet("detail")]
        public async Task<IActionResult> GetCart(Guid userId)
        {
            var cart = await _cartService.GetCartAsync(userId);
            return cart != null ? Ok(cart) : NotFound();
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddTocCart(Guid userId, [FromBody] CartItem cartItem)
        {
            try
            {
                var result = await _cartService.AddItemToCartAsync(userId, cartItem);
                return result ? Ok() : BadRequest();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateQuantity(Guid productId, Guid userId, int quantityReq)
        {
            try
            {
                var result = await _cartService.UpdateItemQuantityAsync(userId, productId, quantityReq);
                return result ? Ok() : NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("delete/productItem")]
        public async Task<IActionResult> RemoveItem(Guid productId, Guid userId)
        {
            var result = await _cartService.RemoveItemFromCartAsync(userId, productId);
            return result ? Ok() : NotFound();
        }


        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart(Guid userId)
        {
            var result = await _cartService.ClearCartAsync(userId);
            return result ? Ok() : BadRequest();
        }
    }
}
