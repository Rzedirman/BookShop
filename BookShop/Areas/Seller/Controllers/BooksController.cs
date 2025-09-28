// Areas/Seller/Controllers/BooksController.cs
// Book management controller for sellers

using BookShop.Helpers;
using BookShop.Interfaces;
using BookShop.Models;
using BookShop.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BookShop.Areas.Seller.Controllers
{
    [Area("Seller")]
    [Authorize(Roles = "seller")]
    public class BooksController : Controller
    {
        private readonly myShopContext _context;
        private readonly ILogger<BooksController> _logger;
        private readonly IFileStorageService _fileStorageService;

        public BooksController(
            myShopContext context,
            ILogger<BooksController> logger,
            IFileStorageService fileStorageService)
        {
            _context = context;
            _logger = logger;
            _fileStorageService = fileStorageService;
        }

        // GET: Seller/Books
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber)
        {
            _logger.LogInformation("Loading seller books management");

            // Get current seller's user ID
            var currentUserEmail = User.Identity.Name;
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == currentUserEmail);

            if (currentUser == null)
            {
                return RedirectToAction("Login", "Home", new { area = "" });
            }

            ViewData["CurrentSort"] = sortOrder;
            ViewData["TitleSortParam"] = String.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
            ViewData["PriceSortParam"] = sortOrder == "price" ? "price_desc" : "price";
            ViewData["DateSortParam"] = sortOrder == "date" ? "date_desc" : "date";
            ViewData["SalesSortParam"] = sortOrder == "sales" ? "sales_desc" : "sales";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            // Get only seller's books
            var books = _context.Products
                .Include(p => p.Author)
                .Include(p => p.Genre)
                .Include(p => p.Language)
                .Include(p => p.Orders)
                .Where(p => p.SellerId == currentUser.UserId)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                books = books.Where(p =>
                    p.Title.Contains(searchString) ||
                    p.Author.Name.Contains(searchString) ||
                    p.Author.LastName.Contains(searchString) ||
                    p.Genre.GenreName.Contains(searchString) ||
                    p.Description.Contains(searchString));
            }

            // Apply sorting
            switch (sortOrder)
            {
                case "title_desc":
                    books = books.OrderByDescending(p => p.Title);
                    break;
                case "price":
                    books = books.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    books = books.OrderByDescending(p => p.Price);
                    break;
                case "date":
                    books = books.OrderBy(p => p.PublicationDate);
                    break;
                case "date_desc":
                    books = books.OrderByDescending(p => p.PublicationDate);
                    break;
                case "sales":
                    books = books.OrderBy(p => p.Orders.Sum(o => o.Amount));
                    break;
                case "sales_desc":
                    books = books.OrderByDescending(p => p.Orders.Sum(o => o.Amount));
                    break;
                default:
                    books = books.OrderBy(p => p.Title);
                    break;
            }

            int pageSize = 10;
            return View(await PaginatedList<Product>.CreateAsync(books.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Seller/Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Home", new { area = "" });
            }

            var book = await _context.Products
                .Include(p => p.Author)
                .Include(p => p.Genre)
                .Include(p => p.Language)
                .Include(p => p.Orders)
                .ThenInclude(o => o.User)
                .FirstOrDefaultAsync(m => m.ProductId == id && m.SellerId == currentUser.UserId);

            if (book == null)
            {
                return NotFound();
            }

            // Calculate statistics
            ViewData["TotalSales"] = book.Orders.Sum(o => o.Amount);
            ViewData["TotalRevenue"] = book.Orders.Sum(o => o.TotalPrice);
            ViewData["LastSoldDate"] = book.Orders.Any() ? book.Orders.Max(o => o.OrderDate) : (DateTime?)null;

            return View(book);
        }

        // GET: Seller/Books/Create
        public IActionResult Create()
        {
            PopulateDropdownLists();
            return View(new CreateBookViewModel());
        }

        // POST: Seller/Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBookViewModel viewModel)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Home", new { area = "" });
            }

            // Custom validation for author
            if (viewModel.IsCreatingNewAuthor)
            {
                if (string.IsNullOrWhiteSpace(viewModel.AuthorFirstName) || string.IsNullOrWhiteSpace(viewModel.AuthorLastName))
                {
                    ModelState.AddModelError("", "Author first name and last name are required when creating a new author.");
                }
                if (string.IsNullOrWhiteSpace(viewModel.AuthorCountry))
                {
                    ModelState.AddModelError("AuthorCountry", "Author country is required when creating a new author.");
                }
            }

            // Custom validation for genre
            if (viewModel.IsCreatingNewGenre && string.IsNullOrWhiteSpace(viewModel.NewGenreName))
            {
                ModelState.AddModelError("NewGenreName", "Genre name is required when creating a new genre.");
            }

            // Custom validation for language
            if (viewModel.IsCreatingNewLanguage && string.IsNullOrWhiteSpace(viewModel.NewLanguageName))
            {
                ModelState.AddModelError("NewLanguageName", "Language name is required when creating a new language.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Handle author creation/selection with fuzzy matching
                    int authorId;
                    if (viewModel.IsCreatingNewAuthor)
                    {
                        authorId = await GetOrCreateAuthorAsync(viewModel.AuthorFirstName.Trim(),
                            viewModel.AuthorLastName.Trim(), viewModel.AuthorCountry.Trim(),
                            viewModel.AuthorBirthDate, viewModel.AuthorDeathDate);
                    }
                    else
                    {
                        authorId = viewModel.AuthorId.Value;
                    }

                    // Handle genre creation/selection with fuzzy matching
                    int genreId;
                    if (viewModel.IsCreatingNewGenre)
                    {
                        genreId = await GetOrCreateGenreAsync(viewModel.NewGenreName.Trim());
                    }
                    else
                    {
                        genreId = viewModel.GenreId.Value;
                    }

                    // Handle language creation/selection with fuzzy matching
                    int languageId;
                    if (viewModel.IsCreatingNewLanguage)
                    {
                        languageId = await GetOrCreateLanguageAsync(viewModel.NewLanguageName.Trim());
                    }
                    else
                    {
                        languageId = viewModel.LanguageId.Value;
                    }

                    // Create the product
                    var product = new Product
                    {
                        Title = viewModel.Title.Trim(),
                        AuthorId = authorId,
                        GenreId = genreId,
                        LanguageId = languageId,
                        Description = viewModel.Description.Trim(),
                        Price = viewModel.Price,
                        InStock = viewModel.InStock,
                        PublicationDate = viewModel.PublicationDate,
                        SellerId = currentUser.UserId,
                        ImageName = "noimage.png" // Default, will be updated if cover image is provided
                    };

                    _context.Add(product);
                    await _context.SaveChangesAsync();

                    // Handle file uploads
                    if (viewModel.CoverImage != null)
                    {
                        product.ImageName = await _fileStorageService.SaveBookCoverAsync(viewModel.CoverImage, product.ProductId);
                    }

                    if (viewModel.BookFile != null)
                    {
                        product.FileName = await _fileStorageService.SaveBookAsync(viewModel.BookFile, product.ProductId);
                    }

                    _context.Update(product);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Seller {currentUser.UserId} created new book: {product.Title}");
                    TempData["SuccessMessage"] = "Book created successfully!";
                    return RedirectToAction(nameof(Details), new { id = product.ProductId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating book for seller {SellerId}", currentUser.UserId);
                    ModelState.AddModelError("", "An error occurred while creating the book. Please try again.");
                }
            }

            PopulateDropdownLists();
            return View(viewModel);
        }

        // GET: Seller/Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Home", new { area = "" });
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == id && p.SellerId == currentUser.UserId);

            if (product == null)
            {
                return NotFound();
            }

            var viewModel = new BookViewModel
            {
                Id = product.ProductId,
                Title = product.Title,
                AuthorId = product.AuthorId,
                GenreId = product.GenreId,
                LanguageId = product.LanguageId,
                Description = product.Description,
                Price = product.Price,
                InStock = product.InStock,
                PublicationDate = product.PublicationDate,
                ImageName = product.ImageName,
                FilePath = product.FileName
            };

            PopulateDropdownListsForEdit(product);
            return View(viewModel);
        }

        // POST: Seller/Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BookViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Home", new { area = "" });
            }

            // Remove validation for optional fields
            ModelState.Remove("CoverImage");
            ModelState.Remove("BookFile");

            if (ModelState.IsValid)
            {
                try
                {
                    var product = await _context.Products
                        .FirstOrDefaultAsync(p => p.ProductId == id && p.SellerId == currentUser.UserId);

                    if (product == null)
                    {
                        return NotFound();
                    }

                    // Update product properties
                    product.Title = viewModel.Title.Trim();
                    product.AuthorId = viewModel.AuthorId;
                    product.GenreId = viewModel.GenreId;
                    product.LanguageId = viewModel.LanguageId;
                    product.Description = viewModel.Description.Trim();
                    product.Price = viewModel.Price;
                    product.InStock = viewModel.InStock;
                    product.PublicationDate = viewModel.PublicationDate;

                    // Handle file uploads
                    if (viewModel.CoverImage != null)
                    {
                        if (!string.IsNullOrEmpty(product.ImageName) && product.ImageName != "noimage.png")
                        {
                            await _fileStorageService.DeleteBookCoverAsync(product.ProductId);
                        }
                        product.ImageName = await _fileStorageService.SaveBookCoverAsync(viewModel.CoverImage, product.ProductId);
                    }

                    if (viewModel.BookFile != null)
                    {
                        if (!string.IsNullOrEmpty(product.FileName))
                        {
                            await _fileStorageService.DeleteBookAsync(product.ProductId);
                        }
                        product.FileName = await _fileStorageService.SaveBookAsync(viewModel.BookFile, product.ProductId);
                    }

                    _context.Update(product);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Seller {currentUser.UserId} updated book: {product.Title}");
                    TempData["SuccessMessage"] = "Book updated successfully!";
                    return RedirectToAction(nameof(Details), new { id = product.ProductId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating book {BookId} for seller {SellerId}", id, currentUser.UserId);
                    ModelState.AddModelError("", "An error occurred while updating the book. Please try again.");
                }
            }

            var originalProduct = await _context.Products.FindAsync(id);
            PopulateDropdownListsForEdit(originalProduct);
            return View(viewModel);
        }

        // GET: Seller/Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Home", new { area = "" });
            }

            var product = await _context.Products
                .Include(p => p.Author)
                .Include(p => p.Genre)
                .Include(p => p.Language)
                .Include(p => p.Orders)
                .Include(p => p.Carts)
                .FirstOrDefaultAsync(m => m.ProductId == id && m.SellerId == currentUser.UserId);

            if (product == null)
            {
                return NotFound();
            }

            ViewData["HasOrders"] = product.Orders.Any();
            ViewData["InCarts"] = product.Carts.Any();
            return View(product);
        }

        // POST: Seller/Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Home", new { area = "" });
            }

            var product = await _context.Products
                .Include(p => p.Orders)
                .Include(p => p.Carts)
                .FirstOrDefaultAsync(p => p.ProductId == id && p.SellerId == currentUser.UserId);

            if (product == null)
            {
                return NotFound();
            }

            if (product.Orders.Any())
            {
                TempData["ErrorMessage"] = "Cannot delete a book that has been sold. This preserves order history.";
                return RedirectToAction(nameof(Index));
            }

            // Remove from carts
            _context.Carts.RemoveRange(product.Carts);

            // Remove favorites
            var favorites = await _context.Favorites.Where(f => f.ProductId == id).ToListAsync();
            _context.Favorites.RemoveRange(favorites);

            // Delete files
            if (!string.IsNullOrEmpty(product.ImageName) && product.ImageName != "noimage.png")
            {
                await _fileStorageService.DeleteBookCoverAsync(product.ProductId);
            }
            if (!string.IsNullOrEmpty(product.FileName))
            {
                await _fileStorageService.DeleteBookAsync(product.ProductId);
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Seller {currentUser.UserId} deleted book: {product.Title}");
            TempData["SuccessMessage"] = "Book deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        #region Private Helper Methods

        private async Task<User> GetCurrentUserAsync()
        {
            var currentUserEmail = User.Identity.Name;
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == currentUserEmail);
        }

        /// <summary>
        /// Fuzzy matching for author names to prevent duplicates
        /// </summary>
        private async Task<int> GetOrCreateAuthorAsync(string firstName, string lastName, string country,
            DateTime? birthDate = null, DateTime? deathDate = null)
        {
            // Check for exact match first
            var existingAuthor = await _context.Authors
                .FirstOrDefaultAsync(a =>
                    a.Name.ToLower() == firstName.ToLower() &&
                    a.LastName.ToLower() == lastName.ToLower() &&
                    a.Country.ToLower() == country.ToLower());

            if (existingAuthor != null)
            {
                return existingAuthor.AuthorId;
            }

            // Check for fuzzy matches (similar names)
            var similarAuthors = await _context.Authors
                .Where(a =>
                    (a.Name.ToLower().Contains(firstName.ToLower()) || firstName.ToLower().Contains(a.Name.ToLower())) &&
                    (a.LastName.ToLower().Contains(lastName.ToLower()) || lastName.ToLower().Contains(a.LastName.ToLower())))
                .ToListAsync();

            // If we find a very similar author, return it
            var exactMatch = similarAuthors.FirstOrDefault(a =>
                LevenshteinDistance(a.Name.ToLower(), firstName.ToLower()) <= 2 &&
                LevenshteinDistance(a.LastName.ToLower(), lastName.ToLower()) <= 2);

            if (exactMatch != null)
            {
                return exactMatch.AuthorId;
            }

            // Create new author
            var newAuthor = new Author
            {
                Name = firstName,
                LastName = lastName,
                Country = country,
                BirthDate = birthDate,
                DeathDate = deathDate
            };

            _context.Authors.Add(newAuthor);
            await _context.SaveChangesAsync();
            return newAuthor.AuthorId;
        }

        /// <summary>
        /// Fuzzy matching for genre names to prevent duplicates
        /// </summary>
        private async Task<int> GetOrCreateGenreAsync(string genreName)
        {
            // Check for exact match
            var existingGenre = await _context.Genres
                .FirstOrDefaultAsync(g => g.GenreName.ToLower() == genreName.ToLower());

            if (existingGenre != null)
            {
                return existingGenre.GenreId;
            }

            // Check for fuzzy matches
            var similarGenres = await _context.Genres
                .Where(g => g.GenreName.ToLower().Contains(genreName.ToLower()) || genreName.ToLower().Contains(g.GenreName.ToLower()))
                .ToListAsync();

            var exactMatch = similarGenres.FirstOrDefault(g =>
                LevenshteinDistance(g.GenreName.ToLower(), genreName.ToLower()) <= 2);

            if (exactMatch != null)
            {
                return exactMatch.GenreId;
            }

            // Create new genre
            var newGenre = new Genre { GenreName = genreName };
            _context.Genres.Add(newGenre);
            await _context.SaveChangesAsync();
            return newGenre.GenreId;
        }

        /// <summary>
        /// Fuzzy matching for language names to prevent duplicates
        /// </summary>
        private async Task<int> GetOrCreateLanguageAsync(string languageName)
        {
            // Check for exact match
            var existingLanguage = await _context.Languages
                .FirstOrDefaultAsync(l => l.LanguageName.ToLower() == languageName.ToLower());

            if (existingLanguage != null)
            {
                return existingLanguage.LanguageId;
            }

            // Check for fuzzy matches
            var similarLanguages = await _context.Languages
                .Where(l => l.LanguageName.ToLower().Contains(languageName.ToLower()) || languageName.ToLower().Contains(l.LanguageName.ToLower()))
                .ToListAsync();

            var exactMatch = similarLanguages.FirstOrDefault(l =>
                LevenshteinDistance(l.LanguageName.ToLower(), languageName.ToLower()) <= 2);

            if (exactMatch != null)
            {
                return exactMatch.LanguageId;
            }

            // Create new language
            var newLanguage = new Language { LanguageName = languageName };
            _context.Languages.Add(newLanguage);
            await _context.SaveChangesAsync();
            return newLanguage.LanguageId;
        }

        /// <summary>
        /// Calculate Levenshtein distance for fuzzy string matching
        /// </summary>
        private int LevenshteinDistance(string s, string t)
        {
            if (string.IsNullOrEmpty(s)) return t?.Length ?? 0;
            if (string.IsNullOrEmpty(t)) return s.Length;

            int[,] d = new int[s.Length + 1, t.Length + 1];

            for (int i = 0; i <= s.Length; i++) d[i, 0] = i;
            for (int j = 0; j <= t.Length; j++) d[0, j] = j;

            for (int i = 1; i <= s.Length; i++)
            {
                for (int j = 1; j <= t.Length; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }

            return d[s.Length, t.Length];
        }

        private void PopulateDropdownLists()
        {
            ViewData["AuthorId"] = new SelectList(_context.Authors
                .OrderBy(a => a.LastName)
                .ThenBy(a => a.Name)
                .Select(a => new
                {
                    a.AuthorId,
                    FullName = $"{a.LastName}, {a.Name}"
                }),
                "AuthorId", "FullName");

            ViewData["GenreId"] = new SelectList(_context.Genres.OrderBy(g => g.GenreName), "GenreId", "GenreName");
            ViewData["LanguageId"] = new SelectList(_context.Languages.OrderBy(l => l.LanguageName), "LanguageId", "LanguageName");
        }

        private void PopulateDropdownListsForEdit(Product product)
        {
            ViewData["AuthorId"] = new SelectList(_context.Authors
                .OrderBy(a => a.LastName)
                .ThenBy(a => a.Name)
                .Select(a => new
                {
                    a.AuthorId,
                    FullName = $"{a.LastName}, {a.Name}"
                }),
                "AuthorId", "FullName", product?.AuthorId);

            ViewData["GenreId"] = new SelectList(_context.Genres.OrderBy(g => g.GenreName),
                "GenreId", "GenreName", product?.GenreId);
            ViewData["LanguageId"] = new SelectList(_context.Languages.OrderBy(l => l.LanguageName),
                "LanguageId", "LanguageName", product?.LanguageId);
        }

        #endregion
    }
}