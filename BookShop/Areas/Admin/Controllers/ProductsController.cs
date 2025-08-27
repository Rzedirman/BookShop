using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BookShop.Helpers;
using BookShop.Interfaces;
using BookShop.Models;
using BookShop.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class ProductsController : Controller
    {
        private readonly myShopContext _context;
        private readonly ILogger<ProductsController> _logger;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IFileStorageService _fileStorageService;

        public ProductsController(
            myShopContext context,
            ILogger<ProductsController> logger,
            IWebHostEnvironment hostEnvironment,
            IFileStorageService fileStorageService)
        {
            _context = context;
            _logger = logger;
            _hostEnvironment = hostEnvironment;
            _fileStorageService = fileStorageService;
        }

        // GET: Admin/Products
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber)
        {
            _logger.LogInformation("Loading admin products management");

            ViewData["CurrentSort"] = sortOrder;
            ViewData["TitleSortParam"] = String.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
            ViewData["AuthorSortParam"] = sortOrder == "author" ? "author_desc" : "author";
            ViewData["GenreSortParam"] = sortOrder == "genre" ? "genre_desc" : "genre";
            ViewData["PriceSortParam"] = sortOrder == "price" ? "price_desc" : "price";
            ViewData["DateSortParam"] = sortOrder == "date" ? "date_desc" : "date";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var products = _context.Products
                .Include(p => p.Author)
                .Include(p => p.Genre)
                .Include(p => p.Language)
                .Include(p => p.Seller)
                .AsQueryable();

            // Apply search filter if specified
            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p =>
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
                    products = products.OrderByDescending(p => p.Title);
                    break;
                case "author":
                    products = products.OrderBy(p => p.Author.LastName).ThenBy(p => p.Author.Name);
                    break;
                case "author_desc":
                    products = products.OrderByDescending(p => p.Author.LastName).ThenByDescending(p => p.Author.Name);
                    break;
                case "genre":
                    products = products.OrderBy(p => p.Genre.GenreName);
                    break;
                case "genre_desc":
                    products = products.OrderByDescending(p => p.Genre.GenreName);
                    break;
                case "price":
                    products = products.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price);
                    break;
                case "date":
                    products = products.OrderBy(p => p.PublicationDate);
                    break;
                case "date_desc":
                    products = products.OrderByDescending(p => p.PublicationDate);
                    break;
                default:
                    products = products.OrderBy(p => p.Title);
                    break;
            }

            int pageSize = 10;
            return View(await PaginatedList<Product>.CreateAsync(products.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Admin/Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Author)
                .Include(p => p.Genre)
                .Include(p => p.Language)
                .Include(p => p.Seller)
                .Include(p => p.Orders)
                .FirstOrDefaultAsync(m => m.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            // Get the total sales and revenue for the product
            ViewData["TotalSales"] = product.Orders.Sum(o => o.Amount);
            ViewData["TotalRevenue"] = product.Orders.Sum(o => o.TotalPrice);

            return View(product);
        }

        // GET: Admin/Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Create the view model
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
                FilePath = product.FileName,
                SellerId = product.SellerId.HasValue ? product.SellerId.Value.ToString() : null
            };

            // Populate dropdown lists
            PopulateDropdownLists(product);

            return View(viewModel);
        }

        // POST: Admin/Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BookViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            ModelState.Remove("CoverImage");
            ModelState.Remove("BookFile");
            ModelState.Remove("FilePath");
            ModelState.Remove("SellerId");

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the existing product
                    var product = await _context.Products.FindAsync(id);
                    if (product == null)
                    {
                        return NotFound();
                    }

                    // Update product properties
                    product.Title = viewModel.Title;
                    product.AuthorId = viewModel.AuthorId;
                    product.GenreId = viewModel.GenreId;
                    product.LanguageId = viewModel.LanguageId;
                    product.Description = viewModel.Description;
                    product.Price = viewModel.Price;
                    product.InStock = viewModel.InStock;
                    product.PublicationDate = viewModel.PublicationDate;

                    // Handle seller assignment if provided
                    if (!string.IsNullOrEmpty(viewModel.SellerId) && int.TryParse(viewModel.SellerId, out int sellerId))
                    {
                        product.SellerId = sellerId;
                    }
                    else
                    {
                        product.SellerId = null;
                    }

                    // Handle cover image upload if provided
                    if (viewModel.CoverImage != null && viewModel.CoverImage.Length > 0)
                    {
                        // Delete the old image if it exists and is not the default
                        if (!string.IsNullOrEmpty(product.ImageName) && product.ImageName != "noimage.png")
                        {
                            await _fileStorageService.DeleteBookCoverAsync(product.ProductId);
                        }

                        // Save the new image
                        product.ImageName = await _fileStorageService.SaveBookCoverAsync(viewModel.CoverImage, product.ProductId);
                    }

                    // Handle book file upload if provided
                    if (viewModel.BookFile != null)
                    {
                        // Delete the old file if it exists
                        if (!string.IsNullOrEmpty(product.FileName))
                        {
                            await _fileStorageService.DeleteBookAsync(product.ProductId);
                        }

                        // Save the new file
                        product.FileName = await _fileStorageService.SaveBookAsync(viewModel.BookFile, product.ProductId);
                    }

                    _context.Update(product);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Updated product ID {product.ProductId}: {product.Title}");

                    TempData["SuccessMessage"] = "Book updated successfully";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(viewModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error updating product ID {id}");
                    ModelState.AddModelError("", "An error occurred while updating the book. Please try again.");
                }
            }

            // If we got this far, something failed; redisplay form
            PopulateDropdownLists(null, viewModel.AuthorId, viewModel.GenreId, viewModel.LanguageId,
                !string.IsNullOrEmpty(viewModel.SellerId) ? int.Parse(viewModel.SellerId) : (int?)null);
            return View(viewModel);
        }

        // GET: Admin/Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Author)
                .Include(p => p.Genre)
                .Include(p => p.Language)
                .Include(p => p.Seller)
                .Include(p => p.Orders)
                .Include(p => p.Carts)
                .FirstOrDefaultAsync(m => m.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            // Check if the product has orders or is in carts
            ViewData["HasOrders"] = product.Orders.Any();
            ViewData["InCarts"] = product.Carts.Any();

            return View(product);
        }

        // POST: Admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products
                .Include(p => p.Orders)
                .Include(p => p.Carts)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            // Check if the product has orders
            if (product.Orders.Any())
            {
                TempData["ErrorMessage"] = "This book cannot be deleted because it has associated orders.";
                return RedirectToAction(nameof(Index));
            }

            // Remove any cart items referencing this product
            _context.Carts.RemoveRange(product.Carts);

            // Remove any favorites referencing this product
            var favorites = await _context.Favorites.Where(f => f.ProductId == id).ToListAsync();
            _context.Favorites.RemoveRange(favorites);

            // Delete associated files
            if (!string.IsNullOrEmpty(product.ImageName) && product.ImageName != "noimage.png")
            {
                await _fileStorageService.DeleteBookCoverAsync(product.ProductId);
            }

            if (!string.IsNullOrEmpty(product.FileName))
            {
                await _fileStorageService.DeleteBookAsync(product.ProductId);
            }

            // Delete the product
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Deleted product ID {id}: {product.Title}");

            TempData["SuccessMessage"] = "Book deleted successfully";
            return RedirectToAction(nameof(Index));
        }


        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }

        private void PopulateDropdownLists(Product product = null, int? selectedAuthorId = null,
            int? selectedGenreId = null, int? selectedLanguageId = null, int? selectedSellerId = null)
        {
            // For authors, show "LastName, FirstName" format
            ViewData["AuthorId"] = new SelectList(_context.Authors
                .OrderBy(a => a.LastName)
                .ThenBy(a => a.Name)
                .Select(a => new
                {
                    a.AuthorId,
                    FullName = $"{a.LastName}, {a.Name}"
                }),
                "AuthorId", "FullName", selectedAuthorId ?? product?.AuthorId);

            // For genres
            ViewData["GenreId"] = new SelectList(_context.Genres
                .OrderBy(g => g.GenreName),
                "GenreId", "GenreName", selectedGenreId ?? product?.GenreId);

            // For languages
            ViewData["LanguageId"] = new SelectList(_context.Languages
                .OrderBy(l => l.LanguageName),
                "LanguageId", "LanguageName", selectedLanguageId ?? product?.LanguageId);

            // For sellers (only include users with seller role)
            var sellerRoleId = _context.Roles.FirstOrDefault(r => r.RoleName == "seller")?.RoleId;
            if (sellerRoleId.HasValue)
            {
                ViewData["SellerId"] = new SelectList(_context.Users
                    .Where(u => u.RoleId == sellerRoleId.Value)
                    .OrderBy(u => u.LastName)
                    .ThenBy(u => u.Name)
                    .Select(u => new
                    {
                        u.UserId,
                        FullName = $"{u.LastName}, {u.Name} ({u.Email})"
                    }),
                    "UserId", "FullName", selectedSellerId ?? product?.SellerId);
            }
            else
            {
                ViewData["SellerId"] = new SelectList(Enumerable.Empty<SelectListItem>());
            }
        }
    }
}