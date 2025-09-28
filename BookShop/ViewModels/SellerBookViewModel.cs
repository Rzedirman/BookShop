// ViewModels/SellerViewModels.cs
// View models for seller area functionality

using BookShop.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookShop.ViewModels
{
    /// <summary>
    /// View model for seller's book management with enhanced details
    /// </summary>
    public class SellerBookViewModel : BookViewModel
    {
        [Display(Name = "Total Sales")]
        public int TotalSales { get; set; }

        [Display(Name = "Total Earnings")]
        [DataType(DataType.Currency)]
        public decimal TotalEarnings { get; set; }

        [Display(Name = "Last Sold")]
        [DataType(DataType.Date)]
        public DateTime? LastSoldDate { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } // Available, Out of Stock, etc.
    }
}