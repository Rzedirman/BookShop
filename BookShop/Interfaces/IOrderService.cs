// Interfaces/IOrderService.cs
// Interface for order processing and management

using BookShop.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookShop.Interfaces
{
    /// <summary>
    /// Service interface for order processing and management
    /// </summary>
    public interface IOrderService
    {
        /// <summary>
        /// Creates a single order for a product
        /// </summary>
        Task<Order> CreateOrderAsync(int userId, int productId, decimal price);

        /// <summary>
        /// Creates multiple orders from cart items (bulk order)
        /// </summary>
        Task<List<Order>> CreateBulkOrderAsync(int userId, List<Cart> cartItems);

        /// <summary>
        /// Retrieves all orders for a specific user
        /// </summary>
        Task<IEnumerable<Order>> GetUserOrdersAsync(int userId);

        /// <summary>
        /// Retrieves details of a specific order
        /// </summary>
        Task<Order> GetOrderDetailsAsync(int orderId);
    }
}