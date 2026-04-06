using Ecommerce_Common;
using Ecommerce_Entity.DTO;
using Ecommerce_Service.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Ecommerce_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class SubCategoryController : ControllerBase
    {
        private readonly ISubCategoryRepo _repo;

        public SubCategoryController(ISubCategoryRepo repo)
        {
            _repo = repo;
        }

        [AllowAnonymous]
        [HttpGet]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _repo.GetAllSubCategories();
            if (result.isError) return NotFound(result);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _repo.GetSubCategoryById(id);
            if (result.isError) return NotFound(result);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("ByCategory/{categoryId}")]
        public async Task<IActionResult> GetByCategory(int categoryId)
        {
            var result = await _repo.GetByCategoryId(categoryId);
            if (result.isError) return NotFound(result);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SubCategoryCreateDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _repo.AddSubCategory(model);
            if (result.isError) return BadRequest(result);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SubCategoryUpdateDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _repo.UpdateSubCategory(model);
            if (result.isError) return BadRequest(result);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _repo.DeleteSubCategory(id);
            if (result.isError) return BadRequest(result);
            return Ok(result);
        }
    }
}
