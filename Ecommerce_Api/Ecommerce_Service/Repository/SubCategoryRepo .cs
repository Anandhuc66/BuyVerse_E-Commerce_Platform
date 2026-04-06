using Ecommerce_Common;
using Ecommerce_Entity.Data;
using Ecommerce_Entity.DTO;
using Ecommerce_Entity.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce_Service.Repository
{
    public class SubCategoryRepo : ISubCategoryRepo
    {
        private readonly ApplicationDbContext _context;

        public SubCategoryRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<SubCategoryDto>>> GetAllSubCategories()
        {
            var result = new Result<List<SubCategoryDto>>();

            var subcategories = await _context.SubCategoriesSet
                .Include(sc => sc.Category)
                .Select(sc => new SubCategoryDto
                {
                    Id = sc.Id,
                    Name = sc.Name,
                    Description = sc.Description,
                    CategoryId = sc.CategoryId,
                    CategoryName = sc.Category.Name
                })
                .ToListAsync();

            if (subcategories.Any())
                result.Response = subcategories;
            else
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "No subcategories found" });

            return result;
        }

        public async Task<Result<List<SubCategoryDto>>> GetByCategoryId(int categoryId)
        {
            var result = new Result<List<SubCategoryDto>>();

            var subcategories = await _context.SubCategoriesSet
                .Where(sc => sc.CategoryId == categoryId)
                .Include(sc => sc.Category)
                .Select(sc => new SubCategoryDto
                {
                    Id = sc.Id,
                    Name = sc.Name,
                    Description = sc.Description,
                    CategoryId = sc.CategoryId,
                    CategoryName = sc.Category.Name
                })
                .ToListAsync();

            if (subcategories.Any())
                result.Response = subcategories;
            else
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "No subcategories under this category" });

            return result;
        }

        public async Task<Result<SubCategoryDto>> GetSubCategoryById(int id)
        {
            var result = new Result<SubCategoryDto>();

            var subcategory = await _context.SubCategoriesSet
                .Include(sc => sc.Category)
                .Where(sc => sc.Id == id)
                .Select(sc => new SubCategoryDto
                {
                    Id = sc.Id,
                    Name = sc.Name,
                    Description = sc.Description,
                    CategoryId = sc.CategoryId,
                    CategoryName = sc.Category.Name
                })
                .FirstOrDefaultAsync();

            if (subcategory != null)
                result.Response = subcategory;
            else
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "Subcategory not found" });

            return result;
        }

        public async Task<Result<SubCategory>> AddSubCategory(SubCategoryCreateDto model)
        {
            var result = new Result<SubCategory>();

            var subcategory = new SubCategory
            {
                Name = model.Name,
                Description = model.Description,
                CategoryId = model.CategoryId
            };

            await _context.SubCategoriesSet.AddAsync(subcategory);
            await _context.SaveChangesAsync();

            result.Response = subcategory;
            result.Message = "SubCategory created successfully";
            return result;
        }

        public async Task<Result<SubCategory>> UpdateSubCategory(SubCategoryUpdateDto model)
        {
            var result = new Result<SubCategory>();

            var subcategory = await _context.SubCategoriesSet.FindAsync(model.Id);
            if (subcategory == null)
            {
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "Subcategory not found" });
                return result;
            }

            subcategory.Name = model.Name;
            subcategory.Description = model.Description;
            subcategory.CategoryId = model.CategoryId;

            _context.SubCategoriesSet.Update(subcategory);
            await _context.SaveChangesAsync();

            result.Response = subcategory;
            result.Message = "SubCategory updated successfully";
            return result;
        }

        public async Task<Result<bool>> DeleteSubCategory(int id)
        {
            var result = new Result<bool>();

            var subcategory = await _context.SubCategoriesSet.FindAsync(id);
            if (subcategory == null)
            {
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "Subcategory not found" });
                return result;
            }

            // Guard: prevent delete if products exist under this subcategory
            var hasProducts = await _context.ProductsSet.AnyAsync(p => p.SubCategoryId == id);
            if (hasProducts)
            {
                result.Errors.Add(new Errors { ErrorCode = 400, ErrorMessage = "Cannot delete subcategory: it has existing products. Remove or reassign them first." });
                return result;
            }

            _context.SubCategoriesSet.Remove(subcategory);
            await _context.SaveChangesAsync();

            result.Response = true;
            result.Message = "SubCategory deleted successfully";
            return result;
        }
    }
}
