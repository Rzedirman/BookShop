// Classes/CartService.cs
// Implementation of shopping cart operations

using BookShop.Interfaces;
using BookShop.Models;
using BookShop.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShop.Classes
{
    /// <summary>
    /// Service for managing shopping cart operations
    /// </summary>
    public class CartService : ICartService
    {
        private readonly myShopContext _context;
        private readonly ILogger<CartService> _logger;

        public CartService(myShopContext context, ILogger<CartService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<CartViewModel> GetUserCartAsync(int userId)
        {
            try
            {
                _logger.LogInformation($"Retrieving cart for user ID: {userId}");

                var cartItems = await _context.Carts
                    .Where(c => c.UserId == userId)
                    .Include(c => c.Product)
                        .ThenInclude(p => p.Author)
                    .Select(c => new CartItemViewModel
                    {
                        CartId = c.CartId,
                        ProductId = c.Product.ProductId,
                        Title = c.Product.Title,
                        AuthorName = c.Product.Author.Name + " " + c.Product.Author.LastName,
                        Price = c.Product.Price,
                        ImageName = c.Product.ImageName
                    })
                    .ToListAsync();

                var viewModel = new CartViewModel
                {
                    Items = cartItems,
                    TotalPrice = cartItems.Sum(item => item.Price),
                    ItemCount = cartItems.Count
                };

                _logger.LogInformation($"Cart retrieved with {viewModel.ItemCount} items for user ID: {userId}");
                return viewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving cart for user ID: {userId}");
                throw;
            }
        }

        public async Task<bool> AddToCartAsync(int userId, int productId)
        {
            try
            {
                _logger.LogInformation($"Adding product ID: {productId} to cart for user ID: {userId}");

                // Check if product exists
                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                {
                    _logger.LogWarning($"Product ID: {productId} not found");
                    return false;
                }

                // Check if product is in stock
                if (product.InStock <= 0)
                {
                    _logger.LogWarning($"Product ID: {productId} is out of stock");
                    return false;
                }

                // Check if item already exists in cart
                var existingCartItem = await _context.Carts
                    .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

                if (existingCartItem != null)
                {
                    _logger.LogInformation($"Product ID: {productId} already in cart for user ID: {userId}");
                    return true; // Already in cart, consider it a success
                }

                // Check if user already owns this book
                var alreadyOwned = await _context.Orders
                    .AnyAsync(o => o.UserId == userId && o.ProductId == productId);

                if (alreadyOwned)
                {
                    _logger.LogWarning($"User ID: {userId} already owns product ID: {productId}");
                    return false;
                }

                // Add new item to cart
                var cartItem = new Cart
                {
                    UserId = userId,
                    ProductId = productId,
                    Quantity = 1 // For digital books, quantity is always 1
                };

                _context.Carts.Add(cartItem);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Product ID: {productId} added to cart successfully for user ID: {userId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding product ID: {productId} to cart for user ID: {userId}");
                throw;
            }
        }

        public async Task<bool> RemoveFromCartAsync(int userId, int productId)
        {
            try
            {
                _logger.LogInformation($"Removing product ID: {productId} from cart for user ID: {userId}");

                var cartItem = await _context.Carts
                    .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

                if (cartItem == null)
                {
                    _logger.LogWarning($"Cart item not found for product ID: {productId} and user ID: {userId}");
                    return false;
                }

                _context.Carts.Remove(cartItem);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Product ID: {productId} removed from cart successfully for user ID: {userId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing product ID: {productId} from cart for user ID: {userId}");
                throw;
            }
        }

        public async Task<decimal> GetCartTotalAsync(int userId)
        {
            try
            {
                _logger.LogInformation($"Calculating cart total for user ID: {userId}");

                var total = await _context.Carts
                    .Where(c => c.UserId == userId)
                    .Include(c => c.Product)
                    .SumAsync(c => c.Product.Price);

                return total;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calculating cart total for user ID: {userId}");
                throw;
            }
        }

        public async Task<int> GetCartItemCountAsync(int userId)
        {
            try
            {
                _logger.LogInformation($"Getting cart item count for user ID: {userId}");

                var count = await _context.Carts
                    .Where(c => c.UserId == userId)
                    .CountAsync();

                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting cart item count for user ID: {userId}");
                throw;
            }
        }

        public async Task ClearCartAsync(int userId)
        {
            try
            {
                _logger.LogInformation($"Clearing cart for user ID: {userId}");

                var cartItems = await _context.Carts
                    .Where(c => c.UserId == userId)
                    .ToListAsync();

                if (cartItems.Any())
                {
                    _context.Carts.RemoveRange(cartItems);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Cart cleared successfully for user ID: {userId}. Removed {cartItems.Count} items");
                }
                else
                {
                    _logger.LogInformation($"Cart was already empty for user ID: {userId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error clearing cart for user ID: {userId}");
                throw;
            }
        }
    }
}