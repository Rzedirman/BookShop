// BookViewModel.cs
// View model for book display and manipulation

using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace BookShop.ViewModels
{
    public class BookViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot be longer than 200 characters")]
        [Display(Name = "Title")]
        public string Title { get; set; }
        
        [Required(ErrorMessage = "Author is required")]
        [Display(Name = "Author")]
        public int AuthorId { get; set; }
        //public string AuthorName { get; set; }
        
        [Required(ErrorMessage = "Genre is required")]
        [Display(Name = "Genre")]
        public int GenreId { get; set; }
        //public string GenreName { get; set; }
        
        [Required(ErrorMessage = "Language is required")]
        [Display(Name = "Language")]
        public int LanguageId { get; set; }
        //public string LanguageName { get; set; }
        
        [Required(ErrorMessage = "Description is required")]
        [StringLength(2000, ErrorMessage = "Description cannot be longer than 2000 characters")]
        [Display(Name = "Description")]
        public string Description { get; set; }
        
        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 999.99, ErrorMessage = "Price must be between 0.01 and 999.99")]
        [Display(Name = "Price")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }
        
        [Display(Name = "In Stock")]
        public int InStock { get; set; }
        
        [Required(ErrorMessage = "Publication date is required")]
        [Display(Name = "Publication Date")]
        [DataType(DataType.Date)]
        public DateTime PublicationDate { get; set; }
        
        [Display(Name = "Cover Image")]
        public IFormFile CoverImage { get; set; }
        public string ImageName { get; set; }
        
        [Display(Name = "Book File")]
        public IFormFile BookFile { get; set; }
        public string FilePath { get; set; }
        public long FileSize { get; set; }
        
        [Display(Name = "Seller")]
        public string SellerId { get; set; }
        //public string SellerName { get; set; }
        
        // Additional display properties
        public decimal AverageRating { get; set; }
        public int ReviewsCount { get; set; }
        public int SalesCount { get; set; }
        public bool IsFavorite { get; set; }
    }
}
