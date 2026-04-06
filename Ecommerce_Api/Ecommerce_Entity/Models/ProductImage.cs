using System;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce_Entity.Models
{
    public class ProductImage
    {
        [Key]
        public int Id { get; set; }

        // The actual image URL (required)
        [Required]
        public string ImageUrl { get; set; }

        // Optional text for accessibility & SEO
        public string? AltText { get; set; }

        // Controls image order (1,2,3…)
        public int SortOrder { get; set; } = 0;

        // Mark which is the main product image
        public bool IsPrimary { get; set; } = false;

        // Foreign Key
        public int ProductId { get; set; }
        public Product Product { get; set; }

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
