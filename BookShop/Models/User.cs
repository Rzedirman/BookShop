﻿using System;
using System.Collections.Generic;

namespace BookShop.Models
{
    public partial class User
    {
        public User()
        {
            Carts = new HashSet<Cart>();
            Orders = new HashSet<Order>();
            ProductsAsSeller = new HashSet<Product>();
        }

        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string Name { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public DateTime BirthDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Password { get; set; } = null!;
        public decimal Balance { get; set; } = 0;

        public virtual Role Role { get; set; } = null!;
        public virtual ICollection<Cart> Carts { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Favorite> Favorites { get; set; } = new HashSet<Favorite>();
        public virtual ICollection<Product> ProductsAsSeller { get; set; }
    }
}
