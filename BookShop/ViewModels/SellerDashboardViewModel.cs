// ViewModels/SellerViewModels.cs
// View models for seller area functionality

using BookShop.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookShop.ViewModels
{
    /// <summary>
    /// View model for seller dashboard with statistics and recent activity
    /// </summary>
    public class SellerDashboardViewModel
    {
        public int TotalBooks { get; set; }
        public int TotalBooksSold { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal CurrentUserBalance { get; set; }
        public List<Order> RecentOrders { get; set; } = new List<Order>();
        public List<TopSellingBookViewModel> TopSellingBooks { get; set; } = new List<TopSellingBookViewModel>();
    }
}