using Ecommerce_Common;
using Ecommerce_Entity.DTO;
using Ecommerce_Entity.Models;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ecommerce_Service.Repository
{
    public interface IProductRepo
    {
        Task<Result<List<ProductDto>>> GetAllProducts();
        Task<Result<PagedResult<ProductDto>>> GetAllProductsPaged(int pageNumber, int pageSize);
        Task<Result<ProductDto>> GetProductById(int id);
        Task<Result<List<ProductDto>>> GetProductsBySupplierId(int supplierId);
        Task<Result<List<ProductDto>>> GetProductsByUserId(string userId);


        Task<Result<Product>> AddProduct(ProductCreateDto model);
        Task<Result<Product>> AddProductWithImages(ProductWithImagesCreateDto model, IWebHostEnvironment env);

        Task<Result<Product>> UpdateProduct(ProductUpdateDto model);
        Task<Result<bool>> DeleteProduct(int id);
        Task<int?> GetSupplierIdByUserId(string userId);
        Task<Result<List<ProductDto>>> GetProductsBySubCategoryId(int subCategoryId);
        Task<Result<List<ProductDto>>> SearchProducts(string keyword);

    }
}
