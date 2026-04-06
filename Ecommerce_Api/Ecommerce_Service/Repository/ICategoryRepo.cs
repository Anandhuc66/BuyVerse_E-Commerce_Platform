using Ecommerce_Common;
using Ecommerce_Entity.DTO;
using Ecommerce_Entity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce_Service.Repository

{
    public interface ICategoryRepo
    {
        Task<Result<List<Category>>> GetAllCategories();
        Task<Result<Category>> GetCategoryById(int id);
        Task<Result<Category>> AddCategory(CategoryCreateDto model);
        Task<Result<Category>> UpdateCategory(CategoryUpdateDto model);
        Task<Result<bool>> DeleteCategory(int id);
    }
}
