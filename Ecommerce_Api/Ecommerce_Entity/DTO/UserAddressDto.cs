using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce_Entity.DTO
{
    public class UserAddressDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }

        public string FullAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? Country { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class UserAddressCreateDto
    {
        public string UserId { get; set; }

        [Required]
        [MaxLength(500)]
        public string FullAddress { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(100)]
        public string? State { get; set; }

        [MaxLength(20)]
        public string? ZipCode { get; set; }

        [MaxLength(100)]
        public string? Country { get; set; }
    }

    public class UserAddressUpdateDto : UserAddressCreateDto
    {
        public int Id { get; set; }
    }
}

