// Classes/CustomerService.cs
// Implementation of customer-related operations

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
    /// Service for managing customer profile, wallet, and library operations
    /// </summary>
    public class CustomerService : ICustomerService
    {
        private readonly myShopContext _context;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(myShopContext context, ILogger<CustomerService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Profile Operations

        public async Task<User> GetUserProfileAsync(int userId)
        {
            try
            {
                _logger.LogInformation($"Retrieving profile for user ID: {userId}");

                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                {
                    _logger.LogWarning($"User with ID {userId} not found");
                    return null;
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving profile for user ID: {userId}");
                throw;
            }
        }

        public async Task<bool> UpdateUserProfileAsync(User user)
        {
            try
            {
                _logger.LogInformation($"Updating profile for user ID: {user.UserId}");

                var existingUser = await _context.Users.FindAsync(user.UserId);
                if (existingUser == null)
                {
                    _logger.LogWarning($"User with ID {user.UserId} not found for update");
                    return false;
                }

                // Update only allowed fields
                existingUser.Name = user.Name;
                existingUser.LastName = user.LastName;
                existingUser.Email = user.Email;
                existingUser.Phone = user.Phone;
                existingUser.BirthDate = user.BirthDate;

                _context.Users.Update(existingUser);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Profile updated successfully for user ID: {user.UserId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating profile for user ID: {user.UserId}");
                throw;
            }
        }

        #endregion

        #region Wallet Operations

        public async Task<decimal> GetWalletBalanceAsync(int userId)
        {
            try
            {
                _logger.LogInformation($"Retrieving wallet balance for user ID: {userId}");

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning($"User with ID {userId} not found");
                    return 0;
                }

                return user.Balance;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving wallet balance for user ID: {userId}");
                throw;
            }
        }

        public async Task<bool> TopUpWalletAsync(int userId, decimal amount)
        {
            try
            {
                if (amount <= 0)
                {
                    _logger.LogWarning($"Invalid top-up amount: {amount} for user ID: {userId}");
                    return false;
                }

                _logger.LogInformation($"Processing top-up of ${amount} for user ID: {userId}");

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning($"User with ID {userId} not found for wallet top-up");
                    return false;
                }

                user.Balance += amount;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Wallet topped up successfully. New balance: ${user.Balance} for user ID: {userId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error topping up wallet for user ID: {userId}");
                throw;
            }
        }

        public async Task<bool> DeductFromWalletAsync(int userId, decimal amount)
        {
            try
            {
                if (amount <= 0)
                {
                    _logger.LogWarning($"Invalid deduction amount: {amount} for user ID: {userId}");
                    return false;
                }

                _logger.LogInformation($"Processing deduction of ${amount} for user ID: {userId}");

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning($"User with ID {userId} not found for wallet deduction");
                    return false;
                }

                // Check if user has sufficient balance
                if (user.Balance < amount)
                {
                    _logger.LogWarning($"Insufficient balance for user ID: {userId}. Balance: ${user.Balance}, Required: ${amount}");
                    return false;
                }

                user.Balance -= amount;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Amount deducted successfully. New balance: ${user.Balance} for user ID: {userId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deducting from wallet for user ID: {userId}");
                throw;
            }
        }

        #endregion

        #region Library Operations

        public async Task<IEnumerable<LibraryBookViewModel>> GetOwnedBooksAsync(int userId)
        {
            try
            {
                _logger.LogInformation($"Retrieving owned books for user ID: {userId}");

                var ownedBooks = await _context.Orders
                    .Where(o => o.UserId == userId)
                    .Include(o => o.Product)
                        .ThenInclude(p => p.Author)
                    .OrderByDescending(o => o.OrderDate)
                    .Select(o => new LibraryBookViewModel
                    {
                        ProductId = o.Product.ProductId,
                        Title = o.Product.Title,
                        AuthorName = o.Product.Author.Name + " " + o.Product.Author.LastName,
                        ImageName = o.Product.ImageName,
                        PurchaseDate = o.OrderDate,
                        OrderId = o.OrderId
                    })
                    .ToListAsync();

                _logger.LogInformation($"Found {ownedBooks.Count} owned books for user ID: {userId}");
                return ownedBooks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving owned books for user ID: {userId}");
                throw;
            }
        }

        public async Task<bool> HasUserPurchasedBookAsync(int userId, int productId)
        {
            try
            {
                _logger.LogInformation($"Checking if user ID: {userId} owns book ID: {productId}");

                var hasPurchased = await _context.Orders
                    .AnyAsync(o => o.UserId == userId && o.ProductId == productId);

                return hasPurchased;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking book ownership for user ID: {userId}, product ID: {productId}");
                throw;
            }
        }

        #endregion
    }
}