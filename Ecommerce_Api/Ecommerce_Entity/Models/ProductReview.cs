// ProductReview.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce_Entity.Models
{
    public class ProductReview
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        public string? Title { get; set; }
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
