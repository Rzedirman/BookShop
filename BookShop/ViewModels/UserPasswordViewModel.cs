// Add this to your ViewModels folder - UserPasswordViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace BookShop.ViewModels
{
    public class UserPasswordViewModel
    {
        public int UserId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("NewPassword", ErrorMessage = "The passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}