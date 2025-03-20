using System.ComponentModel.DataAnnotations;

namespace BookShop.ViewModels
{
    public class CreateUserViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "LastName")]
        public string LastName { get; set; }

        [Display(Name = "Phone")]
        public string? Phone { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "BirthDate")]
        public DateTime BirthDate { get; set; }

        [Display(Name = "I want to sell books on this platform")]
        public bool IsSeller { get; set; } = false;
    }
}
