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
    public class LanguagesController : Controller
    {
        private readonly myShopContext _context;
        private readonly ILogger<LanguagesController> _logger;

        public LanguagesController(myShopContext context, ILogger<LanguagesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/Languages
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber)
        {
            _logger.LogInformation("Loading admin language management");

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

            var languages = from l in _context.Languages
                         .Include(l => l.Products) // Include related products
                            select l;

            // Apply search filter if specified
            if (!string.IsNullOrEmpty(searchString))
            {
                languages = languages.Where(l => l.LanguageName.Contains(searchString));
            }

            // Default is alphabetical order
            switch (sortOrder)
            {
                case "name_desc":
                    languages = languages.OrderByDescending(l => l.LanguageName);
                    break;
                default:
                    languages = languages.OrderBy(l => l.LanguageName);
                    break;
            }

            int pageSize = 10;
            return View(await PaginatedList<Language>.CreateAsync(languages.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Admin/Languages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var language = await _context.Languages
                .Include(l => l.Products) // Include related products
                .ThenInclude(p => p.Author) // Include each product's author for display
                .FirstOrDefaultAsync(m => m.LanguageId == id);

            if (language == null)
            {
                return NotFound();
            }

            return View(language);
        }

        // GET: Admin/Languages/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Languages/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LanguageName")] Language language)
        {
            // Remove LanguageId from binding to ensure a new record is created

            if (ModelState.IsValid)
            {
                // Check if language with same name already exists
                if (await _context.Languages.AnyAsync(l => l.LanguageName.ToLower() == language.LanguageName.ToLower()))
                {
                    ModelState.AddModelError("LanguageName", "A language with this name already exists");
                    return View(language);
                }

                _context.Add(language);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Created new language: {language.LanguageName}");

                TempData["SuccessMessage"] = "Language created successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(language);
        }

        // GET: Admin/Languages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var language = await _context.Languages.FindAsync(id);
            if (language == null)
            {
                return NotFound();
            }
            return View(language);
        }

        // POST: Admin/Languages/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LanguageId,LanguageName")] Language language)
        {
            if (id != language.LanguageId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Check if language with same name already exists (excluding current language)
                if (await _context.Languages.AnyAsync(l =>
                    l.LanguageName.ToLower() == language.LanguageName.ToLower() &&
                    l.LanguageId != language.LanguageId))
                {
                    ModelState.AddModelError("LanguageName", "A language with this name already exists");
                    return View(language);
                }

                try
                {
                    _context.Update(language);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Updated language ID {language.LanguageId} to: {language.LanguageName}");

                    TempData["SuccessMessage"] = "Language updated successfully";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LanguageExists(language.LanguageId))
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
            return View(language);
        }

        // GET: Admin/Languages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var language = await _context.Languages
                .Include(l => l.Products) // Include related products
                .ThenInclude(p => p.Author) // Include each product's author for display
                .FirstOrDefaultAsync(m => m.LanguageId == id);

            if (language == null)
            {
                return NotFound();
            }

            // Check if the language is in use
            var languageInUse = await _context.Products.AnyAsync(p => p.LanguageId == id);
            ViewData["LanguageInUse"] = languageInUse;

            return View(language);
        }

        // POST: Admin/Languages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var language = await _context.Languages.FindAsync(id);

            // Re-check if language is in use before attempting to delete
            bool languageInUse = await _context.Products.AnyAsync(p => p.LanguageId == id);

            if (languageInUse)
            {
                TempData["ErrorMessage"] = "This language cannot be deleted because it is used by one or more books.";
                return RedirectToAction(nameof(Index));
            }

            if (language != null)
            {
                _context.Languages.Remove(language);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Deleted language ID {id}: {language.LanguageName}");

                TempData["SuccessMessage"] = "Language deleted successfully";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool LanguageExists(int id)
        {
            return _context.Languages.Any(e => e.LanguageId == id);
        }
    }
}