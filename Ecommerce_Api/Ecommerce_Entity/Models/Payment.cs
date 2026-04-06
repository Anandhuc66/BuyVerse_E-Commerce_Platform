using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce_Entity.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string PaymentMethod { get; set; } // e.g. "Card", "UPI", "COD"

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        public string TransactionId { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Completed, Failed

        // Foreign Key
        [Required]
        public int OrderId { get; set; }
        public Order Order { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
