// ViewModels/LibraryBookViewModel.cs
// View model for books in user's library with purchase information

using System;
using System.ComponentModel.DataAnnotations;

namespace BookShop.ViewModels
{
    /// <summary>
    /// View model for displaying owned books in user's library
    /// </summary>
    public class LibraryBookViewModel
    {
        public int ProductId { get; set; }

        [Display(Name = "Title")]
        public string Title { get; set; }

        [Display(Name = "Author")]
        public string AuthorName { get; set; }

        [Display(Name = "Cover Image")]
        public string ImageName { get; set; }

        [Display(Name = "Purchase Date")]
        [DisplayFormat(DataFormatString = "{0:MMMM dd, yyyy}")]
        public DateTime PurchaseDate { get; set; }

        public int OrderId { get; set; }
    }
}