// ViewModels/CartViewModels.cs
// View models for shopping cart functionality

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookShop.ViewModels
{
    /// <summary>
    /// View model for shopping cart with items and totals
    /// </summary>
    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();

        [Display(Name = "Total Price")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalPrice { get; set; }

        [Display(Name = "Items in Cart")]
        public int ItemCount { get; set; }
    }

    /// <summary>
    /// View model for individual cart item
    /// </summary>
    public class CartItemViewModel
    {
        public int CartId { get; set; }

        public int ProductId { get; set; }

        [Display(Name = "Title")]
        public string Title { get; set; }

        [Display(Name = "Author")]
        public string AuthorName { get; set; }

        [Display(Name = "Price")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Price { get; set; }

        [Display(Name = "Cover")]
        public string ImageName { get; set; }
    }
}