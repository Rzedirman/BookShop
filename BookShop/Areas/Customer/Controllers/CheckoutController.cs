// Areas/Customer/Controllers/CheckoutController.cs
// Controller for checkout process and order completion

using BookShop.Interfaces;
using BookShop.Models;
using BookShop.ViewModels;
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
    public class CheckoutController : Controller
    {
        private readonly ICartService _cartService;
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly myShopContext _context;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(
            ICartService cartService,
            ICustomerService customerService,
            IOrderService orderService,
            myShopContext context,
            ILogger<CheckoutController> logger)
        {
            _cartService = cartService;
            _customerService = customerService;
            _orderService = orderService;
            _context = context;
            _logger = logger;
        }

        // GET: Customer/Checkout
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Loading checkout page");

                // Get current user
                var currentUserEmail = User.Identity.Name;
                var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == currentUserEmail);

                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }

                // Get cart
                var cart = await _cartService.GetUserCartAsync(currentUser.UserId);

                // Check if cart is empty
                if (cart.ItemCount == 0)
                {
                    TempData["ErrorMessage"] = "Your cart is empty.";
                    return RedirectToAction("Index", "Cart", new { area = "Customer" });
                }

                // Get wallet balance
                var walletBalance = await _customerService.GetWalletBalanceAsync(currentUser.UserId);

                // Create checkout view model
                var viewModel = new CheckoutViewModel
                {
                    Items = cart.Items,
                    TotalPrice = cart.TotalPrice,
                    WalletBalance = walletBalance
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading checkout page");
                TempData["ErrorMessage"] = "An error occurred while loading checkout.";
                return RedirectToAction("Index", "Cart", new { area = "Customer" });
            }
        }

        // POST: Customer/Checkout/Process
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Process()
        {
            try
            {
                _logger.LogInformation("Processing checkout");

                // Get current user
                var currentUserEmail = User.Identity.Name;
                var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == currentUserEmail);

                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }

                // Get cart items from database with full navigation properties
                var cartItems = await _context.Carts
                    .Where(c => c.UserId == currentUser.UserId)
                    .Include(c => c.Product)
                    .ToListAsync();

                // Check if cart is empty
                if (!cartItems.Any())
                {
                    TempData["ErrorMessage"] = "Your cart is empty.";
                    return RedirectToAction("Index", "Cart", new { area = "Customer" });
                }

                // Calculate total price
                var totalPrice = cartItems.Sum(c => c.Product.Price);

                // Get wallet balance
                var walletBalance = await _customerService.GetWalletBalanceAsync(currentUser.UserId);

                // Check if user has sufficient funds
                if (walletBalance < totalPrice)
                {
                    _logger.LogWarning($"Insufficient funds for user ID: {currentUser.UserId}. Balance: ${walletBalance}, Required: ${totalPrice}");
                    TempData["ErrorMessage"] = $"Insufficient funds. Your balance: ${walletBalance:F2}, Required: ${totalPrice:F2}";
                    return RedirectToAction(nameof(Index));
                }

                // Create orders for all cart items
                var orders = await _orderService.CreateBulkOrderAsync(currentUser.UserId, cartItems);

                if (orders == null || !orders.Any())
                {
                    _logger.LogError($"Failed to create orders for user ID: {currentUser.UserId}");
                    TempData["ErrorMessage"] = "Failed to process your order. Please try again.";
                    return RedirectToAction(nameof(Index));
                }

                // Deduct amount from wallet
                var deductSuccess = await _customerService.DeductFromWalletAsync(currentUser.UserId, totalPrice);

                if (!deductSuccess)
                {
                    _logger.LogError($"Failed to deduct from wallet for user ID: {currentUser.UserId}");

                    // Rollback: delete the created orders
                    _context.Orders.RemoveRange(orders);
                    await _context.SaveChangesAsync();

                    TempData["ErrorMessage"] = "Payment processing failed. Please try again.";
                    return RedirectToAction(nameof(Index));
                }

                // Clear cart
                await _cartService.ClearCartAsync(currentUser.UserId);

                _logger.LogInformation($"Order processed successfully for user ID: {currentUser.UserId}. Total: ${totalPrice}");

                // Redirect to success page with order details
                TempData["OrderIds"] = string.Join(",", orders.Select(o => o.OrderId));
                TempData["TotalPaid"] = totalPrice;

                return RedirectToAction(nameof(Success));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing checkout");
                TempData["ErrorMessage"] = "An error occurred while processing your order.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Customer/Checkout/Success
        public async Task<IActionResult> Success()
        {
            try
            {
                _logger.LogInformation("Loading order success page");

                // Get current user
                var currentUserEmail = User.Identity.Name;
                var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == currentUserEmail);

                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }

                // Get order IDs from TempData
                var orderIdsString = TempData["OrderIds"] as string;
                var totalPaid = TempData["TotalPaid"] as decimal?;

                if (string.IsNullOrEmpty(orderIdsString))
                {
                    // No order data found, redirect to library
                    return RedirectToAction("Index", "Library", new { area = "Customer" });
                }

                // Parse order IDs
                var orderIds = orderIdsString.Split(',').Select(int.Parse).ToList();

                // Get order details
                var orders = await _context.Orders
                    .Where(o => orderIds.Contains(o.OrderId))
                    .Include(o => o.Product)
                        .ThenInclude(p => p.Author)
                    .ToListAsync();

                // Get updated wallet balance
                var remainingBalance = await _customerService.GetWalletBalanceAsync(currentUser.UserId);

                // Create confirmation view model
                var viewModel = new OrderConfirmationViewModel
                {
                    Orders = orders,
                    TotalPaid = totalPaid ?? orders.Sum(o => o.TotalPrice),
                    OrderDate = orders.FirstOrDefault()?.OrderDate ?? DateTime.Now,
                    RemainingBalance = remainingBalance
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading order success page");
                TempData["SuccessMessage"] = "Your order was completed successfully!";
                return RedirectToAction("Index", "Library", new { area = "Customer" });
            }
        }
    }
}