// ViewModels/SalesReportViewModels.cs
// View models for seller sales reporting functionality

using BookShop.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookShop.ViewModels
{
    /// <summary>
    /// Comprehensive sales report with multiple data breakdowns
    /// </summary>
    public class SalesReportViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ReportType { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalBooksSold { get; set; }
        public int UniqueCustomers { get; set; }
        public List<DailySalesData> DailySales { get; set; } = new List<DailySalesData>();
        public List<BookPerformanceData> TopBooks { get; set; } = new List<BookPerformanceData>();
        public List<GenrePerformanceData> GenrePerformance { get; set; } = new List<GenrePerformanceData>();
    }

    /// <summary>
    /// Daily sales data for charts and trends
    /// </summary>
    public class DailySalesData
    {
        public DateTime Date { get; set; }
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
        public int BooksSold { get; set; }
    }

    /// <summary>
    /// Book performance metrics for reporting
    /// </summary>
    public class BookPerformanceData
    {
        public Product Book { get; set; }
        public int OrderCount { get; set; }
        public int BooksSold { get; set; }
        public decimal Revenue { get; set; }
    }

    /// <summary>
    /// Genre performance metrics for reporting
    /// </summary>
    public class GenrePerformanceData
    {
        public string GenreName { get; set; }
        public int OrderCount { get; set; }
        public int BooksSold { get; set; }
        public decimal Revenue { get; set; }
    }

    /// <summary>
    /// Comprehensive sales overview for seller dashboard
    /// </summary>
    public class SellerSalesOverviewViewModel
    {
        public decimal TotalEarnings { get; set; }
        public int TotalBooksSold { get; set; }
        public int TotalOrders { get; set; }
        public int UniqueCustomers { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal ThisMonthSales { get; set; }
        public decimal LastMonthSales { get; set; }
        public decimal SalesGrowthPercentage { get; set; }
        public List<SellerMonthlySalesViewModel> MonthlySalesData { get; set; } = new List<SellerMonthlySalesViewModel>();
        public List<TopSellingBookViewModel> TopSellingBooks { get; set; } = new List<TopSellingBookViewModel>();
        public List<Order> RecentOrders { get; set; } = new List<Order>();
    }

    /// <summary>
    /// Detailed analytics for individual book performance
    /// </summary>
    public class BookAnalyticsViewModel
    {
        public Product Book { get; set; }
        public int TotalSales { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int UniqueCustomers { get; set; }
        public decimal AverageOrderSize { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<SellerMonthlySalesViewModel> MonthlyPerformance { get; set; } = new List<SellerMonthlySalesViewModel>();
        public List<Order> RecentOrders { get; set; } = new List<Order>();
        public dynamic TopCustomers { get; set; }
    }
}