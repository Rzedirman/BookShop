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
    public class GenresController : Controller
    {
        private readonly myShopContext _context;
        private readonly ILogger<GenresController> _logger;

        public GenresController(myShopContext context, ILogger<GenresController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/Genres
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber)
        {
            _logger.LogInformation("Loading admin genre management");

            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParam"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var genres = from g in _context.Genres
                         .Include(g => g.Products) // Include related products
                         select g;

            // Apply search filter if specified
            if (!string.IsNullOrEmpty(searchString))
            {
                genres = genres.Where(g => g.GenreName.Contains(searchString));
            }

            // Default is alphabetical order
            switch (sortOrder)
            {
                case "name_desc":
                    genres = genres.OrderByDescending(g => g.GenreName);
                    break;
                default:
                    genres = genres.OrderBy(g => g.GenreName);
                    break;
            }

            int pageSize = 10;
            return View(await Helpers.PaginatedList<Genre>.CreateAsync(genres.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Admin/Genres/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var genre = await _context.Genres
                .Include(g => g.Products) // Include related products
                .ThenInclude(p => p.Author) // Include each product's author for display
                .FirstOrDefaultAsync(m => m.GenreId == id);

            if (genre == null)
            {
                return NotFound();
            }

            return View(genre);
        }

        // GET: Admin/Genres/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Genres/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("GenreName")] Genre genre)
        {
            // Remove GenreId from binding to ensure a new record is created

            if (ModelState.IsValid)
            {
                // Check if genre with same name already exists
                if (await _context.Genres.AnyAsync(g => g.GenreName.ToLower() == genre.GenreName.ToLower()))
                {
                    ModelState.AddModelError("GenreName", "A genre with this name already exists");
                    return View(genre);
                }

                _context.Add(genre);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Created new genre: {genre.GenreName}");

                TempData["SuccessMessage"] = "Genre created successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(genre);
        }

        // GET: Admin/Genres/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var genre = await _context.Genres.FindAsync(id);
            if (genre == null)
            {
                return NotFound();
            }
            return View(genre);
        }

        // POST: Admin/Genres/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("GenreId,GenreName")] Genre genre)
        {
            if (id != genre.GenreId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Check if genre with same name already exists (excluding current genre)
                if (await _context.Genres.AnyAsync(g =>
                    g.GenreName.ToLower() == genre.GenreName.ToLower() &&
                    g.GenreId != genre.GenreId))
                {
                    ModelState.AddModelError("GenreName", "A genre with this name already exists");
                    return View(genre);
                }

                try
                {
                    _context.Update(genre);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Updated genre ID {genre.GenreId} to: {genre.GenreName}");

                    TempData["SuccessMessage"] = "Genre updated successfully";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GenreExists(genre.GenreId))
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
            return View(genre);
        }

        // GET: Admin/Genres/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var genre = await _context.Genres
                .Include(g => g.Products) // Include related products
                .ThenInclude(p => p.Author) // Include each product's author for display
                .FirstOrDefaultAsync(m => m.GenreId == id);

            if (genre == null)
            {
                return NotFound();
            }

            // Check if the genre is in use
            var genreInUse = await _context.Products.AnyAsync(p => p.GenreId == id);
            ViewData["GenreInUse"] = genreInUse;

            return View(genre);
        }

        // POST: Admin/Genres/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var genre = await _context.Genres.FindAsync(id);

            // Re-check if genre is in use before attempting to delete
            bool genreInUse = await _context.Products.AnyAsync(p => p.GenreId == id);

            if (genreInUse)
            {
                TempData["ErrorMessage"] = "This genre cannot be deleted because it is used by one or more books.";
                return RedirectToAction(nameof(Index));
            }

            if (genre != null)
            {
                _context.Genres.Remove(genre);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Deleted genre ID {id}: {genre.GenreName}");

                TempData["SuccessMessage"] = "Genre deleted successfully";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool GenreExists(int id)
        {
            return _context.Genres.Any(e => e.GenreId == id);
        }
    }
}