// Interfaces/ICustomerService.cs
// Interface for customer-related operations including profile, wallet, and library management

using BookShop.Models;
using BookShop.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookShop.Interfaces
{
    /// <summary>
    /// Service interface for customer profile, wallet, and library operations
    /// </summary>
    public interface ICustomerService
    {
        // Profile Operations
        /// <summary>
        /// Retrieves user profile information by user ID
        /// </summary>
        Task<User> GetUserProfileAsync(int userId);

        /// <summary>
        /// Updates user profile information
        /// </summary>
        Task<bool> UpdateUserProfileAsync(User user);

        // Wallet Operations
        /// <summary>
        /// Gets the current wallet balance for a user
        /// </summary>
        Task<decimal> GetWalletBalanceAsync(int userId);

        /// <summary>
        /// Adds funds to user's wallet
        /// </summary>
        Task<bool> TopUpWalletAsync(int userId, decimal amount);

        /// <summary>
        /// Deducts funds from user's wallet (used during checkout)
        /// </summary>
        Task<bool> DeductFromWalletAsync(int userId, decimal amount);

        // Library Operations
        /// <summary>
        /// Retrieves all books owned by a user with purchase information
        /// </summary>
        Task<IEnumerable<LibraryBookViewModel>> GetOwnedBooksAsync(int userId);

        /// <summary>
        /// Checks if a user has already purchased a specific book
        /// </summary>
        Task<bool> HasUserPurchasedBookAsync(int userId, int productId);
    }
}
