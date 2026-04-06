using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce_Entity.DTO
{
    public class SubCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }

        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
    }

    public class SubCategoryCreateDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public int CategoryId { get; set; }
    }

    public class SubCategoryUpdateDto : SubCategoryCreateDto
    {
        [Required]
        public int Id { get; set; }
    }
}
