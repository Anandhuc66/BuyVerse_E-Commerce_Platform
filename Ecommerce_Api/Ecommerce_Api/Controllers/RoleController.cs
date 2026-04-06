using Ecommerce_Entity.DTO;
using Ecommerce_Service.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Ecommerce_Api.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRoleRepo _roleRepo;

        public RoleController(IRoleRepo roleRepo)
        {
            _roleRepo = roleRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _roleRepo.GetAllRoles();
            if (result.isError) return NotFound(result);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _roleRepo.GetRoleById(id);
            if (result.isError) return NotFound(result);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AppRoleCreateDto model)
        {
            var result = await _roleRepo.CreateRole(model);
            if (result.isError) return BadRequest(result);
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] AppRoleUpdateDto model)
        {
            var result = await _roleRepo.UpdateRole(model);
            if (result.isError) return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _roleRepo.DeleteRole(id);
            if (result.isError) return BadRequest(result);
            return Ok(result);
        }
    }
}
