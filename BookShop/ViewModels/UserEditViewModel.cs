using System;
using System.ComponentModel.DataAnnotations;

namespace BookShop.ViewModels
{
    public class UserEditViewModel
    {
        public int UserId { get; set; }

        [Required]
        [Display(Name = "First Name")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(100, ErrorMessage = "Last name cannot be longer than 100 characters")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        [StringLength(150, ErrorMessage = "Email cannot be longer than 150 characters")]
        public string Email { get; set; }

        [Display(Name = "Phone Number")]
        [StringLength(20, ErrorMessage = "Phone number cannot be longer than 20 characters")]
        [RegularExpression(@"^[0-9\+\-\(\)\s]*$", ErrorMessage = "Please enter a valid phone number")]
        public string Phone { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Birth Date")]
        public DateTime BirthDate { get; set; }

        [Required]
        [Display(Name = "Role")]
        public int RoleId { get; set; }
    }
}