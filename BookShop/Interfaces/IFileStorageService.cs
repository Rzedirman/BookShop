// IFileStorageService.cs
// Interface for file storage operations (books and images)

using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace BookShop.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveBookAsync(IFormFile file, int productId);
        Task<Stream> GetBookContentAsync(int productId);
        Task DeleteBookAsync(int productId);
        
        Task<string> SaveBookCoverAsync(IFormFile file, int productId);
        Task<Stream> GetBookCoverAsync(int productId);
        Task DeleteBookCoverAsync(int productId);
    }
}
