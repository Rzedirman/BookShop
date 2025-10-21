// Interfaces/ICartService.cs
// Interface for shopping cart operations

using BookShop.ViewModels;
using System.Threading.Tasks;

namespace BookShop.Interfaces
{
    /// <summary>
    /// Service interface for shopping cart operations
    /// </summary>
    public interface ICartService
    {
        /// <summary>
        /// Retrieves the user's shopping cart with all items
        /// </summary>
        Task<CartViewModel> GetUserCartAsync(int userId);

        /// <summary>
        /// Adds a product to the user's cart
        /// </summary>
        Task<bool> AddToCartAsync(int userId, int productId);

        /// <summary>
        /// Removes a specific product from the user's cart
        /// </summary>
        Task<bool> RemoveFromCartAsync(int userId, int productId);

        /// <summary>
        /// Calculates the total price of all items in the cart
        /// </summary>
        Task<decimal> GetCartTotalAsync(int userId);

        /// <summary>
        /// Gets the number of items in the user's cart
        /// </summary>
        Task<int> GetCartItemCountAsync(int userId);

        /// <summary>
        /// Removes all items from the user's cart
        /// </summary>
        Task ClearCartAsync(int userId);
    }
}