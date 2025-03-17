// BookmarkViewModel.cs
// View model for bookmarks in e-books

using System;
using System.ComponentModel.DataAnnotations;

namespace BookShop.ViewModels
{
    public class BookmarkViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; }
        
        [Display(Name = "Page")]
        public int PageNumber { get; set; }
        
        [Display(Name = "Bookmark Name")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
        public string Name { get; set; }
        
        [Display(Name = "Created")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime CreatedDate { get; set; }
    }
}
