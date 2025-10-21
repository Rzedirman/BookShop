// Classes/OrderService.cs
// Implementation of order processing and management

using BookShop.Interfaces;
using BookShop.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShop.Classes
{
    /// <summary>
    /// Service for processing and managing orders
    /// </summary>
    public class OrderService : IOrderService
    {
        private readonly myShopContext _context;
        private readonly ILogger<OrderService> _logger;

        public OrderService(myShopContext context, ILogger<OrderService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Order> CreateOrderAsync(int userId, int productId, decimal price)
        {
            try
            {
                _logger.LogInformation($"Creating order for user ID: {userId}, product ID: {productId}");

                // Verify product exists
                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                {
                    _logger.LogWarning($"Product ID: {productId} not found");
                    return null;
                }

                // Check if user already owns this book
                var alreadyOwned = await _context.Orders
                    .AnyAsync(o => o.UserId == userId && o.ProductId == productId);

                if (alreadyOwned)
                {
                    _logger.LogWarning($"User ID: {userId} already owns product ID: {productId}");
                    return null;
                }

                // Create order
                var order = new Order
                {
                    UserId = userId,
                    ProductId = productId,
                    Amount = 1, // Digital books always have quantity 1
                    TotalPrice = price,
                    OrderDate = DateTime.Now,
                    DeliveryAddress = "Digital Download" // For digital products
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Order created successfully. Order ID: {order.OrderId}");
                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating order for user ID: {userId}, product ID: {productId}");
                throw;
            }
        }

        public async Task<List<Order>> CreateBulkOrderAsync(int userId, List<Cart> cartItems)
        {
            try
            {
                _logger.LogInformation($"Creating bulk order for user ID: {userId} with {cartItems.Count} items");

                var orders = new List<Order>();

                foreach (var cartItem in cartItems)
                {
                    // Load product details
                    var product = await _context.Products.FindAsync(cartItem.ProductId);
                    if (product == null)
                    {
                        _logger.LogWarning($"Product ID: {cartItem.ProductId} not found, skipping");
                        continue;
                    }

                    // Check if user already owns this book
                    var alreadyOwned = await _context.Orders
                        .AnyAsync(o => o.UserId == userId && o.ProductId == cartItem.ProductId);

                    if (alreadyOwned)
                    {
                        _logger.LogWarning($"User ID: {userId} already owns product ID: {cartItem.ProductId}, skipping");
                        continue;
                    }

                    // Create order for this item
                    var order = new Order
                    {
                        UserId = userId,
                        ProductId = cartItem.ProductId,
                        Amount = 1, // Digital books always have quantity 1
                        TotalPrice = product.Price,
                        OrderDate = DateTime.Now,
                        DeliveryAddress = "Digital Download"
                    };

                    _context.Orders.Add(order);
                    orders.Add(order);
                }

                if (orders.Any())
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Bulk order created successfully. {orders.Count} orders created for user ID: {userId}");
                }
                else
                {
                    _logger.LogWarning($"No orders created for user ID: {userId}. All items may already be owned or invalid");
                }

                return orders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating bulk order for user ID: {userId}");
                throw;
            }
        }

        public async Task<IEnumerable<Order>> GetUserOrdersAsync(int userId)
        {
            try
            {
                _logger.LogInformation($"Retrieving orders for user ID: {userId}");

                var orders = await _context.Orders
                    .Where(o => o.UserId == userId)
                    .Include(o => o.Product)
                        .ThenInclude(p => p.Author)
                    .Include(o => o.Product)
                        .ThenInclude(p => p.Genre)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                _logger.LogInformation($"Found {orders.Count} orders for user ID: {userId}");
                return orders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving orders for user ID: {userId}");
                throw;
            }
        }

        public async Task<Order> GetOrderDetailsAsync(int orderId)
        {
            try
            {
                _logger.LogInformation($"Retrieving order details for order ID: {orderId}");

                var order = await _context.Orders
                    .Include(o => o.Product)
                        .ThenInclude(p => p.Author)
                    .Include(o => o.Product)
                        .ThenInclude(p => p.Genre)
                    .Include(o => o.Product)
                        .ThenInclude(p => p.Language)
                    .Include(o => o.User)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null)
                {
                    _logger.LogWarning($"Order ID: {orderId} not found");
                    return null;
                }

                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving order details for order ID: {orderId}");
                throw;
            }
        }
    }
}