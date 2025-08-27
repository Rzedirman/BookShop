// FileStorageService.cs
// Handles storage and retrieval of digital book files and cover images
// Implemented version that was stubbed out before

using BookShop.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BookShop.Classes
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FileStorageService> _logger;
        private readonly string _bookBasePath;
        private readonly string _coverBasePath;
        private readonly string[] _allowedBookExtensions;
        private readonly string[] _allowedImageExtensions;
        private readonly long _maxBookFileSize;
        private readonly long _maxCoverImageSize;

        public FileStorageService(
            IWebHostEnvironment environment,
            IConfiguration configuration,
            ILogger<FileStorageService> logger)
        {
            _environment = environment;
            _configuration = configuration;
            _logger = logger;

            // Get file paths and allowed extensions
            _bookBasePath = Path.Combine(_environment.WebRootPath, "books");
            _coverBasePath = Path.Combine(_environment.WebRootPath, "images/covers");
            _allowedBookExtensions = new[] { ".pdf", ".epub", ".mobi" };
            _allowedImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

            // Set size limits - 50MB for books, 2MB for images
            _maxBookFileSize = 52428800; // 50MB
            _maxCoverImageSize = 2097152; // 2MB

            // Ensure directories exist
            Directory.CreateDirectory(_bookBasePath);
            Directory.CreateDirectory(_coverBasePath);
        }

        public async Task<string> SaveBookAsync(IFormFile file, int productId)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty or null", nameof(file));
            }

            // Validate file size
            if (file.Length > _maxBookFileSize)
            {
                throw new ArgumentException($"File size exceeds maximum allowed size of {_maxBookFileSize / 1024 / 1024}MB", nameof(file));
            }

            // Get file extension and validate
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!IsValidFileExtension(fileExtension, _allowedBookExtensions))
            {
                throw new ArgumentException($"File extension '{fileExtension}' is not allowed. Allowed extensions: {string.Join(", ", _allowedBookExtensions)}", nameof(file));
            }

            // Generate file name based on product ID and original extension
            var fileName = $"book_{productId}{fileExtension}";
            var filePath = Path.Combine(_bookBasePath, fileName);

            // Save the file
            _logger.LogInformation($"Saving book file for product ID: {productId} to {filePath}");
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }

        public async Task<Stream> GetBookContentAsync(int productId)
        {
            // Find the book file for this product
            var directory = new DirectoryInfo(_bookBasePath);
            var bookFile = directory.GetFiles($"book_{productId}.*").FirstOrDefault();

            if (bookFile == null || !bookFile.Exists)
            {
                _logger.LogWarning($"Book file not found for product ID: {productId}");
                return null;
            }

            _logger.LogInformation($"Retrieving book content for product ID: {productId} from {bookFile.FullName}");

            // Open the file as a read-only stream
            return new FileStream(bookFile.FullName, FileMode.Open, FileAccess.Read);
        }

        public async Task DeleteBookAsync(int productId)
        {
            // Find the book file for this product
            var directory = new DirectoryInfo(_bookBasePath);
            var bookFile = directory.GetFiles($"book_{productId}.*").FirstOrDefault();

            if (bookFile == null || !bookFile.Exists)
            {
                _logger.LogWarning($"Book file not found for deletion for product ID: {productId}");
                return;
            }

            _logger.LogInformation($"Deleting book file for product ID: {productId} at {bookFile.FullName}");

            // Delete the file
            bookFile.Delete();

            // Return a completed task
            await Task.CompletedTask;
        }

        public async Task<string> SaveBookCoverAsync(IFormFile file, int productId)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty or null", nameof(file));
            }

            // Validate file size
            if (file.Length > _maxCoverImageSize)
            {
                throw new ArgumentException($"Image size exceeds maximum allowed size of {_maxCoverImageSize / 1024}KB", nameof(file));
            }

            // Get file extension and validate
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!IsValidFileExtension(fileExtension, _allowedImageExtensions))
            {
                throw new ArgumentException($"Image extension '{fileExtension}' is not allowed. Allowed extensions: {string.Join(", ", _allowedImageExtensions)}", nameof(file));
            }

            // Generate file name based on product ID and original extension
            var fileName = $"cover_{productId}{fileExtension}";
            var filePath = Path.Combine(_coverBasePath, fileName);

            // Save the file
            _logger.LogInformation($"Saving book cover for product ID: {productId} to {filePath}");
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }

        public async Task<Stream> GetBookCoverAsync(int productId)
        {
            // Find the cover image for this product
            var directory = new DirectoryInfo(_coverBasePath);
            var coverFile = directory.GetFiles($"cover_{productId}.*").FirstOrDefault();

            if (coverFile == null || !coverFile.Exists)
            {
                _logger.LogWarning($"Cover image not found for product ID: {productId}");

                // Return the default "no image" cover
                var defaultImagePath = Path.Combine(_coverBasePath, "noimage.png");
                if (File.Exists(defaultImagePath))
                {
                    return new FileStream(defaultImagePath, FileMode.Open, FileAccess.Read);
                }

                return null;
            }

            _logger.LogInformation($"Retrieving book cover for product ID: {productId} from {coverFile.FullName}");

            // Open the file as a read-only stream
            return new FileStream(coverFile.FullName, FileMode.Open, FileAccess.Read);
        }

        public async Task DeleteBookCoverAsync(int productId)
        {
            // Find the cover image for this product
            var directory = new DirectoryInfo(_coverBasePath);
            var coverFile = directory.GetFiles($"cover_{productId}.*").FirstOrDefault();

            if (coverFile == null || !coverFile.Exists)
            {
                _logger.LogWarning($"Cover image not found for deletion for product ID: {productId}");
                return;
            }

            _logger.LogInformation($"Deleting book cover for product ID: {productId} at {coverFile.FullName}");

            // Delete the file
            coverFile.Delete();

            // Return a completed task
            await Task.CompletedTask;
        }

        // Private helper methods
        private bool IsValidFileExtension(string fileExtension, string[] allowedExtensions)
        {
            return allowedExtensions.Contains(fileExtension.ToLowerInvariant());
        }
    }
}