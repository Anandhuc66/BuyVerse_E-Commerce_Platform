using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce_Entity.DTO
{
    public class CartDto
    {
        public int CartId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }

        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }

        public int Quantity { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CartCreateDto
    {
        public string? UserId { get; set; } // Overridden by server from JWT

        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, 10000, ErrorMessage = "Quantity must be between 1 and 10000.")]
        public int Quantity { get; set; }
    }

    public class CartUpdateDto
    {
        [Required]
        public int CartId { get; set; }

        [Required]
        [Range(1, 10000, ErrorMessage = "Quantity must be between 1 and 10000.")]
        public int Quantity { get; set; }   // Only this should update
    }

    public class CartItemWithUserDto
    {
        // Cart
        public int CartId { get; set; }
        public int Quantity { get; set; }
        public DateTime CartCreatedAt { get; set; }

        // User
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Gender { get; set; }
        public DateTime UserCreatedAt { get; set; }
        public int? SupplierId { get; set; }
        public string? SupplierName { get; set; }

        // Product
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }

        // Category (via SubCategory)
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }

        // Supplier
        public int ProductSupplierId { get; set; }
        public string ProductSupplierName { get; set; }

        // Images
        public List<string> ImageUrls { get; set; }
    }
}
