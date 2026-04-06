using Ecommerce_Common;
using Ecommerce_Entity.DTO;
using Ecommerce_Entity.Models;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce_Service.Repository
{
    public interface IProductImageRepo
    {
        Task<Result<List<ProductImage>>> GetAll();
        Task<Result<ProductImage>> GetById(int id);
        Task<Result<ProductImage>> Add(ProductImageCreateDto model, IWebHostEnvironment env);
        Task<Result<List<ProductImage>>> AddMultipleFiles(ProductImageUploadDto model, IWebHostEnvironment env);

        Task<Result<ProductImage>> Update(ProductImageUpdateDto model);
        Task<Result<bool>> Delete(int id);
    }
}
