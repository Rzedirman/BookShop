using System;
using System.Collections.Generic;

namespace BookShop.Models
{
    public partial class Autor
    {
        public Autor()
        {
            Products = new HashSet<Product>();
        }

        public int AutorId { get; set; }
        public string Name { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Country { get; set; } = null!;
        public DateTime BirthDate { get; set; }
        public DateTime? DeathDate { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }
}
