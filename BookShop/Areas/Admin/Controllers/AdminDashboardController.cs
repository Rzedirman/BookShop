// AdminDashboardController.cs
// Controller for the administration dashboard

using BookShop.Interfaces;
using BookShop.Models;
using BookShop.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BookShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class DashboardController : Controller
    {
        private readonly ILogger<DashboardController> _logger;
        //private readonly IBookService _bookService; - Commented for future implementation
        private readonly myShopContext _context;


        public DashboardController(
            ILogger<DashboardController> logger,
            //IBookService bookService,
            myShopContext context)
        {
            _logger = logger;
            //_bookService = bookService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Loading admin dashboard");

            var totalSalesAmount = await _context.Orders
            .Join(_context.Products,
            order => order.ProductId,
            product => product.ProductId,
            (order, product) => new {
                OrderAmount = order.Amount,
                ProductPrice = product.Price
            })
            .SumAsync(x => x.OrderAmount * x.ProductPrice);

            // Create a dashboard view model with statistics
            var dashboard = new DashboardViewModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalBooks = await _context.Products.CountAsync(),
                TotalOrders = await _context.Orders.CountAsync(),
                TotalSales = totalSalesAmount,
                RecentOrders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Product)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync(),
                    TopSellingBooks = await _context.Orders
                .GroupBy(o => o.ProductId)
                .Select(g => new TopSellingBookViewModel
                {
                    ProductId = g.Key,
                    Title = g.First().Product.Title,
                    SalesCount = g.Sum(o => o.Amount), // Sum of all books sold, not just order count
                    TotalAmount = g.Sum(o => o.Amount * o.Product.Price) // Multiply quantity by price
                })
                .OrderByDescending(b => b.SalesCount)
                .Take(5)
                .ToListAsync()
                };

            return View(dashboard);
        }

        public async Task<IActionResult> Users()
        {
            _logger.LogInformation("Loading user management");

            var users = await _context.Users
                .Include(u => u.Role)
                .ToListAsync();

            return View(users);
        }

        public async Task<IActionResult> Books()
        {
            _logger.LogInformation("Loading book management");

            var books = await _context.Products
                .Include(p => p.Author)
                .Include(p => p.Genre)
                .Include(p => p.Language)
                .ToListAsync();

            return View(books);
        }

        public async Task<IActionResult> Orders()
        {
            _logger.LogInformation("Loading order management");

            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> Reports()
        {
            _logger.LogInformation("Loading reports");

            // Monthly sales summary
            var monthlySales = await _context.Orders
            .Join(_context.Products,
            order => order.ProductId,
            product => product.ProductId,
            (order, product) => new {
                Order = order,
                Product = product
            })
            .GroupBy(x => new { Month = x.Order.OrderDate.Month, Year = x.Order.OrderDate.Year })
            .Select(g => new MonthlySalesViewModel
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                OrderCount = g.Count(),
                TotalAmount = g.Sum(x => x.Order.Amount * x.Product.Price)
            })
            .OrderByDescending(s => s.Year)
            .ThenByDescending(s => s.Month)
            .ToListAsync();

            var salesByGenre = await _context.Orders
                .Join(_context.Products,
                    order => order.ProductId,
                    product => product.ProductId,
                    (order, product) => new {
                        Order = order,
                        Product = product
                    })
                .GroupBy(x => x.Product.Genre.GenreName)
                .Select(g => new SalesByGenreViewModel
                {
                    GenreName = g.Key,
                    OrderCount = g.Sum(x => x.Order.Amount),
                    TotalAmount = g.Sum(x => x.Order.Amount * x.Product.Price)
                })
                .OrderByDescending(s => s.OrderCount)
                .ToListAsync();

            var viewModel = new ReportsViewModel
            {
                MonthlySales = monthlySales,
                SalesByGenre = salesByGenre
            };

            return View(viewModel);
        }

        // User management actions
        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            // TODO: Get user details for editing
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            ViewBag.Roles = await _context.Roles.ToListAsync();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUser(User user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = await _context.Users.FindAsync(user.UserId);
                    if (existingUser == null)
                    {
                        return NotFound();
                    }

                    // Update user properties
                    existingUser.Name = user.Name;
                    existingUser.LastName = user.LastName;
                    existingUser.Email = user.Email;
                    existingUser.Phone = user.Phone;
                    existingUser.BirthDate = user.BirthDate;
                    existingUser.RoleId = user.RoleId;

                    _context.Update(existingUser);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Users));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating user");
                    ModelState.AddModelError("", "Unable to save changes. Please try again.");
                }
            }

            ViewBag.Roles = await _context.Roles.ToListAsync();
            return View("EditUser", user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(UserPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FindAsync(model.UserId);
                if (user == null)
                {
                    return NotFound();
                }

                // Hash the new password
                string hashedPassword = String.Empty;
                using (var myHash = SHA256.Create())
                {
                    var byteArrayResultOfRawData = Encoding.UTF8.GetBytes(model.NewPassword);
                    var byteArrayResult = myHash.ComputeHash(byteArrayResultOfRawData);
                    hashedPassword = string.Concat(Array.ConvertAll(byteArrayResult, h => h.ToString("X2")));
                }

                // Update the user's password
                user.Password = hashedPassword;
                _context.Update(user);
                await _context.SaveChangesAsync();

                // Add a success message
                TempData["PasswordMessage"] = "Password has been changed successfully.";

                return RedirectToAction(nameof(EditUser), new { id = model.UserId });
            }

            // If we got this far, something failed; redisplay form
            var existingUser = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == model.UserId);

            ViewBag.Roles = await _context.Roles.ToListAsync();

            // Add an error message
            TempData["PasswordMessage"] = "Failed to change password. Please check your input.";

            return View("EditUser", existingUser);
        }




        // Book management actions
        [HttpGet]
        public async Task<IActionResult> EditBook(int id)
        {
            // TODO: Get book details for editing
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> UpdateBook(/* Book model */)
        {
            // TODO: Update book details
            return RedirectToAction(nameof(Books));
        }
    }
}
