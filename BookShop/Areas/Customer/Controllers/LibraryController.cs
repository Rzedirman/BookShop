// Areas/Customer/Controllers/LibraryController.cs
// Controller for managing customer's owned books library

using BookShop.Interfaces;
using BookShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BookShop.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "user")]
    public class LibraryController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly IFileStorageService _fileStorageService;
        private readonly myShopContext _context;
        private readonly ILogger<LibraryController> _logger;

        public LibraryController(
            ICustomerService customerService,
            IFileStorageService fileStorageService,
            myShopContext context,
            ILogger<LibraryController> logger)
        {
            _customerService = customerService;
            _fileStorageService = fileStorageService;
            _context = context;
            _logger = logger;
        }

        // GET: Customer/Library
        public async Task<IActionResult> Index(string sortOrder)
        {
            try
            {
                _logger.LogInformation("Loading customer library");

                // Get current user
                var currentUserEmail = User.Identity.Name;
                var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == currentUserEmail);

                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }

                // Get owned books
                var ownedBooks = await _customerService.GetOwnedBooksAsync(currentUser.UserId);

                // Apply sorting
                ViewData["CurrentSort"] = sortOrder;
                ViewData["TitleSortParam"] = String.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
                ViewData["DateSortParam"] = sortOrder == "date" ? "date_desc" : "date";
                ViewData["AuthorSortParam"] = sortOrder == "author" ? "author_desc" : "author";

                var sortedBooks = sortOrder switch
                {
                    "title_desc" => ownedBooks.OrderByDescending(b => b.Title),
                    "date" => ownedBooks.OrderBy(b => b.PurchaseDate),
                    "date_desc" => ownedBooks.OrderByDescending(b => b.PurchaseDate),
                    "author" => ownedBooks.OrderBy(b => b.AuthorName),
                    "author_desc" => ownedBooks.OrderByDescending(b => b.AuthorName),
                    _ => ownedBooks.OrderBy(b => b.Title)
                };

                return View(sortedBooks.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading customer library");
                TempData["ErrorMessage"] = "An error occurred while loading your library.";
                return RedirectToAction("Index", "Home", new { area = "" });
            }
        }

        // GET: Customer/Library/Download/5
        public async Task<IActionResult> Download(int id)
        {
            try
            {
                _logger.LogInformation($"Processing download request for product ID: {id}");

                // Get current user
                var currentUserEmail = User.Identity.Name;
                var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == currentUserEmail);

                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }

                // Verify ownership - check if user has purchased this book
                var hasPurchased = await _customerService.HasUserPurchasedBookAsync(currentUser.UserId, id);

                if (!hasPurchased)
                {
                    _logger.LogWarning($"User ID: {currentUser.UserId} attempted to download book ID: {id} without ownership");
                    TempData["ErrorMessage"] = "You don't own this book.";
                    return RedirectToAction(nameof(Index));
                }

                // Get book details
                var book = await _context.Products
                    .Include(p => p.Author)
                    .FirstOrDefaultAsync(p => p.ProductId == id);

                if (book == null)
                {
                    _logger.LogWarning($"Book ID: {id} not found");
                    return NotFound();
                }

                // Check if book file exists
                if (string.IsNullOrEmpty(book.FileName))
                {
                    _logger.LogWarning($"Book ID: {id} has no file attached");
                    TempData["ErrorMessage"] = "This book file is not available for download.";
                    return RedirectToAction(nameof(Index));
                }

                // Get book content stream
                var bookStream = await _fileStorageService.GetBookContentAsync(id);

                if (bookStream == null)
                {
                    _logger.LogWarning($"Book file not found for product ID: {id}");
                    TempData["ErrorMessage"] = "Book file not found. Please contact support.";
                    return RedirectToAction(nameof(Index));
                }

                // Determine content type based on file extension
                var fileExtension = Path.GetExtension(book.FileName).ToLowerInvariant();
                var contentType = fileExtension switch
                {
                    ".pdf" => "application/pdf",
                    ".epub" => "application/epub+zip",
                    ".mobi" => "application/x-mobipocket-ebook",
                    _ => "application/octet-stream"
                };

                // Generate download filename
                var downloadFileName = $"{book.Title.Replace(" ", "_")}_{book.Author.Name}_{book.Author.LastName}{fileExtension}";

                _logger.LogInformation($"Serving download for book ID: {id} to user ID: {currentUser.UserId}");

                // Return file for download
                return File(bookStream, contentType, downloadFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error downloading book ID: {id}");
                TempData["ErrorMessage"] = "An error occurred while downloading the book.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}