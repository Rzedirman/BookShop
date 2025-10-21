// Classes/FavoriteService.cs
// Implementation of favorites management operations

using BookShop.Interfaces;
using BookShop.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShop.Classes
{
    /// <summary>
    /// Service for managing user's favorite books
    /// </summary>
    public class FavoriteService : IFavoriteService
    {
        private readonly myShopContext _context;
        private readonly ILogger<FavoriteService> _logger;

        public FavoriteService(myShopContext context, ILogger<FavoriteService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Product>> GetUserFavoritesAsync(int userId)
        {
            try
            {
                _logger.LogInformation($"Retrieving favorites for user ID: {userId}");

                var favorites = await _context.Favorites
                    .Where(f => f.UserId == userId)
                    .Include(f => f.Product)
                        .ThenInclude(p => p.Author)
                    .Include(f => f.Product)
                        .ThenInclude(p => p.Genre)
                    .Include(f => f.Product)
                        .ThenInclude(p => p.Language)
                    .Select(f => f.Product)
                    .ToListAsync();

                _logger.LogInformation($"Found {favorites.Count} favorites for user ID: {userId}");
                return favorites;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving favorites for user ID: {userId}");
                throw;
            }
        }

        public async Task<bool> AddToFavoritesAsync(int userId, int productId)
        {
            try
            {
                _logger.LogInformation($"Adding product ID: {productId} to favorites for user ID: {userId}");

                // Check if product exists
                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                {
                    _logger.LogWarning($"Product ID: {productId} not found");
                    return false;
                }

                // Check if already in favorites
                var existingFavorite = await _context.Favorites
                    .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);

                if (existingFavorite != null)
                {
                    _logger.LogInformation($"Product ID: {productId} already in favorites for user ID: {userId}");
                    return true; // Already in favorites, consider it a success
                }

                // Add to favorites
                var favorite = new Favorite
                {
                    UserId = userId,
                    ProductId = productId,
                    AddedDate = DateTime.Now
                };

                _context.Favorites.Add(favorite);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Product ID: {productId} added to favorites successfully for user ID: {userId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding product ID: {productId} to favorites for user ID: {userId}");
                throw;
            }
        }

        public async Task<bool> RemoveFromFavoritesAsync(int userId, int productId)
        {
            try
            {
                _logger.LogInformation($"Removing product ID: {productId} from favorites for user ID: {userId}");

                var favorite = await _context.Favorites
                    .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);

                if (favorite == null)
                {
                    _logger.LogWarning($"Favorite not found for product ID: {productId} and user ID: {userId}");
                    return false;
                }

                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Product ID: {productId} removed from favorites successfully for user ID: {userId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing product ID: {productId} from favorites for user ID: {userId}");
                throw;
            }
        }

        public async Task<bool> ToggleFavoriteAsync(int userId, int productId)
        {
            try
            {
                _logger.LogInformation($"Toggling favorite status for product ID: {productId} and user ID: {userId}");

                var favorite = await _context.Favorites
                    .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);

                if (favorite != null)
                {
                    // Remove from favorites
                    _context.Favorites.Remove(favorite);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Product ID: {productId} removed from favorites for user ID: {userId}");
                    return false; // Return false to indicate it was removed
                }
                else
                {
                    // Check if product exists before adding
                    var product = await _context.Products.FindAsync(productId);
                    if (product == null)
                    {
                        _logger.LogWarning($"Product ID: {productId} not found");
                        return false;
                    }

                    // Add to favorites
                    var newFavorite = new Favorite
                    {
                        UserId = userId,
                        ProductId = productId,
                        AddedDate = DateTime.Now
                    };

                    _context.Favorites.Add(newFavorite);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Product ID: {productId} added to favorites for user ID: {userId}");
                    return true; // Return true to indicate it was added
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error toggling favorite for product ID: {productId} and user ID: {userId}");
                throw;
            }
        }

        public async Task<bool> IsFavoriteAsync(int userId, int productId)
        {
            try
            {
                _logger.LogInformation($"Checking if product ID: {productId} is favorite for user ID: {userId}");

                var isFavorite = await _context.Favorites
                    .AnyAsync(f => f.UserId == userId && f.ProductId == productId);

                return isFavorite;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking favorite status for product ID: {productId} and user ID: {userId}");
                throw;
            }
        }
    }
}