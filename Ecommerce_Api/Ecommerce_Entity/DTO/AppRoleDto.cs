using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce_Entity.DTO
{
    public class AppRoleDto
    {
        public string Id { get; set; }
        public string Name { get; set; }            // REQUIRED
        public string DisplayName { get; set; }
        public string Description { get; set; }
    }

    public class AppRoleCreateDto
    {
        public string Name { get; set; }            // REQUIRED
        public string DisplayName { get; set; }     // Optional UI label
        public string Description { get; set; }
    }

    public class AppRoleUpdateDto
    {
        public string Id { get; set; }              // REQUIRED
        public string Name { get; set; }            // REQUIRED
        public string DisplayName { get; set; }
        public string Description { get; set; }
    }
}

