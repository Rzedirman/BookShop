using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookShop.Models;

namespace BookShop.Controllers
{
    public class ProductsController : Controller
    {
        private readonly myShopContext _context;

        public ProductsController(myShopContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var myShopContext = _context.Products.Include(p => p.Author).Include(p => p.Genre).Include(p => p.Language);
            return View(await myShopContext.ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Author)
                .Include(p => p.Genre)
                .Include(p => p.Language)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            ViewData["AuthorId"] = new SelectList(_context.Authors, "AuthorId", "AuthorId");
            ViewData["GenreId"] = new SelectList(_context.Genres, "GenreId", "GenreId");
            ViewData["LanguageId"] = new SelectList(_context.Languages, "LanguageId", "LanguageId");
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,AuthorId,GenreId,LanguageId,Title,Description,Price,InStock,PublicationDate,ImageName,FileName")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AuthorId"] = new SelectList(_context.Authors, "AuthorId", "AuthorId", product.AuthorId);
            ViewData["GenreId"] = new SelectList(_context.Genres, "GenreId", "GenreId", product.GenreId);
            ViewData["LanguageId"] = new SelectList(_context.Languages, "LanguageId", "LanguageId", product.LanguageId);
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["AuthorId"] = new SelectList(_context.Authors, "AuthorId", "AuthorId", product.AuthorId);
            ViewData["GenreId"] = new SelectList(_context.Genres, "GenreId", "GenreId", product.GenreId);
            ViewData["LanguageId"] = new SelectList(_context.Languages, "LanguageId", "LanguageId", product.LanguageId);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,AuthorId,GenreId,LanguageId,Title,Description,Price,InStock,PublicationDate,ImageName,FileName")] Product product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
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
            ViewData["AuthorId"] = new SelectList(_context.Authors, "AuthorId", "AuthorId", product.AuthorId);
            ViewData["GenreId"] = new SelectList(_context.Genres, "GenreId", "GenreId", product.GenreId);
            ViewData["LanguageId"] = new SelectList(_context.Languages, "LanguageId", "LanguageId", product.LanguageId);
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Author)
                .Include(p => p.Genre)
                .Include(p => p.Language)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Products == null)
            {
                return Problem("Entity set 'myShopContext.Products'  is null.");
            }
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
          return (_context.Products?.Any(e => e.ProductId == id)).GetValueOrDefault();
        }
    }
}
