// Areas/Seller/Controllers/DashboardController.cs
// Main dashboard controller for sellers

using BookShop.Models;
using BookShop.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BookShop.Areas.Seller.Controllers
{
    [Area("Seller")]
    [Authorize(Roles = "seller")]
    public class DashboardController : Controller
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly myShopContext _context;

        public DashboardController(
            ILogger<DashboardController> logger,
            myShopContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Loading seller dashboard");

            // Get current seller's user ID
            var currentUserEmail = User.Identity.Name;
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == currentUserEmail);

            if (currentUser == null)
            {
                return RedirectToAction("Login", "Home", new { area = "" });
            }

            // Get seller's books
            var sellerBooks = await _context.Products
                .Where(p => p.SellerId == currentUser.UserId)
                .Include(p => p.Orders)
                .Include(p => p.Author)
                .Include(p => p.Genre)
                .ToListAsync();

            // Get orders for seller's books
            var sellerOrders = await _context.Orders
                .Include(o => o.Product)
                .Include(o => o.User)
                .Where(o => o.Product.SellerId == currentUser.UserId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            // Calculate statistics
            var totalSalesAmount = sellerOrders.Sum(o => o.TotalPrice);
            var totalBooksSold = sellerOrders.Sum(o => o.Amount);

            // Create dashboard view model
            var dashboard = new SellerDashboardViewModel
            {
                TotalBooks = sellerBooks.Count,
                TotalBooksSold = totalBooksSold,
                TotalEarnings = totalSalesAmount,
                TotalOrders = sellerOrders.Count,
                RecentOrders = sellerOrders.Take(5).ToList(),
                TopSellingBooks = sellerBooks
                    .Where(b => b.Orders.Any())
                    .Select(b => new TopSellingBookViewModel
                    {
                        ProductId = b.ProductId,
                        Title = b.Title,
                        SalesCount = b.Orders.Sum(o => o.Amount),
                        TotalAmount = b.Orders.Sum(o => o.TotalPrice)
                    })
                    .OrderByDescending(b => b.SalesCount)
                    .Take(5)
                    .ToList(),
                CurrentUserBalance = currentUser.Balance
            };

            return View(dashboard);
        }

        public async Task<IActionResult> Account()
        {
            _logger.LogInformation("Loading seller account info");

            var currentUserEmail = User.Identity.Name;
            var currentUser = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == currentUserEmail);

            if (currentUser == null)
            {
                return RedirectToAction("Login", "Home", new { area = "" });
            }

            var viewModel = new SellerAccountViewModel
            {
                UserId = currentUser.UserId,
                Name = currentUser.Name,
                LastName = currentUser.LastName,
                Email = currentUser.Email,
                Phone = currentUser.Phone,
                BirthDate = currentUser.BirthDate,
                Balance = currentUser.Balance,
                CreatedDate = currentUser.CreatedDate,
                RoleName = currentUser.Role.RoleName
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAccount(SellerAccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _context.Users.FindAsync(model.UserId);

                if (currentUser == null || currentUser.Email != User.Identity.Name)
                {
                    return Forbid();
                }

                // Check if email already exists for a different user
                if (await _context.Users.AnyAsync(u => u.Email == model.Email && u.UserId != model.UserId))
                {
                    ModelState.AddModelError("Email", "This email is already registered");
                    return View("Account", model);
                }

                // Update user properties
                currentUser.Name = model.Name;
                currentUser.LastName = model.LastName;
                currentUser.Email = model.Email;
                currentUser.Phone = model.Phone;
                currentUser.BirthDate = model.BirthDate;

                _context.Update(currentUser);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Seller updated account info: {currentUser.UserId}");

                TempData["SuccessMessage"] = "Account information updated successfully";
                return RedirectToAction(nameof(Account));
            }

            return View("Account", model);
        }
    }
}