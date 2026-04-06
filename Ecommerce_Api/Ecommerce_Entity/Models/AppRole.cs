using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Ecommerce_Entity.Models
{
    public class AppRole : IdentityRole
    {
        public string? DisplayName { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }
}
