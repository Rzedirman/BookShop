// ViewModels/CustomerProfileViewModel.cs
// View model for customer profile page with wallet information

using BookShop.Models;
using System.ComponentModel.DataAnnotations;

namespace BookShop.ViewModels
{
    /// <summary>
    /// View model for customer profile page including user info and wallet balance
    /// </summary>
    public class CustomerProfileViewModel
    {
        // User Information
        public User UserInfo { get; set; }

        // Wallet Information
        [Display(Name = "Current Balance")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal WalletBalance { get; set; }

        // Top-up amount input
        [Display(Name = "Top-up Amount")]
        [Range(1, 10000, ErrorMessage = "Top-up amount must be between $1 and $10,000")]
        [DataType(DataType.Currency)]
        public decimal TopUpAmount { get; set; }
    }
}