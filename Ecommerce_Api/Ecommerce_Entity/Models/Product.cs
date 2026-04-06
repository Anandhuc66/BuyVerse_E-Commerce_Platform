using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce_Entity.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required, MaxLength(50)]
        public string SKU { get; set; } // Unique product code

        public int StockQuantity { get; set; }

        // Foreign Keys
        public int SubCategoryId { get; set; }
        public SubCategory SubCategory { get; set; }

        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; }

        // Navigation
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        [NotMapped]
        public int CategoryId => SubCategory?.CategoryId ?? 0;
    }
}
