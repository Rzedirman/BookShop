using System;
using System.Collections.Generic;

namespace BookShop.Models
{
    public partial class Language
    {
        public Language()
        {
            Products = new HashSet<Product>();
        }

        public int LanguageId { get; set; }
        public string LanguageName { get; set; } = null!;

        public virtual ICollection<Product> Products { get; set; }
    }
}
