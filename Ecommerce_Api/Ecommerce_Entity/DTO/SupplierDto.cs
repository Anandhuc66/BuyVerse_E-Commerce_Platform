    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    namespace Ecommerce_Entity.DTO
    {
    public class SupplierDto
    {
        public int SupplierId { get; set; }
        public string CompanyName { get; set; }
        public string ContactEmail { get; set; }
        public string Phone { get; set; }

        public string UserId { get; set; }
        public string FullName { get; set; }     // from ApplicationUser
        public string Gender { get; set; }       // from ApplicationUser

        public List<ProductDto> Products { get; set; }
    }

    public class SupplierCreateDto
    {
        [Required]
        [MaxLength(200)]
        public string CompanyName { get; set; }

        [Required]
        [EmailAddress]
        public string ContactEmail { get; set; }

        [Required]
        public string Phone { get; set; }

        // REMOVE FullName
        [Required]
        public string Gender { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }
    }

    public class SupplierUpdateDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string CompanyName { get; set; }

        [Required]
        [EmailAddress]
        public string ContactEmail { get; set; }

        [Required]
        public string Phone { get; set; }
    }

}
