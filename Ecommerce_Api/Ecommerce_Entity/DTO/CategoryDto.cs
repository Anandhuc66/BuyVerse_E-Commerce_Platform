using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce_Entity.DTO
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        //public List<SubCategoryDto> SubCategories { get; set; }
    }
    public class CategoryCreateDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }
    }

    public class CategoryUpdateDto : CategoryCreateDto
    {
        [Required]
        public int Id { get; set; }
    }
}
