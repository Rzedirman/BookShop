// Areas/Customer/Controllers/FavoritesController.cs
// Controller for managing customer's favorite books

using BookShop.Interfaces;
using BookShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BookShop.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "user")]
    public class FavoritesController : Controller
    {
        private readonly IFavoriteService _favoriteService;
        private readonly myShopContext _context;
        private readonly ILogger<FavoritesController> _logger;

        public FavoritesController(
            IFavoriteService favoriteService,
            myShopContext context,
            ILogger<FavoritesController> logger)
        {
            _favoriteService = favoriteService;
            _context = context;
            _logger = logger;
        }

        // GET: Customer/Favorites
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Loading customer favorites");

                // Get current user
                var currentUserEmail = User.Identity.Name;
                var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == currentUserEmail);

                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }

                // Get favorite books
                var favoriteBooks = await _favoriteService.GetUserFavoritesAsync(currentUser.UserId);

                return View(favoriteBooks.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading customer favorites");
                TempData["ErrorMessage"] = "An error occurred while loading your favorites.";
                return RedirectToAction("Index", "Home", new { area = "" });
            }
        }

        // POST: Customer/Favorites/Add/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int id)
        {
            try
            {
                _logger.LogInformation($"Adding product ID: {id} to favorites");

                // Get current user
                var currentUserEmail = User.Identity.Name;
                var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == currentUserEmail);

                if (currentUser == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                // Add to favorites
                var success = await _favoriteService.AddToFavoritesAsync(currentUser.UserId, id);

                if (success)
                {
                    _logger.LogInformation($"Product ID: {id} added to favorites for user ID: {currentUser.UserId}");
                    return Json(new { success = true, message = "Added to favorites" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to add to favorites" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding product ID: {id} to favorites");
                return Json(new { success = false, message = "An error occurred" });
            }
        }

        // POST: Customer/Favorites/Remove/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id)
        {
            try
            {
                _logger.LogInformation($"Removing product ID: {id} from favorites");

                // Get current user
                var currentUserEmail = User.Identity.Name;
                var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == currentUserEmail);

                if (currentUser == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                // Remove from favorites
                var success = await _favoriteService.RemoveFromFavoritesAsync(currentUser.UserId, id);

                if (success)
                {
                    _logger.LogInformation($"Product ID: {id} removed from favorites for user ID: {currentUser.UserId}");
                    return Json(new { success = true, message = "Removed from favorites" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to remove from favorites" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing product ID: {id} from favorites");
                return Json(new { success = false, message = "An error occurred" });
            }
        }

        // POST: Customer/Favorites/Toggle/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int id)
        {
            try
            {
                _logger.LogInformation($"Toggling favorite status for product ID: {id}");

                // Get current user
                var currentUserEmail = User.Identity.Name;
                var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == currentUserEmail);

                if (currentUser == null)
                {
                    return Json(new { success = false, message = "User not authenticated", isFavorite = false });
                }

                // Toggle favorite status
                var wasAdded = await _favoriteService.ToggleFavoriteAsync(currentUser.UserId, id);

                if (wasAdded)
                {
                    _logger.LogInformation($"Product ID: {id} added to favorites for user ID: {currentUser.UserId}");
                    return Json(new { success = true, message = "Added to favorites", isFavorite = true });
                }
                else
                {
                    _logger.LogInformation($"Product ID: {id} removed from favorites for user ID: {currentUser.UserId}");
                    return Json(new { success = true, message = "Removed from favorites", isFavorite = false });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error toggling favorite status for product ID: {id}");
                return Json(new { success = false, message = "An error occurred", isFavorite = false });
            }
        }
    }
}