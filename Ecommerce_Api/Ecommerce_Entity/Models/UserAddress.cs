// UserAddress.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce_Entity.Models
{
    public class UserAddress
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required]
        public string FullAddress { get; set; }

        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? Country { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
