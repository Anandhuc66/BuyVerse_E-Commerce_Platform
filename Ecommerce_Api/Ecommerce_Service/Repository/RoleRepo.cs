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
    public class RoleRepo : IRoleRepo
    {
        private readonly RoleManager<AppRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public RoleRepo(RoleManager<AppRole> roleManager, ApplicationDbContext context)
        {
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<Result<List<AppRole>>> GetAllRoles()
        {
            var result = new Result<List<AppRole>>();

            var roles = await _context.Roles.ToListAsync();

            if (roles.Any()) 
                result.Response = roles;
            else 
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "No roles found" });

            return result;
        }

        public async Task<Result<AppRole>> GetRoleById(string id)
        {
            var result = new Result<AppRole>();

            var role = await _roleManager.FindByIdAsync(id);

            if (role != null) 
                result.Response = role;
            else 
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "Role not found" });

            return result;
        }

        public async Task<Result<AppRole>> CreateRole(AppRoleCreateDto model)
        {
            var result = new Result<AppRole>();
           var role = new AppRole
            {
                Name = model.Name,
               NormalizedName = model.Name.ToUpper(),
               DisplayName = model.DisplayName,   // Optional - UI label
                Description = model.Description
            };

            var identityResult = await _roleManager.CreateAsync(role);

            if (!identityResult.Succeeded)
            {
                foreach (var error in identityResult.Errors)
                    result.Errors.Add(new Errors { ErrorCode = 400, ErrorMessage = error.Description });
                return result;
            }

            result.Response = role;
            result.Message = "Role created successfully";
            return result;
        }

        public async Task<Result<AppRole>> UpdateRole(AppRoleUpdateDto model)
        {
            var result = new Result<AppRole>();
            var role = await _roleManager.FindByIdAsync(model.Id);
            if (role == null)
            {
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "Role not found" });
                return result;
            }

            role.Name = model.Name;
            role.NormalizedName = model.Name.ToUpper();
            role.DisplayName = model.DisplayName;
            role.Description = model.Description;


            _context.Roles.Update(role);
            await _context.SaveChangesAsync();

            result.Response = role;
            result.Message = "Role updated successfully";
            return result;
        }

        public async Task<Result<bool>> DeleteRole(string id)
        {
            var result = new Result<bool>();

            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "Role not found" });
                return result;
            }

            var deleteResult = await _roleManager.DeleteAsync(role);

            if (!deleteResult.Succeeded)
            {
                foreach (var error in deleteResult.Errors)
                    result.Errors.Add(new Errors { ErrorCode = 400, ErrorMessage = error.Description });
                return result;
            }

            result.Response = true;
            result.Message = "Role deleted successfully";
            return result;
        }
    }
}
