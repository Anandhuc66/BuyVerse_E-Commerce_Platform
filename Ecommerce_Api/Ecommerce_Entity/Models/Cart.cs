using System;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce_Entity.Models
{
    public class Cart
    {
        [Key]
        public int CartId { get; set; }

        // Foreign Keys
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int Quantity { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
