using Ecommerce_Common;
using Ecommerce_Entity.Data;
using Ecommerce_Entity.DTO;
using Ecommerce_Entity.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ecommerce_Service.Repository
{
    public class ProductImageRepo : IProductImageRepo
    {
        private readonly ApplicationDbContext _context;

        public ProductImageRepo(ApplicationDbContext context) => _context = context;

        // ===============================
        // Get All
        // ===============================
        public async Task<Result<List<ProductImage>>> GetAll()
        {
            var result = new Result<List<ProductImage>>();
            var images = await _context.ProductImagesSet.Include(p => p.Product).ToListAsync();

            if (!images.Any())
            {
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "No Product Images Found" });
                return result;
            }

            result.Response = images;
            return result;
        }

        // ===============================
        // Get By Id
        // ===============================
        public async Task<Result<ProductImage>> GetById(int id)
        {
            var result = new Result<ProductImage>();
            var image = await _context.ProductImagesSet.Include(p => p.Product)
                                                       .FirstOrDefaultAsync(p => p.Id == id);

            if (image == null)
            {
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "Product Image Not Found" });
                return result;
            }

            result.Response = image;
            return result;
        }

        // ===============================
        // Add single image
        // ===============================
        public async Task<Result<ProductImage>> Add(ProductImageCreateDto model, IWebHostEnvironment env)
        {
            var result = new Result<ProductImage>();

            if (model.Image == null || model.Image.Length == 0)
            {
                result.Errors.Add(new Errors { ErrorCode = 400, ErrorMessage = "No image file provided" });
                return result;
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(model.Image.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext))
            {
                result.Errors.Add(new Errors { ErrorCode = 400, ErrorMessage = "Invalid file type. Allowed: jpg, jpeg, png, webp" });
                return result;
            }

            // Validate file size (max 5MB)
            if (model.Image.Length > 5 * 1024 * 1024)
            {
                result.Errors.Add(new Errors { ErrorCode = 400, ErrorMessage = "File size exceeds 5MB limit" });
                return result;
            }

            var product = await _context.ProductsSet.FindAsync(model.ProductId);
            if (product == null)
            {
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "Product not found" });
                return result;
            }

            // Save file to disk
            string webRootPath = env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            string uploadPath = Path.Combine(webRootPath, "uploads");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.Image.CopyToAsync(stream);
            }

            var image = new ProductImage
            {
                ProductId = model.ProductId,
                ImageUrl = $"uploads/{fileName}",
                IsPrimary = model.IsPrimary
            };

            // If this is primary, remove any existing primary
            if (model.IsPrimary)
            {
                var oldPrimary = await _context.ProductImagesSet
                    .Where(i => i.ProductId == model.ProductId && i.IsPrimary)
                    .FirstOrDefaultAsync();

                if (oldPrimary != null)
                {
                    oldPrimary.IsPrimary = false;
                }
            }

            await _context.ProductImagesSet.AddAsync(image);
            await _context.SaveChangesAsync();

            result.Response = image;
            result.Message = "Product image added successfully";
            return result;
        }

        // ===============================
        // Add multiple files
        // ===============================
        public async Task<Result<List<ProductImage>>> AddMultipleFiles(ProductImageUploadDto model, IWebHostEnvironment env)
        {
            var result = new Result<List<ProductImage>>();

            var product = await _context.ProductsSet.FindAsync(model.ProductId);
            if (product == null)
            {
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "Product not found" });
                return result;
            }

            if (model.Images == null || !model.Images.Any())
            {
                result.Errors.Add(new Errors { ErrorCode = 400, ErrorMessage = "No files provided." });
                return result;
            }

            string webRootPath = env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            string uploadPath = Path.Combine(webRootPath, "uploads");

            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var imagesList = new List<ProductImage>();

            // Validate ALL files BEFORE writing any to disk (prevents orphaned files)
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            for (int i = 0; i < model.Images.Count; i++)
            {
                var file = model.Images[i];
                string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(ext))
                {
                    result.Errors.Add(new Errors { ErrorCode = 400, ErrorMessage = $"Invalid file type for '{file.FileName}'. Allowed: jpg, jpeg, png, webp" });
                    return result;
                }
                if (file.Length > 5 * 1024 * 1024)
                {
                    result.Errors.Add(new Errors { ErrorCode = 400, ErrorMessage = $"File '{file.FileName}' exceeds 5MB limit" });
                    return result;
                }
            }

            // Remove old primary if new one is selected
            if (model.PrimaryIndex >= 0)
            {
                var oldPrimary = await _context.ProductImagesSet
                    .Where(i => i.ProductId == model.ProductId && i.IsPrimary)
                    .FirstOrDefaultAsync();

                if (oldPrimary != null)
                    oldPrimary.IsPrimary = false;
            }

            // All validated — now write files
            for (int i = 0; i < model.Images.Count; i++)
            {
                var file = model.Images[i];

                string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                string fileName = $"{Guid.NewGuid()}{extension}";
                string filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                imagesList.Add(new ProductImage
                {
                    ProductId = model.ProductId,
                    ImageUrl = $"uploads/{fileName}",
                    IsPrimary = i == model.PrimaryIndex
                });
            }

            await _context.ProductImagesSet.AddRangeAsync(imagesList);
            await _context.SaveChangesAsync();

            result.Response = imagesList;
            result.Message = $"{imagesList.Count} images uploaded successfully";
            return result;
        }

        // ===============================
        // Update
        // ===============================
        public async Task<Result<ProductImage>> Update(ProductImageUpdateDto model)
        {
            var result = new Result<ProductImage>();
            var image = await _context.ProductImagesSet.FindAsync(model.Id);

            if (image == null)
            {
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "Product Image Not Found" });
                return result;
            }

            // Ensure only one primary image exists
            if (model.IsPrimary)
            {
                var oldPrimary = await _context.ProductImagesSet
                    .Where(i => i.ProductId == model.ProductId && i.IsPrimary)
                    .FirstOrDefaultAsync();

                if (oldPrimary != null && oldPrimary.Id != model.Id)
                    oldPrimary.IsPrimary = false;
            }

            // Validate ImageUrl to prevent path traversal
            if (string.IsNullOrWhiteSpace(model.ImageUrl) || 
                model.ImageUrl.Contains("..") || 
                !model.ImageUrl.StartsWith("uploads/"))
            {
                result.Errors.Add(new Errors { ErrorCode = 400, ErrorMessage = "Invalid image URL" });
                return result;
            }

            image.ImageUrl = model.ImageUrl;
            image.IsPrimary = model.IsPrimary;

            await _context.SaveChangesAsync();

            result.Response = image;
            result.Message = "Product image updated successfully";
            return result;
        }

        // ===============================
        // Delete
        // ===============================
        public async Task<Result<bool>> Delete(int id)
        {
            var result = new Result<bool>();

            var image = await _context.ProductImagesSet.FindAsync(id);
            if (image == null)
            {
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "Product Image Not Found" });
                return result;
            }

            // Delete physical file
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", image.ImageUrl);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            _context.ProductImagesSet.Remove(image);
            await _context.SaveChangesAsync();

            result.Response = true;
            result.Message = "Product image deleted successfully";
            return result;
        }
    }
}
