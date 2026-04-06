using Ecommerce_Common;
using Ecommerce_Entity.Data;
using Ecommerce_Entity.DTO;
using Ecommerce_Entity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce_Service.Repository
{
    public class SupplierRepo : ISupplierRepo
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;

        public SupplierRepo(
            ApplicationDbContext context,
            RoleManager<AppRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // ============================================================
        // GET ALL SUPPLIERS
        // ============================================================
        public async Task<Result<List<SupplierDto>>> GetAllSuppliers()
        {
            var result = new Result<List<SupplierDto>>();

            var suppliers = await _context.SuppliersSet
                .Include(s => s.Products)
                    .ThenInclude(p => p.SubCategory)
                        .ThenInclude(sc => sc.Category)
                .Include(s => s.Products)
                    .ThenInclude(p => p.Images)
                .Include(s => s.User)
                .ToListAsync();

            if (!suppliers.Any())
            {
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "No Suppliers Found" });
                return result;
            }

            var dtoList = suppliers.Select(s => new SupplierDto
            {
                SupplierId = s.Id,
                CompanyName = s.CompanyName,
                ContactEmail = s.ContactEmail,
                Phone = s.Phone,
                UserId = s.UserId,
                FullName = s.User?.FullName,
                Gender = s.User?.Gender,

                Products = s.Products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    SKU = p.SKU,

                    CategoryId = p.SubCategoryId,
                    CategoryName = p.SubCategory.Name,

                    SupplierId = p.SupplierId,
                    SupplierName = s.CompanyName,

                    ImageUrls = p.Images.Select(i => i.ImageUrl).ToList()
                }).ToList()

            }).ToList();

            result.Response = dtoList;
            return result;
        }

        // ============================================================
        // GET SUPPLIER BY USER ID
        // ============================================================
        public async Task<Result<SupplierDto>> GetSupplierByUserId(string userId)
        {
            var result = new Result<SupplierDto>();

            var supplier = await _context.SuppliersSet
                .Include(s => s.Products)
                    .ThenInclude(p => p.SubCategory)
                .Include(s => s.Products)
                    .ThenInclude(p => p.Images)
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (supplier == null)
            {
                result.Errors.Add(new Errors
                {
                    ErrorCode = 404,
                    ErrorMessage = "Supplier not found for this user ID"
                });
                return result;
            }

            var dto = new SupplierDto
            {
                SupplierId = supplier.Id,
                CompanyName = supplier.CompanyName,
                ContactEmail = supplier.ContactEmail,
                Phone = supplier.Phone,
                UserId = supplier.UserId,
                FullName = supplier.User?.FullName,
                Gender = supplier.User?.Gender,

                Products = supplier.Products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    SKU = p.SKU,
                    StockQuantity = p.StockQuantity,
                    CategoryId = p.SubCategoryId,
                    CategoryName = p.SubCategory.Name,
                    ImageUrls = p.Images.Select(i => i.ImageUrl).ToList()

                }).ToList()
            };

            result.Response = dto;
            return result;
        }

        // ============================================================
        // GET SUPPLIER BY ID
        // ============================================================
        public async Task<Result<Supplier>> GetSupplierById(int id)
        {
            var result = new Result<Supplier>();
            var supplier = await _context.SuppliersSet.FindAsync(id);

            if (supplier == null)
            {
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "Supplier Not Found" });
                return result;
            }

            result.Response = supplier;
            return result;
        }

        // ============================================================
        // ADD SUPPLIER (CREATES ApplicationUser + Supplier)
        // ============================================================
        public async Task<Result<Supplier>> AddSupplier(SupplierCreateDto model)
        {
            var result = new Result<Supplier>();

            // Check email exists
            var existingUser = await _userManager.FindByEmailAsync(model.ContactEmail);
            if (existingUser != null)
            {
                result.Errors.Add(new Errors { ErrorCode = 400, ErrorMessage = "Email already exists." });
                return result;
            }

            // Create ApplicationUser
            var user = new ApplicationUser
            {
                UserName = model.ContactEmail,
                Email = model.ContactEmail,
                FullName = model.CompanyName,
                Gender = "Not specified",
                PhoneNumber = model.Phone
            };

            var password = string.IsNullOrEmpty(model.Password) ? "Supplier@123" : model.Password;
            var createUser = await _userManager.CreateAsync(user, password);

            if (!createUser.Succeeded)
            {
                foreach (var err in createUser.Errors)
                    result.Errors.Add(new Errors { ErrorCode = 400, ErrorMessage = err.Description });

                return result;
            }

            // Assign Supplier role
            const string role = "Supplier";
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new AppRole
                {
                    Name = role,
                    DisplayName = role,
                    Description = "Supplier role"
                });
            }

            await _userManager.AddToRoleAsync(user, role);

            // Create Supplier record
            var supplier = new Supplier
            {
                CompanyName = model.CompanyName,
                ContactEmail = model.ContactEmail,
                Phone = model.Phone,
                UserId = user.Id
            };

            await _context.SuppliersSet.AddAsync(supplier);
            await _context.SaveChangesAsync();

            result.Response = supplier;
            result.Message = "Supplier created successfully.";
            return result;
        }

        // ============================================================
        // UPDATE SUPPLIER + ApplicationUser
        // ============================================================
        public async Task<Result<Supplier>> UpdateSupplier(SupplierUpdateDto model)
        {
            var result = new Result<Supplier>();

            var supplier = await _context.SuppliersSet.FindAsync(model.Id);
            if (supplier == null)
            {
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "Supplier Not Found" });
                return result;
            }

            supplier.CompanyName = model.CompanyName;
            supplier.ContactEmail = model.ContactEmail;
            supplier.Phone = model.Phone;

            // Also update ApplicationUser
            var user = await _userManager.FindByIdAsync(supplier.UserId);
            if (user != null)
            {
                // Check email uniqueness before updating
                if (user.Email != model.ContactEmail)
                {
                    var existingUser = await _userManager.FindByEmailAsync(model.ContactEmail);
                    if (existingUser != null && existingUser.Id != user.Id)
                    {
                        result.Errors.Add(new Errors { ErrorCode = 400, ErrorMessage = "Email already in use by another user." });
                        return result;
                    }
                }

                user.Email = model.ContactEmail;
                user.UserName = model.ContactEmail;
                user.PhoneNumber = model.Phone;

                await _userManager.UpdateAsync(user);
            }

            _context.SuppliersSet.Update(supplier);
            await _context.SaveChangesAsync();

            result.Response = supplier;
            result.Message = "Supplier updated successfully.";
            return result;
        }

        // ============================================================
        // UPDATE SUPPLIER BY USER ID
        // ============================================================
        public async Task<Result<Supplier>> UpdateSupplierByUserId(SupplierUpdateDto model, string userId)
        {
            var result = new Result<Supplier>();

            var supplier = await _context.SuppliersSet.FirstOrDefaultAsync(s => s.UserId == userId);
            if (supplier == null)
            {
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "Supplier Not Found" });
                return result;
            }

            supplier.CompanyName = model.CompanyName;
            supplier.ContactEmail = model.ContactEmail;
            supplier.Phone = model.Phone;

            // Update identity user
            var user = await _userManager.FindByIdAsync(supplier.UserId);
            if (user != null)
            {
                // Check email uniqueness before updating
                if (user.Email != model.ContactEmail)
                {
                    var existingUser = await _userManager.FindByEmailAsync(model.ContactEmail);
                    if (existingUser != null && existingUser.Id != user.Id)
                    {
                        result.Errors.Add(new Errors { ErrorCode = 400, ErrorMessage = "Email already in use by another user." });
                        return result;
                    }
                }

                user.Email = model.ContactEmail;
                user.UserName = model.ContactEmail;
                user.PhoneNumber = model.Phone;
                await _userManager.UpdateAsync(user);
            }

            _context.SuppliersSet.Update(supplier);
            await _context.SaveChangesAsync();

            result.Response = supplier;
            result.Message = "Supplier updated successfully.";
            return result;
        }

        // ============================================================
        // DELETE SUPPLIER
        // ============================================================
        public async Task<Result<bool>> DeleteSupplier(int id)
        {
            var result = new Result<bool>();

            var supplier = await _context.SuppliersSet.FindAsync(id);
            if (supplier == null)
            {
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "Supplier Not Found" });
                return result;
            }

            // Guard: prevent delete if supplier has products
            var hasProducts = await _context.ProductsSet.AnyAsync(p => p.SupplierId == id);
            if (hasProducts)
            {
                result.Errors.Add(new Errors { ErrorCode = 400, ErrorMessage = "Cannot delete supplier: they have existing products. Remove or reassign products first." });
                return result;
            }

            _context.SuppliersSet.Remove(supplier);
            await _context.SaveChangesAsync();

            result.Response = true;
            result.Message = "Supplier deleted successfully.";
            return result;
        }
    }
}
