using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce_Entity.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string OrderNumber { get; set; }

        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required, MaxLength(20)]
        public string Status { get; set; } = "Pending";  // Payment status: Pending, Paid, COD

        [Required, MaxLength(20)]
        public string DeliveryStatus { get; set; } = "Pending";  // Delivery: Pending, Processing, Shipped, Delivered, Cancelled

        // Use address id to reference stored address
        public int? ShippingAddressId { get; set; }
        public UserAddress ShippingAddress { get; set; }

        // Navigation
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();  // PREVENT NULL ISSUES
        public Payment Payment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
