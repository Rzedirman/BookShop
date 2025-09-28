// ViewModels/SellerViewModels.cs
// View models for seller area functionality

using BookShop.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookShop.ViewModels
{
    /// <summary>
    /// View model for monthly sales data for charts and reporting
    /// </summary>
    public class SellerMonthlySalesViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName => new DateTime(Year, Month, 1).ToString("MMMM");
        public int BooksSold { get; set; }
        public decimal TotalEarnings { get; set; }
        public int UniqueCustomers { get; set; }
    }
}