// ApplicationUser.cs
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace Ecommerce_Entity.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string Gender { get; set; }  // "Male" / "Female" / "Other"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Supplier Supplier { get; set; } // optional: supplier profile
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Cart> Carts { get; set; } = new List<Cart>();
        public ICollection<UserAddress> Addresses { get; set; } = new List<UserAddress>();
        //public ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
    }
}
