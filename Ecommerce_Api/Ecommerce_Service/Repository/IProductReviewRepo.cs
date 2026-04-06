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
    public interface IProductReviewRepo
    {
        Task<Result<List<ProductReviewDto>>> GetByProductId(int productId);
        Task<Result<ProductReviewDto>> GetById(int id);

        Task<Result<ProductReview>> Add(ProductReviewCreateDto model);
        Task<Result<ProductReview>> Update(ProductReviewUpdateDto model);
        Task<Result<bool>> Delete(int id);
    }
}
