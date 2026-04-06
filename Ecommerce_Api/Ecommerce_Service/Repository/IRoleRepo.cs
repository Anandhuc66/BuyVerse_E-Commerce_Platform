using Ecommerce_Common;
using Ecommerce_Entity.DTO;
using Ecommerce_Entity.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ecommerce_Service.Repository
{
    public interface IRoleRepo
    {
        Task<Result<List<AppRole>>> GetAllRoles();
        Task<Result<AppRole>> GetRoleById(string id);
        Task<Result<AppRole>> CreateRole(AppRoleCreateDto model);
        Task<Result<AppRole>> UpdateRole(AppRoleUpdateDto model);
        Task<Result<bool>> DeleteRole(string id);
    }
}
