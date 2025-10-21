// Interfaces/IFavoriteService.cs
// Interface for favorites management operations

using BookShop.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookShop.Interfaces
{
    /// <summary>
    /// Service interface for managing user's favorite books
    /// </summary>
    public interface IFavoriteService
    {
        /// <summary>
        /// Retrieves all favorite books for a user
        /// </summary>
        Task<IEnumerable<Product>> GetUserFavoritesAsync(int userId);

        /// <summary>
        /// Adds a book to user's favorites
        /// </summary>
        Task<bool> AddToFavoritesAsync(int userId, int productId);

        /// <summary>
        /// Removes a book from user's favorites
        /// </summary>
        Task<bool> RemoveFromFavoritesAsync(int userId, int productId);

        /// <summary>
        /// Toggles favorite status (add if not exists, remove if exists)
        /// </summary>
        Task<bool> ToggleFavoriteAsync(int userId, int productId);

        /// <summary>
        /// Checks if a book is in user's favorites
        /// </summary>
        Task<bool> IsFavoriteAsync(int userId, int productId);
    }
}