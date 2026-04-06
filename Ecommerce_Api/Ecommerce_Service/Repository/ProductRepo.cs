using Ecommerce_Common;
using Ecommerce_Entity.Data;
using Ecommerce_Entity.DTO;
using Ecommerce_Entity.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce_Service.Repository
{
    public class ProductRepo : IProductRepo
    {
        private readonly ApplicationDbContext _context;
        public ProductRepo(ApplicationDbContext context) => _context = context;

        // GET ALL PRODUCTS
        // =============================
        public async Task<Result<List<ProductDto>>> GetAllProducts()
        {
            var result = new Result<List<ProductDto>>();

            var products = await _context.ProductsSet
                .Include(p => p.SubCategory)
                    .ThenInclude(sc => sc.Category)
                .Include(p => p.Supplier)
                .Include(p => p.Images)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    SKU = p.SKU,
                    StockQuantity = p.StockQuantity,

                    SubCategoryId = p.SubCategoryId,
                    SubCategoryName = p.SubCategory.Name,

                    CategoryId = p.SubCategory.CategoryId,
                    CategoryName = p.SubCategory.Category.Name,

                    SupplierId = p.SupplierId,
                    SupplierName = p.Supplier.CompanyName,

                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,

                    ImageUrls = p.Images.Select(i => i.ImageUrl).ToList()
                })
                .ToListAsync();

            if (!products.Any())
            {
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "No products found" });
                return result;
            }

            result.Response = products;
            return result;
        }

        // GET ALL PRODUCTS (PAGED)
        // =============================
        public async Task<Result<PagedResult<ProductDto>>> GetAllProductsPaged(int pageNumber, int pageSize)
        {
            var result = new Result<PagedResult<ProductDto>>();

            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _context.ProductsSet
                .Include(p => p.SubCategory)
                    .ThenInclude(sc => sc.Category)
                .Include(p => p.Supplier)
                .Include(p => p.Images)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    SKU = p.SKU,
                    StockQuantity = p.StockQuantity,
                    SubCategoryId = p.SubCategoryId,
                    SubCategoryName = p.SubCategory.Name,
                    CategoryId = p.SubCategory.CategoryId,
                    CategoryName = p.SubCategory.Category.Name,
                    SupplierId = p.SupplierId,
                    SupplierName = p.Supplier.CompanyName,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    ImageUrls = p.Images.Select(i => i.ImageUrl).ToList()
                })
                .ToListAsync();

            result.Response = new PagedResult<ProductDto>
            {
                Items = products,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            return result;
        }

        // GET PRODUCT BY ID
        // =============================
        public async Task<Result<ProductDto>> GetProductById(int id)
        {
            var result = new Result<ProductDto>();

            var product = await _context.ProductsSet
                .Include(p => p.SubCategory)
                    .ThenInclude(sc => sc.Category)
                .Include(p => p.Supplier)
                .Include(p => p.Images)
                .Where(p => p.Id == id)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    SKU = p.SKU,
                    StockQuantity = p.StockQuantity,

                    SubCategoryId = p.SubCategoryId,
                    SubCategoryName = p.SubCategory.Name,

                    CategoryId = p.SubCategory.CategoryId,
                    CategoryName = p.SubCategory.Category.Name,

                    SupplierId = p.SupplierId,
                    SupplierName = p.Supplier.CompanyName,

                    ImageUrls = p.Images.Select(i => i.ImageUrl).ToList(),

                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (product == null)
            {
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "Product not found" });
                return result;
            }

            result.Response = product;
            return result;
        }

        // GET PRODUCTS BY SUPPLIER
        // =============================
        public async Task<Result<List<ProductDto>>> GetProductsBySupplierId(int supplierId)
        {
            var result = new Result<List<ProductDto>>();

            var products = await _context.ProductsSet
                .Include(p => p.SubCategory)
                    .ThenInclude(sc => sc.Category)
                .Include(p => p.Supplier)
                .Include(p => p.Images)
                .Where(p => p.SupplierId == supplierId)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    SKU = p.SKU,
                    StockQuantity = p.StockQuantity,

                    SubCategoryId = p.SubCategoryId,
                    SubCategoryName = p.SubCategory.Name,

                    CategoryId = p.SubCategory.CategoryId,
                    CategoryName = p.SubCategory.Category.Name,

                    SupplierId = p.SupplierId,
                    SupplierName = p.Supplier.CompanyName,

                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,

                    ImageUrls = p.Images.Select(i => i.ImageUrl).ToList()
                })
                .ToListAsync();

            if (!products.Any())
            {
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "No products found for this supplier" });
                return result;
            }

            result.Response = products;
            return result;
        }

        // =============================
        // GET PRODUCTS BY USER ID
        // =============================
        public async Task<Result<List<ProductDto>>> GetProductsByUserId(string userId)
        {
            var result = new Result<List<ProductDto>>();

            var products = await _context.ProductsSet
                .Include(p => p.Supplier)
                    .ThenInclude(s => s.User)
                .Include(p => p.SubCategory)
                    .ThenInclude(sc => sc.Category)
                .Include(p => p.Images)
                .Where(p => p.Supplier.UserId == userId)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    SKU = p.SKU,
                    StockQuantity = p.StockQuantity,

                    SubCategoryId = p.SubCategoryId,
                    SubCategoryName = p.SubCategory.Name,

                    CategoryId = p.SubCategory.CategoryId,
                    CategoryName = p.SubCategory.Category.Name,

                    SupplierId = p.SupplierId,
                    SupplierName = p.Supplier.CompanyName,

                    ImageUrls = p.Images.Select(i => i.ImageUrl).ToList(),

                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .ToListAsync();

            if (!products.Any())
            {
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "No products found for this user" });
                return result;
            }

            result.Response = products;
            return result;
        }
        // =====================================
        // GET PRODUCTS BY SUBCATEGORY ID
        // =====================================
        public async Task<Result<List<ProductDto>>> GetProductsBySubCategoryId(int subCategoryId)
        {
            var result = new Result<List<ProductDto>>();

            var products = await _context.ProductsSet
                .Include(p => p.SubCategory)
                    .ThenInclude(sc => sc.Category)
                .Include(p => p.Supplier)
                .Include(p => p.Images)
                .Where(p => p.SubCategoryId == subCategoryId)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    SKU = p.SKU,
                    StockQuantity = p.StockQuantity,

                    SubCategoryId = p.SubCategoryId,
                    SubCategoryName = p.SubCategory.Name,

                    CategoryId = p.SubCategory.CategoryId,
                    CategoryName = p.SubCategory.Category.Name,

                    SupplierId = p.SupplierId,
                    SupplierName = p.Supplier.CompanyName,

                    ImageUrls = p.Images.Select(i => i.ImageUrl).ToList(),
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .ToListAsync();

            if (!products.Any())
            {
                result.Errors.Add(new Errors
                {
                    ErrorCode = 404,
                    ErrorMessage = "No products found for this subcategory"
                });
                return result;
            }

            result.Response = products;
            return result;
        }
        public async Task<Result<List<ProductDto>>> SearchProducts(string keyword)
        {
            var result = new Result<List<ProductDto>>();

            var products = await _context.ProductsSet
                .Include(p => p.SubCategory)
                    .ThenInclude(sc => sc.Category)
                .Include(p => p.Images)
                .Where(p =>
                    p.Name.Contains(keyword) ||
                    p.Description.Contains(keyword) ||
                    p.SubCategory.Name.Contains(keyword)
                )
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    SKU = p.SKU,
                    StockQuantity = p.StockQuantity,
                    SubCategoryId = p.SubCategoryId,
                    SubCategoryName = p.SubCategory.Name,
                    CategoryId = p.SubCategory.CategoryId,
                    CategoryName = p.SubCategory.Category.Name,
                    SupplierId = p.SupplierId,
                    SupplierName = p.Supplier.CompanyName,
                    ImageUrls = p.Images.Select(i => i.ImageUrl).ToList(),
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .ToListAsync();

            result.Response = products;
            return result;
        }


        // =============================
        // ADD PRODUCT
        // =============================
        public async Task<Result<Product>> AddProduct(ProductCreateDto model)
        {
            var result = new Result<Product>();

            var product = new Product
            {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                StockQuantity = model.StockQuantity,
                SKU = model.SKU,
                SupplierId = model.SupplierId,
                SubCategoryId = model.SubCategoryId
            };

            await _context.ProductsSet.AddAsync(product);
            await _context.SaveChangesAsync();

            result.Response = product;
            result.Message = "Product added successfully";
            return result;
        }

        // =============================
        // ADD PRODUCT + IMAGES
        // =============================
        public async Task<Result<Product>> AddProductWithImages(ProductWithImagesCreateDto model, IWebHostEnvironment env)
        {
            var result = new Result<Product>();

            var product = new Product
            {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                StockQuantity = model.StockQuantity,
                SKU = model.SKU,
                SubCategoryId = model.SubCategoryId,
                SupplierId = model.SupplierId
            };

            await _context.ProductsSet.AddAsync(product);
            await _context.SaveChangesAsync();

            // Save images
            if (model.Images != null && model.Images.Any())
            {
                string root = env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                string uploadPath = Path.Combine(root, "uploads");

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                foreach (var img in model.Images)
                {
                    // Validate file type
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                    var ext = Path.GetExtension(img.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(ext))
                    {
                        result.Errors.Add(new Errors { ErrorCode = 400, ErrorMessage = $"Invalid file type '{ext}'. Allowed: jpg, jpeg, png, webp" });
                        return result;
                    }

                    // Validate file size (max 5MB)
                    if (img.Length > 5 * 1024 * 1024)
                    {
                        result.Errors.Add(new Errors { ErrorCode = 400, ErrorMessage = $"File '{img.FileName}' exceeds 5MB limit" });
                        return result;
                    }

                    string fileName = $"{Guid.NewGuid()}{ext}";
                    string filePath = Path.Combine(uploadPath, fileName);

                    using (var fs = new FileStream(filePath, FileMode.Create))
                        await img.CopyToAsync(fs);

                    await _context.ProductImagesSet.AddAsync(new ProductImage
                    {
                        ProductId = product.Id,
                        ImageUrl = $"uploads/{fileName}",
                        IsPrimary = false
                    });
                }

                await _context.SaveChangesAsync();
            }

            result.Response = product;
            result.Message = "Product & images added successfully";
            return result;
        }

        // =============================
        // UPDATE PRODUCT
        // =============================
        public async Task<Result<Product>> UpdateProduct(ProductUpdateDto model)
        {
            var result = new Result<Product>();

            var product = await _context.ProductsSet.FindAsync(model.Id);
            if (product == null)
            {
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "Product not found" });
                return result;
            }

            product.Name = model.Name;
            product.Description = model.Description;
            product.Price = model.Price;
            product.StockQuantity = model.StockQuantity;
            product.SKU = model.SKU;
            product.SupplierId = model.SupplierId;
            product.SubCategoryId = model.SubCategoryId;

            product.UpdatedAt = DateTime.UtcNow;

            _context.ProductsSet.Update(product);
            await _context.SaveChangesAsync();

            result.Response = product;
            result.Message = "Product updated successfully";
            return result;
        }

        // =============================
        // DELETE PRODUCT
        // =============================
        public async Task<Result<bool>> DeleteProduct(int id)
        {
            var result = new Result<bool>();

            var product = await _context.ProductsSet.FindAsync(id);
            if (product == null)
            {
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "Product not found" });
                return result;
            }

            // Check for existing order details (FK Restrict prevents cascade)
            var hasOrders = await _context.OrderDetailsSet.AnyAsync(od => od.ProductId == id);
            if (hasOrders)
            {
                result.Errors.Add(new Errors { ErrorCode = 400, ErrorMessage = "Cannot delete product: it has existing orders. Consider deactivating it instead." });
                return result;
            }

            // Check for active cart references (FK Restrict prevents cascade)
            var hasCartItems = await _context.CartsSet.AnyAsync(c => c.ProductId == id);
            if (hasCartItems)
            {
                result.Errors.Add(new Errors { ErrorCode = 400, ErrorMessage = "Cannot delete product: it exists in user carts. Remove cart references first." });
                return result;
            }

            // Delete physical image files before removing product (cascade will remove DB records)
            var images = await _context.ProductImagesSet
                .Where(i => i.ProductId == id)
                .ToListAsync();
            foreach (var img in images)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", img.ImageUrl);
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }

            _context.ProductsSet.Remove(product);
            await _context.SaveChangesAsync();

            result.Response = true;
            result.Message = "Product deleted successfully";
            return result;
        }

        public async Task<int?> GetSupplierIdByUserId(string userId)
        {
            var supplier = await _context.SuppliersSet
                .FirstOrDefaultAsync(s => s.UserId == userId);

            return supplier?.Id;
        }

    }
}
