// ViewModels/SellerViewModels.cs
// View models for seller area functionality

using BookShop.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookShop.ViewModels
{
    /// <summary>
    /// View model for book creation with author and category management
    /// </summary>
    public class CreateBookViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot be longer than 200 characters")]
        [Display(Name = "Book Title")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(2000, ErrorMessage = "Description cannot be longer than 2000 characters")]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 999.99, ErrorMessage = "Price must be between $0.01 and $999.99")]
        [Display(Name = "Price ($)")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stock quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Stock must be at least 1")]
        [Display(Name = "In Stock")]
        public int InStock { get; set; }

        [Required(ErrorMessage = "Publication date is required")]
        [Display(Name = "Publication Date")]
        [DataType(DataType.Date)]
        public DateTime PublicationDate { get; set; }

        // Author Information
        [Display(Name = "Existing Author")]
        public int? AuthorId { get; set; }

        [Display(Name = "Author First Name")]
        [StringLength(100)]
        public string AuthorFirstName { get; set; }

        [Display(Name = "Author Last Name")]
        [StringLength(100)]
        public string AuthorLastName { get; set; }

        [Display(Name = "Author Country")]
        [StringLength(50)]
        public string AuthorCountry { get; set; }

        [Display(Name = "Author Birth Date")]
        [DataType(DataType.Date)]
        public DateTime? AuthorBirthDate { get; set; }

        [Display(Name = "Author Death Date")]
        [DataType(DataType.Date)]
        public DateTime? AuthorDeathDate { get; set; }

        // Category Information
        [Display(Name = "Existing Genre")]
        public int? GenreId { get; set; }

        [Display(Name = "New Genre Name")]
        [StringLength(250)]
        public string NewGenreName { get; set; }

        [Display(Name = "Existing Language")]
        public int? LanguageId { get; set; }

        [Display(Name = "New Language Name")]
        [StringLength(100)]
        public string NewLanguageName { get; set; }

        // File Uploads
        [Display(Name = "Book Cover")]
        public Microsoft.AspNetCore.Http.IFormFile CoverImage { get; set; }

        [Required(ErrorMessage = "Book file is required")]
        [Display(Name = "Book File (PDF, EPUB, MOBI)")]
        public Microsoft.AspNetCore.Http.IFormFile BookFile { get; set; }

        // Helper properties for validation
        public bool IsCreatingNewAuthor => AuthorId == null || AuthorId == 0;
        public bool IsCreatingNewGenre => GenreId == null || GenreId == 0;
        public bool IsCreatingNewLanguage => LanguageId == null || LanguageId == 0;
    }
    
}