// DashboardViewModel.cs - View model for admin dashboard statistics
using BookShop.Models;
using System;
using System.Collections.Generic;

namespace BookShop.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalBooks { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSales { get; set; }
        public List<Order> RecentOrders { get; set; }
        public List<TopSellingBookViewModel> TopSellingBooks { get; set; }
    }

    public class TopSellingBookViewModel
    {
        public int ProductId { get; set; }
        public string Title { get; set; }
        public int SalesCount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class MonthlySalesViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName => new DateTime(Year, Month, 1).ToString("MMMM");
        public int OrderCount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class SalesByGenreViewModel
    {
        public string GenreName { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class ReportsViewModel
    {
        public List<MonthlySalesViewModel> MonthlySales { get; set; }
        public List<SalesByGenreViewModel> SalesByGenre { get; set; }
    }
}