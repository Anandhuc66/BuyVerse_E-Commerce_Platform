using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce_Entity.Models
{
    public class Supplier
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string CompanyName { get; set; }

        [EmailAddress]
        public string ContactEmail { get; set; }

        [Phone]
        public string Phone { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }

        // Relationship back to ApplicationUser
        public string? UserId { get; set; }
        public ApplicationUser User { get; set; }

        // Navigation
        public ICollection<Product> Products { get; set; }
    }
}
