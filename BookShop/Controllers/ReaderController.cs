// ReaderController.cs
// Controller for e-book reading functionality

using BookShop.Interfaces;
using BookShop.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BookShop.Controllers
{
    [Authorize]
    public class ReaderController : Controller
    {
        private readonly ILogger<ReaderController> _logger;
        private readonly IBookService _bookService;
        private readonly IFileStorageService _fileStorageService;

        public ReaderController(
            ILogger<ReaderController> logger,
            IBookService bookService,
            IFileStorageService fileStorageService)
        {
            _logger = logger;
            _bookService = bookService;
            _fileStorageService = fileStorageService;
        }

        // GET: Reader/Index/5
        public async Task<IActionResult> Index(int id)
        {
            _logger.LogInformation($"Opening reader for book ID: {id}");
            
            // TODO: Verify that the user has purchased this book
            // TODO: Get book details
            
            try
            {
                var book = await _bookService.GetBookByIdAsync(id);
                
                if (book == null)
                {
                    return NotFound();
                }
                
                // Pass book details to the view for the reader
                return View(book);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error opening reader for book ID: {id}");
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: Reader/GetBookContent/5
        public async Task<IActionResult> GetBookContent(int id)
        {
            _logger.LogInformation($"Getting book content for book ID: {id}");
            
            // TODO: Verify that the user has purchased this book
            
            try
            {
                // Get the book file stream
                var bookStream = await _fileStorageService.GetBookContentAsync(id);
                
                if (bookStream == null)
                {
                    return NotFound();
                }
                
                // Get the book details for content type information
                var book = await _bookService.GetBookByIdAsync(id);
                
                // Determine content type based on file extension
                string contentType = "application/pdf"; // Default to PDF
                
                // Return the file stream
                return File(bookStream, contentType, enableRangeProcessing: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting book content for book ID: {id}");
                return StatusCode(500, "Error retrieving the book content");
            }
        }

        // GET: Reader/Bookmarks/5
        public async Task<IActionResult> Bookmarks(int id)
        {
            _logger.LogInformation($"Getting bookmarks for book ID: {id}");
            
            try
            {
                // Get the current user ID
                var userId = User.Identity.Name;
                
                // Get bookmarks for this user and book
                var bookmarks = await _bookService.GetUserBookmarksAsync(userId, id);
                
                return View(bookmarks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting bookmarks for book ID: {id}");
                return RedirectToAction("Error", "Home");
            }
        }

        // POST: Reader/AddBookmark
        [HttpPost]
        public async Task<IActionResult> AddBookmark(int bookId, int pageNumber, string name)
        {
            _logger.LogInformation($"Adding bookmark at page {pageNumber} for book ID: {bookId}");
            
            try
            {
                // Get the current user ID
                var userId = User.Identity.Name;
                
                // Add the bookmark
                await _bookService.AddBookmarkAsync(bookId, userId, pageNumber, name);
                
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding bookmark for book ID: {bookId}");
                return StatusCode(500, new { success = false, message = "Error adding bookmark" });
            }
        }

        // POST: Reader/DeleteBookmark
        [HttpPost]
        public async Task<IActionResult> DeleteBookmark(int id)
        {
            _logger.LogInformation($"Deleting bookmark ID: {id}");
            
            try
            {
                // Delete the bookmark
                await _bookService.DeleteBookmarkAsync(id);
                
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting bookmark ID: {id}");
                return StatusCode(500, new { success = false, message = "Error deleting bookmark" });
            }
        }
    }
}
