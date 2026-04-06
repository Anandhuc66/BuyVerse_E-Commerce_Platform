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
    public interface ISubCategoryRepo
    {
        Task<Result<List<SubCategoryDto>>> GetAllSubCategories();
        Task<Result<List<SubCategoryDto>>> GetByCategoryId(int categoryId);
        Task<Result<SubCategoryDto>> GetSubCategoryById(int id);
        Task<Result<SubCategory>> AddSubCategory(SubCategoryCreateDto model);
        Task<Result<SubCategory>> UpdateSubCategory(SubCategoryUpdateDto model);
        Task<Result<bool>> DeleteSubCategory(int id);
    }
}
