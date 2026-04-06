using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce_Entity.DTO
{
    public class ProductDto
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public decimal Price { get; set; }
        public string SKU { get; set; }

        public int StockQuantity { get; set; }

        // SubCategory
        public int SubCategoryId { get; set; }
        public string SubCategoryName { get; set; }

        // Parent Category
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }

        // Supplier
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }

        public List<string> ImageUrls { get; set; } = new();

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class ProductCreateDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(2000)]
        public string Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        [MaxLength(50)]
        public string SKU { get; set; }

        [Required]
        public int SubCategoryId { get; set; }

        [Required]
        public int SupplierId { get; set; }
    }

    public class ProductUpdateDto : ProductCreateDto
    {
        [Required]
        public int Id { get; set; }
    }

    public class ProductWithImagesCreateDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(2000)]
        public string Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        [MaxLength(50)]
        public string SKU { get; set; }

        [Required]
        public int SubCategoryId { get; set; }

        [Required]
        public int SupplierId { get; set; }

        public List<IFormFile>? Images { get; set; }
    }
}
