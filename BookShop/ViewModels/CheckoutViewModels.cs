// ViewModels/CheckoutViewModels.cs
// View models for checkout process

using BookShop.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookShop.ViewModels
{
    /// <summary>
    /// View model for checkout page with cart review and wallet check
    /// </summary>
    public class CheckoutViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();

        [Display(Name = "Total Price")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalPrice { get; set; }

        [Display(Name = "Wallet Balance")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal WalletBalance { get; set; }

        public bool SufficientFunds => WalletBalance >= TotalPrice;
    }

    /// <summary>
    /// View model for order confirmation/success page
    /// </summary>
    public class OrderConfirmationViewModel
    {
        public List<Order> Orders { get; set; } = new List<Order>();

        [Display(Name = "Total Amount Paid")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalPaid { get; set; }

        [Display(Name = "Order Date")]
        [DisplayFormat(DataFormatString = "{0:MMMM dd, yyyy 'at' HH:mm}")]
        public DateTime OrderDate { get; set; }

        [Display(Name = "Remaining Balance")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal RemainingBalance { get; set; }
    }
}