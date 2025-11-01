// Areas/Customer/Controllers/CartController.cs
// Controller for managing shopping cart operations

using BookShop.Interfaces;
using BookShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BookShop.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "user")]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly myShopContext _context;
        private readonly ILogger<CartController> _logger;

        public CartController(
            ICartService cartService,
            myShopContext context,
            ILogger<CartController> logger)
        {
            _cartService = cartService;
            _context = context;
            _logger = logger;
        }

        // GET: Customer/Cart
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Loading shopping cart");

                // Get current user
                var currentUserEmail = User.Identity.Name;
                var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == currentUserEmail);

                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }

                // Get cart
                var cart = await _cartService.GetUserCartAsync(currentUser.UserId);

                return View(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading shopping cart");
                TempData["ErrorMessage"] = "An error occurred while loading your cart.";
                return RedirectToAction("Index", "Home", new { area = "" });
            }
        }

        // POST: Customer/Cart/Add/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int id)
        {
            try
            {
                _logger.LogInformation($"Adding product ID: {id} to cart");

                // Get current user
                var currentUserEmail = User.Identity.Name;
                var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == currentUserEmail);

                if (currentUser == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                // Add to cart
                var success = await _cartService.AddToCartAsync(currentUser.UserId, id);

                if (success)
                {
                    _logger.LogInformation($"Product ID: {id} added to cart for user ID: {currentUser.UserId}");

                    // Get updated cart count
                    var cartCount = await _cartService.GetCartItemCountAsync(currentUser.UserId);

                    return Json(new { success = true, message = "Added to cart", cartCount = cartCount });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to add to cart" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding product ID: {id} to cart");
                return Json(new { success = false, message = "An error occurred" });
            }
        }

        // POST: Customer/Cart/Remove/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id)
        {
            try
            {
                _logger.LogInformation($"Removing product ID: {id} from cart");

                // Get current user
                var currentUserEmail = User.Identity.Name;
                var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == currentUserEmail);

                if (currentUser == null)
                {
                    TempData["ErrorMessage"] = "User not authenticated";
                    return RedirectToAction(nameof(Index));
                }

                // Remove from cart
                var success = await _cartService.RemoveFromCartAsync(currentUser.UserId, id);

                if (success)
                {
                    _logger.LogInformation($"Product ID: {id} removed from cart for user ID: {currentUser.UserId}");
                    TempData["SuccessMessage"] = "Item removed from cart";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to remove item from cart";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing product ID: {id} from cart");
                TempData["ErrorMessage"] = "An error occurred";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Customer/Cart/Clear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clear()
        {
            try
            {
                _logger.LogInformation("Clearing shopping cart");

                // Get current user
                var currentUserEmail = User.Identity.Name;
                var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == currentUserEmail);

                if (currentUser == null)
                {
                    TempData["ErrorMessage"] = "User not authenticated";
                    return RedirectToAction(nameof(Index));
                }

                // Clear cart
                await _cartService.ClearCartAsync(currentUser.UserId);

                _logger.LogInformation($"Cart cleared for user ID: {currentUser.UserId}");
                TempData["SuccessMessage"] = "Cart cleared successfully";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing shopping cart");
                TempData["ErrorMessage"] = "An error occurred while clearing your cart";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}