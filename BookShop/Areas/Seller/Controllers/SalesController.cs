// Areas/Seller/Controllers/SalesController.cs
// Sales analytics and reporting controller for sellers - UPDATED VERSION

using BookShop.Helpers;
using BookShop.Models;
using BookShop.ViewModels; // Make sure to use ViewModels namespace
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
    public class SalesController : Controller
    {
        private readonly myShopContext _context;
        private readonly ILogger<SalesController> _logger;

        public SalesController(myShopContext context, ILogger<SalesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Seller/Sales - Main sales dashboard
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Loading seller sales dashboard");

            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Home", new { area = "" });
            }

            // Get all seller's orders
            var sellerOrders = await _context.Orders
                .Include(o => o.Product)
                .Include(o => o.User)
                .Where(o => o.Product.SellerId == currentUser.UserId)
                .ToListAsync();

            // Calculate monthly sales for the last 12 months
            var monthlySales = sellerOrders
                .Where(o => o.OrderDate >= DateTime.Now.AddMonths(-12))
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new SellerMonthlySalesViewModel
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    BooksSold = g.Sum(o => o.Amount),
                    TotalEarnings = g.Sum(o => o.TotalPrice),
                    UniqueCustomers = g.Select(o => o.UserId).Distinct().Count()
                })
                .OrderBy(s => s.Year).ThenBy(s => s.Month)
                .ToList();

            // Calculate growth percentage
            var thisMonth = DateTime.Now;
            var lastMonth = thisMonth.AddMonths(-1);

            var thisMonthSales = sellerOrders
                .Where(o => o.OrderDate.Year == thisMonth.Year && o.OrderDate.Month == thisMonth.Month)
                .Sum(o => o.TotalPrice);

            var lastMonthSales = sellerOrders
                .Where(o => o.OrderDate.Year == lastMonth.Year && o.OrderDate.Month == lastMonth.Month)
                .Sum(o => o.TotalPrice);

            var growthPercentage = lastMonthSales > 0 ? ((thisMonthSales - lastMonthSales) / lastMonthSales) * 100 : 0;

            var viewModel = new SellerSalesOverviewViewModel
            {
                TotalEarnings = sellerOrders.Sum(o => o.TotalPrice),
                TotalBooksSold = sellerOrders.Sum(o => o.Amount),
                TotalOrders = sellerOrders.Count,
                UniqueCustomers = sellerOrders.Select(o => o.UserId).Distinct().Count(),
                AverageOrderValue = sellerOrders.Any() ? sellerOrders.Average(o => o.TotalPrice) : 0,
                ThisMonthSales = thisMonthSales,
                LastMonthSales = lastMonthSales,
                SalesGrowthPercentage = growthPercentage,
                MonthlySalesData = monthlySales,
                TopSellingBooks = await GetTopSellingBooks(currentUser.UserId),
                RecentOrders = sellerOrders.OrderByDescending(o => o.OrderDate).Take(5).ToList()
            };

            return View(viewModel);
        }

        // GET: Seller/Sales/Orders - List all orders for seller's books
        public async Task<IActionResult> Orders(string sortOrder, string currentFilter, string searchString,
            DateTime? startDate, DateTime? endDate, int? pageNumber)
        {
            _logger.LogInformation("Loading seller orders management");

            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Home", new { area = "" });
            }

            ViewData["CurrentSort"] = sortOrder;
            ViewData["OrderIdSortParam"] = String.IsNullOrEmpty(sortOrder) ? "orderid_desc" : "";
            ViewData["DateSortParam"] = sortOrder == "date" ? "date_desc" : "date";
            ViewData["AmountSortParam"] = sortOrder == "amount" ? "amount_desc" : "amount";
            ViewData["BookSortParam"] = sortOrder == "book" ? "book_desc" : "book";

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

            var orders = _context.Orders
                .Include(o => o.Product)
                .Include(o => o.User)
                .Where(o => o.Product.SellerId == currentUser.UserId)
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
                    o.DeliveryAddress.Contains(searchString));
            }

            // Apply date filters
            if (startDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate <= endDate.Value.AddDays(1));
            }

            // Apply sorting
            switch (sortOrder)
            {
                case "orderid_desc":
                    orders = orders.OrderByDescending(o => o.OrderId);
                    break;
                case "date":
                    orders = orders.OrderBy(o => o.OrderDate);
                    break;
                case "date_desc":
                    orders = orders.OrderByDescending(o => o.OrderDate);
                    break;
                case "amount":
                    orders = orders.OrderBy(o => o.TotalPrice);
                    break;
                case "amount_desc":
                    orders = orders.OrderByDescending(o => o.TotalPrice);
                    break;
                case "book":
                    orders = orders.OrderBy(o => o.Product.Title);
                    break;
                case "book_desc":
                    orders = orders.OrderByDescending(o => o.Product.Title);
                    break;
                default:
                    orders = orders.OrderBy(o => o.OrderId);
                    break;
            }

            int pageSize = 15;
            return View(await PaginatedList<Order>.CreateAsync(orders.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Seller/Sales/BookAnalytics/5 - Detailed analytics for a specific book
        //public async Task<IActionResult> BookAnalytics(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var currentUser = await GetCurrentUserAsync();
        //    if (currentUser == null)
        //    {
        //        return RedirectToAction("Login", "Home", new { area = "" });
        //    }

        //    var book = await _context.Products
        //        .Include(p => p.Author)
        //        .Include(p => p.Genre)
        //        .Include(p => p.Language)
        //        .Include(p => p.Orders)
        //        .ThenInclude(o => o.User)
        //        .FirstOrDefaultAsync(p => p.ProductId == id && p.SellerId == currentUser.UserId);

        //    if (book == null)
        //    {
        //        return NotFound();
        //    }

        //    var orders = book.Orders.ToList();
        //    var totalSales = orders.Sum(o => o.Amount);
        //    var totalRevenue = orders.Sum(o => o.TotalPrice);
        //    var uniqueCustomers = orders.Select(o => o.UserId).Distinct().Count();
        //    var averageOrderSize = orders.Any() ? (decimal)totalSales / orders.Count : 0;
        //    var averageOrderValue = orders.Any() ? totalRevenue / orders.Count : 0;

        //    // Monthly performance for the last 12 months
        //    var monthlyPerformance = orders
        //        .Where(o => o.OrderDate >= DateTime.Now.AddMonths(-12))
        //        .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
        //        .Select(g => new SellerMonthlySalesViewModel
        //        {
        //            Year = g.Key.Year,
        //            Month = g.Key.Month,
        //            BooksSold = g.Sum(o => o.Amount),
        //            TotalEarnings = g.Sum(o => o.TotalPrice),
        //            UniqueCustomers = g.Select(o => o.UserId).Distinct().Count()
        //        })
        //        .OrderBy(s => s.Year).ThenBy(s => s.Month)
        //        .ToList();

        //    // Customer analysis
        //    var topCustomers = orders
        //        .GroupBy(o => o.User)
        //        .Select(g => new
        //        {
        //            Customer = g.Key,
        //            OrderCount = g.Count(),
        //            TotalSpent = g.Sum(o => o.TotalPrice),
        //            BooksPurchased = g.Sum(o => o.Amount),
        //            FirstPurchase = g.Min(o => o.OrderDate),
        //            LastPurchase = g.Max(o => o.OrderDate)
        //        })
        //        .OrderByDescending(c => c.TotalSpent)
        //        .Take(10)
        //        .ToList();

        //    var viewModel = new BookAnalyticsViewModel
        //    {
        //        Book = book,
        //        TotalSales = totalSales,
        //        TotalRevenue = totalRevenue,
        //        TotalOrders = orders.Count,
        //        UniqueCustomers = uniqueCustomers,
        //        AverageOrderSize = averageOrderSize,
        //        AverageOrderValue = averageOrderValue,
        //        MonthlyPerformance = monthlyPerformance,
        //        RecentOrders = orders
        //            .OrderByDescending(o => o.OrderDate)
        //            .Take(15)
        //            .ToList(),
        //        TopCustomers = topCustomers
        //    };

        //    return View(viewModel);
        //}

        // GET: Seller/Sales/Reports - Generate and export reports
        public async Task<IActionResult> Reports(DateTime? startDate, DateTime? endDate, string reportType = "summary")
        {
            _logger.LogInformation("Loading seller sales reports");

            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Home", new { area = "" });
            }

            // Default date range (last 3 months)
            if (!startDate.HasValue) startDate = DateTime.Now.AddMonths(-3);
            if (!endDate.HasValue) endDate = DateTime.Now;

            ViewData["StartDate"] = startDate.Value.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = endDate.Value.ToString("yyyy-MM-dd");
            ViewData["ReportType"] = reportType;

            // Get orders for the period
            var orders = await _context.Orders
                .Include(o => o.Product)
                .ThenInclude(p => p.Author)
                .Include(o => o.Product)
                .ThenInclude(p => p.Genre)
                .Include(o => o.User)
                .Where(o => o.Product.SellerId == currentUser.UserId &&
                           o.OrderDate >= startDate.Value &&
                           o.OrderDate <= endDate.Value.AddDays(1))
                .ToListAsync();

            var reportViewModel = new SalesReportViewModel
            {
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                ReportType = reportType,
                TotalOrders = orders.Count,
                TotalRevenue = orders.Sum(o => o.TotalPrice),
                TotalBooksSold = orders.Sum(o => o.Amount),
                UniqueCustomers = orders.Select(o => o.UserId).Distinct().Count(),

                // Daily sales breakdown
                DailySales = orders
                    .GroupBy(o => o.OrderDate.Date)
                    .Select(g => new DailySalesData
                    {
                        Date = g.Key,
                        OrderCount = g.Count(),
                        Revenue = g.Sum(o => o.TotalPrice),
                        BooksSold = g.Sum(o => o.Amount)
                    })
                    .OrderBy(d => d.Date)
                    .ToList(),

                // Top books in period
                TopBooks = orders
                    .GroupBy(o => o.Product)
                    .Select(g => new BookPerformanceData
                    {
                        Book = g.Key,
                        OrderCount = g.Count(),
                        BooksSold = g.Sum(o => o.Amount),
                        Revenue = g.Sum(o => o.TotalPrice)
                    })
                    .OrderByDescending(b => b.Revenue)
                    .ToList(),

                // Genre performance
                GenrePerformance = orders
                    .GroupBy(o => o.Product.Genre.GenreName)
                    .Select(g => new GenrePerformanceData
                    {
                        GenreName = g.Key,
                        OrderCount = g.Count(),
                        BooksSold = g.Sum(o => o.Amount),
                        Revenue = g.Sum(o => o.TotalPrice)
                    })
                    .OrderByDescending(g => g.Revenue)
                    .ToList()
            };

            return View(reportViewModel);
        }

        #region Private Helper Methods

        private async Task<User> GetCurrentUserAsync()
        {
            var currentUserEmail = User.Identity.Name;
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == currentUserEmail);
        }

        private async Task<List<TopSellingBookViewModel>> GetTopSellingBooks(int sellerId)
        {
            return await _context.Products
                .Where(p => p.SellerId == sellerId)
                .Include(p => p.Orders)
                .Where(p => p.Orders.Any())
                .Select(p => new TopSellingBookViewModel
                {
                    ProductId = p.ProductId,
                    Title = p.Title,
                    SalesCount = p.Orders.Sum(o => o.Amount),
                    TotalAmount = p.Orders.Sum(o => o.TotalPrice)
                })
                .OrderByDescending(b => b.SalesCount)
                .Take(5)
                .ToListAsync();
        }

        #endregion
    }
}