using System;
using System.Collections.Generic;

namespace BookShop.Models
{
    public partial class Product
    {
        public Product()
        {
            Carts = new HashSet<Cart>();
            Orders = new HashSet<Order>();
        }

        public int ProductId { get; set; }
        public int AutorId { get; set; }
        public int GenreId { get; set; }
        public int LanguageId { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public int InStock { get; set; }
        public DateTime PublicationDate { get; set; }
        public string ImageName { get; set; } = null!;
        public string? FileName { get; set; }

        public virtual Autor Autor { get; set; } = null!;
        public virtual Genre Genre { get; set; } = null!;
        public virtual Language Language { get; set; } = null!;
        public virtual ICollection<Cart> Carts { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}
