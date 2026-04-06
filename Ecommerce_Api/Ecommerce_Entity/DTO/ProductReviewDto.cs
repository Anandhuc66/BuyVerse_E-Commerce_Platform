using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce_Entity.DTO
{
    public class ProductReviewDto
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public string ProductName { get; set; }

        public string UserId { get; set; }
        public string UserName { get; set; }

        public int Rating { get; set; }
        public string? Title { get; set; }
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class ProductReviewCreateDto
    {
        [Required]
        public int ProductId { get; set; }

        public string UserId { get; set; } // Overridden by server from JWT

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }

        [MaxLength(200)]
        public string? Title { get; set; }

        [MaxLength(2000)]
        public string? Comment { get; set; }
    }

    public class ProductReviewUpdateDto : ProductReviewCreateDto
    {
        public int Id { get; set; }
    }
}

