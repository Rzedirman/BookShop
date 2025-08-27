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
    public class AuthorsController : Controller
    {
        private readonly myShopContext _context;
        private readonly ILogger<AuthorsController> _logger;

        public AuthorsController(myShopContext context, ILogger<AuthorsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/Authors
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber)
        {
            _logger.LogInformation("Loading admin author management");

            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParam"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["LastNameSortParam"] = sortOrder == "lastname" ? "lastname_desc" : "lastname";
            ViewData["CountrySortParam"] = sortOrder == "country" ? "country_desc" : "country";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var authors = from a in _context.Authors
                         .Include(a => a.Products) // Include related products
                          select a;

            // Apply search filter if specified
            if (!string.IsNullOrEmpty(searchString))
            {
                authors = authors.Where(a =>
                    a.Name.Contains(searchString) ||
                    a.LastName.Contains(searchString) ||
                    a.Country.Contains(searchString));
            }

            // Apply sorting
            switch (sortOrder)
            {
                case "name_desc":
                    authors = authors.OrderByDescending(a => a.Name);
                    break;
                case "lastname":
                    authors = authors.OrderBy(a => a.LastName);
                    break;
                case "lastname_desc":
                    authors = authors.OrderByDescending(a => a.LastName);
                    break;
                case "country":
                    authors = authors.OrderBy(a => a.Country);
                    break;
                case "country_desc":
                    authors = authors.OrderByDescending(a => a.Country);
                    break;
                default:
                    authors = authors.OrderBy(a => a.Name);
                    break;
            }

            int pageSize = 10;
            return View(await PaginatedList<Author>.CreateAsync(authors.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Admin/Authors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var author = await _context.Authors
                .Include(a => a.Products) // Include related products
                .ThenInclude(p => p.Genre) // Include each product's genre for display
                .Include(a => a.Products) // Include again to get another related entity
                .ThenInclude(p => p.Language) // Include each product's language for display
                .FirstOrDefaultAsync(m => m.AuthorId == id);

            if (author == null)
            {
                return NotFound();
            }

            return View(author);
        }

        // GET: Admin/Authors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var author = await _context.Authors.FindAsync(id);
            if (author == null)
            {
                return NotFound();
            }
            return View(author);
        }

        // POST: Admin/Authors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AuthorId,Name,LastName,Country,BirthDate,DeathDate")] Author author)
        {
            if (id != author.AuthorId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(author);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Updated author ID {author.AuthorId}: {author.Name} {author.LastName}");

                    TempData["SuccessMessage"] = "Author updated successfully";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AuthorExists(author.AuthorId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(author);
        }

        // GET: Admin/Authors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var author = await _context.Authors
                .Include(a => a.Products) // Include related products
                .ThenInclude(p => p.Genre) // Include each product's genre for display
                .Include(a => a.Products) // Include again to get another related entity
                .ThenInclude(p => p.Language) // Include each product's language for display
                .FirstOrDefaultAsync(m => m.AuthorId == id);

            if (author == null)
            {
                return NotFound();
            }

            // Check if the author has associated books
            ViewData["AuthorHasBooks"] = author.Products.Any();

            return View(author);
        }

        // POST: Admin/Authors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var author = await _context.Authors
                .Include(a => a.Products)
                .FirstOrDefaultAsync(a => a.AuthorId == id);

            if (author == null)
            {
                return NotFound();
            }

            // Check if the author has associated books
            if (author.Products.Any())
            {
                TempData["ErrorMessage"] = "This author cannot be deleted because they have associated books.";
                return RedirectToAction(nameof(Index));
            }

            _context.Authors.Remove(author);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Deleted author ID {id}: {author.Name} {author.LastName}");

            TempData["SuccessMessage"] = "Author deleted successfully";
            return RedirectToAction(nameof(Index));
        }

        private bool AuthorExists(int id)
        {
            return _context.Authors.Any(e => e.AuthorId == id);
        }
    }
}