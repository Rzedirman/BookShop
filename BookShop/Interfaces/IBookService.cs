// IBookService.cs
// Interface for book-related operations

using BookShop.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookShop.Interfaces
{
    public interface IBookService
    {
        // Book retrieval operations
        Task<IEnumerable<BookViewModel>> GetAllBooksAsync();
        Task<BookViewModel> GetBookByIdAsync(int id);
        Task<IEnumerable<BookViewModel>> GetBooksByAuthorAsync(int authorId);
        Task<IEnumerable<BookViewModel>> GetBooksByGenreAsync(int genreId);
        Task<IEnumerable<BookViewModel>> GetBooksBySellerAsync(string sellerId);
        Task<IEnumerable<BookViewModel>> GetFeaturedBooksAsync(int count);
        Task<IEnumerable<BookViewModel>> GetNewReleasesAsync(int count);
        
        // Search and filter operations
        Task<IEnumerable<BookViewModel>> SearchBooksAsync(string term);
        Task<IEnumerable<BookViewModel>> FilterBooksAsync(FilterParameters parameters);
        
        // Book management operations
        Task<int> AddBookAsync(BookViewModel bookViewModel);
        Task UpdateBookAsync(BookViewModel bookViewModel);
        Task DeleteBookAsync(int id);
        
        // Book statistics operations
        Task<int> GetTotalBooksCountAsync();
        Task<decimal> GetAveragePriceAsync();
        
        // User-specific operations
        Task<bool> AddBookToFavoritesAsync(int bookId, string userId);
        Task<bool> RemoveBookFromFavoritesAsync(int bookId, string userId);
        Task<IEnumerable<BookViewModel>> GetUserFavoriteBooksAsync(string userId);
        Task<IEnumerable<BookViewModel>> GetUserPurchasedBooksAsync(string userId);
        
        // Bookmark operations
        Task AddBookmarkAsync(int bookId, string userId, int pageNumber, string name);
        Task<IEnumerable<BookmarkViewModel>> GetUserBookmarksAsync(string userId, int bookId);
        Task DeleteBookmarkAsync(int bookmarkId);
    }
}
