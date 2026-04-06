using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce_Entity.Models
{
    public class SubCategory
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        public string? Description { get; set; }

        // Foreign Key
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        // Products under this SubCategory
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }

}
