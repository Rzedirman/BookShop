// FilterParameters.cs
// Parameters for filtering books in the catalog

using System;
using System.Collections.Generic;

namespace BookShop.ViewModels
{
    public class FilterParameters
    {
        // Search term
        public string SearchTerm { get; set; }
        
        // Category filters
        public int? GenreId { get; set; }
        public int? AuthorId { get; set; }
        public int? LanguageId { get; set; }
        
        // Price range
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        
        // Date range
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        
        // Sorting
        public string SortBy { get; set; } = "Title"; // Default sort by title
        public bool SortAscending { get; set; } = true; // Default ascending order
        
        // Pagination
        public int Page { get; set; } = 1; // Default to first page
        public int PageSize { get; set; } = 10; // Default page size
    }
}
