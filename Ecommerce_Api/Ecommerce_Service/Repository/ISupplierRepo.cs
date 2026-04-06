using Ecommerce_Common;
using Ecommerce_Entity.DTO;
using Ecommerce_Entity.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ecommerce_Service.Repository
{
    public interface ISupplierRepo
    {
        Task<Result<List<SupplierDto>>> GetAllSuppliers();
        Task<Result<Supplier>> GetSupplierById(int id);
        Task<Result<SupplierDto>> GetSupplierByUserId(string userId);

        Task<Result<Supplier>> AddSupplier(SupplierCreateDto model);
        Task<Result<Supplier>> UpdateSupplier(SupplierUpdateDto model);
        Task<Result<Supplier>> UpdateSupplierByUserId(SupplierUpdateDto model, string userId);

        Task<Result<bool>> DeleteSupplier(int id);
    }
}
