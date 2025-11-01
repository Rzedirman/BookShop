// Controllers/BooksController.cs
// Public controller for book details page (AllowAnonymous)

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

namespace BookShop.Controllers
{
    [AllowAnonymous]
    public class BooksController : Controller
    {
        private readonly myShopContext _context;
        private readonly ICustomerService _customerService;
        private readonly IFavoriteService _favoriteService;
        private readonly ILogger<BooksController> _logger;

        public BooksController(
            myShopContext context,
            ICustomerService customerService,
            IFavoriteService favoriteService,
            ILogger<BooksController> logger)
        {
            _context = context;
            _customerService = customerService;
            _favoriteService = favoriteService;
            _logger = logger;
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                _logger.LogInformation($"Loading book details for product ID: {id}");

                // Get book with all related data
                var book = await _context.Products
                    .Include(p => p.Author)
                    .Include(p => p.Genre)
                    .Include(p => p.Language)
                    .Include(p => p.Seller)
                    .FirstOrDefaultAsync(p => p.ProductId == id);

                if (book == null)
                {
                    _logger.LogWarning($"Book with ID {id} not found");
                    return NotFound();
                }

                // Create view model
                var viewModel = new BookDetailViewModel
                {
                    Book = book,
                    IsOwned = false,
                    IsFavorite = false,
                    IsInCart = false
                };

                // If user is authenticated, check ownership, favorite, and cart status
                if (User.Identity.IsAuthenticated && User.IsInRole("user"))
                {
                    var currentUserEmail = User.Identity.Name;
                    var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == currentUserEmail);

                    if (currentUser != null)
                    {
                        // Check if user owns this book
                        viewModel.IsOwned = await _customerService.HasUserPurchasedBookAsync(currentUser.UserId, id.Value);

                        // Check if book is in favorites
                        viewModel.IsFavorite = await _favoriteService.IsFavoriteAsync(currentUser.UserId, id.Value);

                        // Check if book is in cart
                        viewModel.IsInCart = await _context.Carts
                            .AnyAsync(c => c.UserId == currentUser.UserId && c.ProductId == id.Value);
                    }
                }

                // Get related books (same author or same genre) - limit to 4
                viewModel.RelatedBooks = await _context.Products
                    .Where(p => p.ProductId != id && (p.AuthorId == book.AuthorId || p.GenreId == book.GenreId))
                    .Include(p => p.Author)
                    .Include(p => p.Genre)
                    .OrderByDescending(p => p.PublicationDate)
                    .Take(4)
                    .ToListAsync();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading book details for product ID: {id}");
                TempData["ErrorMessage"] = "An error occurred while loading the book details.";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}