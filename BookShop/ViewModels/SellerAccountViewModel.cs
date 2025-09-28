// ViewModels/SellerViewModels.cs
// View models for seller area functionality

using BookShop.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookShop.ViewModels
{
    /// <summary>
    /// View model for seller account information and editing
    /// </summary>
    public class SellerAccountViewModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
        [Display(Name = "First Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100, ErrorMessage = "Last name cannot be longer than 100 characters")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [StringLength(150, ErrorMessage = "Email cannot be longer than 150 characters")]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [StringLength(20, ErrorMessage = "Phone number cannot be longer than 20 characters")]
        [RegularExpression(@"^[0-9\+\-\(\)\s]*$", ErrorMessage = "Please enter a valid phone number")]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Birth date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Birth Date")]
        public DateTime BirthDate { get; set; }

        [Display(Name = "Account Balance")]
        [DataType(DataType.Currency)]
        public decimal Balance { get; set; }

        [Display(Name = "Member Since")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Role")]
        public string RoleName { get; set; }
    }
}