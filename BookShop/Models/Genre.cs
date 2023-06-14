using System;
using System.Collections.Generic;

namespace BookShop.Models
{
    public partial class Genre
    {
        public Genre()
        {
            Products = new HashSet<Product>();
        }

        public int GenreId { get; set; }
        public string GenreName { get; set; } = null!;

        public virtual ICollection<Product> Products { get; set; }
    }
}
