// ViewModels/BookCatalogViewModels.cs
// View models for public book catalog and detail pages

using BookShop.Helpers;
using BookShop.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookShop.ViewModels
{
    /// <summary>
    /// View model for home page book catalog with bestsellers and filters
    /// </summary>
    public class BookCatalogViewModel
    {
        // Bestselling books section
        public List<Product> Bestsellers { get; set; } = new List<Product>();

        // Main book catalog with pagination
        public PaginatedList<Product> AllBooks { get; set; }

        // Current filter settings
        public FilterParameters CurrentFilters { get; set; } = new FilterParameters();

        // Search term
        [Display(Name = "Search")]
        public string SearchTerm { get; set; }

        // Available filter options for dropdowns
        public List<Genre> AvailableGenres { get; set; } = new List<Genre>();
        public List<Author> AvailableAuthors { get; set; } = new List<Author>();
        public List<Language> AvailableLanguages { get; set; } = new List<Language>();
    }

    /// <summary>
    /// View model for book detail page (public view)
    /// </summary>
    public class BookDetailViewModel
    {
        // Book information
        public Product Book { get; set; }

        // User-specific information (for authenticated users)
        public bool IsOwned { get; set; }
        public bool IsFavorite { get; set; }
        public bool IsInCart { get; set; }

        // Related books (same author or genre)
        public List<Product> RelatedBooks { get; set; } = new List<Product>();
    }
}