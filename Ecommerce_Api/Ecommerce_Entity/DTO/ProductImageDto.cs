using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Ecommerce_Entity.DTO
{
    public class ProductImageDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ImageUrl { get; set; }
        public bool IsPrimary { get; set; }
    }

    // Single file upload
    public class ProductImageCreateDto
    {
        public int ProductId { get; set; }
        public IFormFile Image { get; set; }
        public bool IsPrimary { get; set; }
    }

    // Update (URL allowed)
    public class ProductImageUpdateDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ImageUrl { get; set; }
        public bool IsPrimary { get; set; }
    }

    // Multi-image upload
    public class ProductImageUploadDto
    {
        public int ProductId { get; set; }
        public List<IFormFile> Images { get; set; } = new();
        public int PrimaryIndex { get; set; } = 0;
    }
}
