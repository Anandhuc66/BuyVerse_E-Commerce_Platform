using Ecommerce_Common;
using Ecommerce_Entity.Data;
using Ecommerce_Entity.DTO;
using Ecommerce_Entity.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Ecommerce_Service.Repository
{
    
    public class CategoryRepo : ICategoryRepo
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<Category>>> GetAllCategories()
        {
            var result = new Result<List<Category>>();

            var categories = await _context.CategoriesSet
                .Include(c => c.SubCategories)
                .ToListAsync();

            if (categories.Any())
                result.Response = categories;
            else
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "No categories found" });

            return result;
        }

        public async Task<Result<Category>> GetCategoryById(int id)
        {
            var result = new Result<Category>();

            var category = await _context.CategoriesSet
                .Include(c => c.SubCategories)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category != null)
                result.Response = category;
            else
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "Category not found" });

            return result;
        }

        public async Task<Result<Category>> AddCategory(CategoryCreateDto model)
        {
            var result = new Result<Category>();

            var category = new Category
            {
                Name = model.Name,
                Description = model.Description
            };

            await _context.CategoriesSet.AddAsync(category);
            await _context.SaveChangesAsync();

            result.Response = category;
            result.Message = "Category added successfully";
            return result;
        }

        public async Task<Result<Category>> UpdateCategory(CategoryUpdateDto model)
        {
            var result = new Result<Category>();

            var category = await _context.CategoriesSet.FindAsync(model.Id);

            if (category == null)
            {
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "Category not found" });
                return result;
            }

            category.Name = model.Name;
            category.Description = model.Description;
            category.UpdatedAt = DateTime.UtcNow;

            _context.CategoriesSet.Update(category);
            await _context.SaveChangesAsync();

            result.Response = category;
            result.Message = "Category updated successfully";
            return result;
        }

        public async Task<Result<bool>> DeleteCategory(int id)
        {
            var result = new Result<bool>();

            var category = await _context.CategoriesSet.FindAsync(id);
            if (category == null)
            {
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "Category not found" });
                return result;
            }

            // Check for subcategories that have products (FK Restrict prevents cascade)
            var hasProducts = await _context.SubCategoriesSet
                .Where(sc => sc.CategoryId == id)
                .AnyAsync(sc => sc.Products.Any());

            if (hasProducts)
            {
                result.Errors.Add(new Errors { ErrorCode = 400, ErrorMessage = "Cannot delete category: one or more subcategories contain products. Remove or reassign products first." });
                return result;
            }

            _context.CategoriesSet.Remove(category);
            await _context.SaveChangesAsync();

            result.Response = true;
            result.Message = "Category deleted successfully";
            return result;
        }
    }
}
