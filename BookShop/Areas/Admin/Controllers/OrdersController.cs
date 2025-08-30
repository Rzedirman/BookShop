using System;
using System.Linq;
using System.Threading.Tasks;
using BookShop.Helpers;
using BookShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class OrdersController : Controller
    {
        private readonly myShopContext _context;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(myShopContext context, ILogger<OrdersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/Orders
        public async Task<IActionResult> Index(
            string sortOrder,
            string currentFilter,
            string searchString,
            DateTime? startDate,
            DateTime? endDate,
            decimal? minPrice,
            decimal? maxPrice,
            int? pageNumber)
        {
            _logger.LogInformation("Loading admin orders management");

            // Set up sorting parameters
            ViewData["CurrentSort"] = sortOrder;
            ViewData["OrderIdSortParam"] = String.IsNullOrEmpty(sortOrder) ? "orderid_desc" : "";
            ViewData["UserSortParam"] = sortOrder == "user" ? "user_desc" : "user";
            ViewData["ProductSortParam"] = sortOrder == "product" ? "product_desc" : "product";
            ViewData["DateSortParam"] = sortOrder == "date" ? "date_desc" : "date";
            ViewData["AmountSortParam"] = sortOrder == "amount" ? "amount_desc" : "amount";
            ViewData["PriceSortParam"] = sortOrder == "price" ? "price_desc" : "price";

            // Handle search string
            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;
            ViewData["StartDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = endDate?.ToString("yyyy-MM-dd");
            ViewData["MinPrice"] = minPrice;
            ViewData["MaxPrice"] = maxPrice;

            var orders = _context.Orders
                .Include(o => o.User)
                .Include(o => o.Product)
                .ThenInclude(p => p.Author)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                orders = orders.Where(o =>
                    o.OrderId.ToString().Contains(searchString) ||
                    o.User.Email.Contains(searchString) ||
                    o.User.Name.Contains(searchString) ||
                    o.User.LastName.Contains(searchString) ||
                    o.Product.Title.Contains(searchString) ||
                    o.Product.Author.Name.Contains(searchString) ||
                    o.Product.Author.LastName.Contains(searchString) ||
                    o.DeliveryAddress.Contains(searchString));
            }

            // Apply date range filter
            if (startDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate <= endDate.Value.AddDays(1)); // Include the entire end date
            }

            // Apply price range filter
            if (minPrice.HasValue)
            {
                orders = orders.Where(o => o.TotalPrice >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                orders = orders.Where(o => o.TotalPrice <= maxPrice.Value);
            }

            // Apply sorting
            switch (sortOrder)
            {
                case "orderid_desc":
                    orders = orders.OrderByDescending(o => o.OrderId);
                    break;
                case "user":
                    orders = orders.OrderBy(o => o.User.Email);
                    break;
                case "user_desc":
                    orders = orders.OrderByDescending(o => o.User.Email);
                    break;
                case "product":
                    orders = orders.OrderBy(o => o.Product.Title);
                    break;
                case "product_desc":
                    orders = orders.OrderByDescending(o => o.Product.Title);
                    break;
                case "date":
                    orders = orders.OrderBy(o => o.OrderDate);
                    break;
                case "date_desc":
                    orders = orders.OrderByDescending(o => o.OrderDate);
                    break;
                case "amount":
                    orders = orders.OrderBy(o => o.Amount);
                    break;
                case "amount_desc":
                    orders = orders.OrderByDescending(o => o.Amount);
                    break;
                case "price":
                    orders = orders.OrderBy(o => o.TotalPrice);
                    break;
                case "price_desc":
                    orders = orders.OrderByDescending(o => o.TotalPrice);
                    break;
                default:
                    orders = orders.OrderBy(o => o.OrderId); // Default sort by OrderId ascending
                    break;
            }

            int pageSize = 15;
            return View(await PaginatedList<Order>.CreateAsync(orders.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Admin/Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Product)
                .ThenInclude(p => p.Author)
                .Include(o => o.Product.Genre)
                .Include(o => o.Product.Language)
                .FirstOrDefaultAsync(m => m.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Admin/Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Product)
                .ThenInclude(p => p.Author)
                .FirstOrDefaultAsync(m => m.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Admin/Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            try
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Deleted order ID {id}");

                TempData["SuccessMessage"] = "Order deleted successfully";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting order ID {id}");
                TempData["ErrorMessage"] = "An error occurred while deleting the order";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }
    }
}