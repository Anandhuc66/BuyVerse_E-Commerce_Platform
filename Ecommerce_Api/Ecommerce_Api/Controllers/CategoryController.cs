using Ecommerce_Common;
using Ecommerce_Entity.DTO;
using Ecommerce_Entity.Models;
using Ecommerce_Service.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Ecommerce_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryRepo _repo;

        public CategoryController(ICategoryRepo repo)
        {
            _repo = repo;
        }

        // -------------------------------------------------------
        // GET ALL CATEGORIES (Public)
        // -------------------------------------------------------
        [AllowAnonymous]
        [HttpGet]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _repo.GetAllCategories();
            if (result.isError) return NotFound(result);
            return Ok(result);
        }

        // -------------------------------------------------------
        // GET CATEGORY BY ID (Public)
        // -------------------------------------------------------
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _repo.GetCategoryById(id);
            if (result.isError) return NotFound(result);
            return Ok(result);
        }

        // -------------------------------------------------------
        // CREATE CATEGORY — ADMIN ONLY
        // -------------------------------------------------------
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoryCreateDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _repo.AddCategory(model);
            if (result.isError) return BadRequest(result);

            return Ok(result);
        }

        // -------------------------------------------------------
        // UPDATE CATEGORY — ADMIN ONLY
        // -------------------------------------------------------
        [Authorize(Roles = "Admin")]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] CategoryUpdateDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _repo.UpdateCategory(model);
            if (result.isError) return BadRequest(result);

            return Ok(result);
        }

        // -------------------------------------------------------
        // DELETE CATEGORY — ADMIN ONLY
        // -------------------------------------------------------
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _repo.DeleteCategory(id);
            if (result.isError) return BadRequest(result);

            return Ok(result);
        }
    }
}
