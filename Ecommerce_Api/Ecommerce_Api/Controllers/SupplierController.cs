using Ecommerce_Common;
using Ecommerce_Entity.DTO;
using Ecommerce_Service.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class SupplierController : ControllerBase
    {
        private readonly ISupplierRepo _repo;

        public SupplierController(ISupplierRepo repo)
        {
            _repo = repo;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _repo.GetAllSuppliers();
            if (result.isError) return NotFound(result);
            return Ok(result);
        }

        [Authorize(Roles = "Admin,Supplier")]
        [HttpGet("GetSupplierByUserId/{userId}")]
        public async Task<IActionResult> GetSupplierByUserId(string userId)
        {
            // Ownership: supplier can only fetch own data
            if (User.IsInRole("Supplier"))
            {
                var loggedUserId = User.FindFirst("sub")?.Value;
                if (loggedUserId != userId)
                    return NotFound();
            }

            var result = await _repo.GetSupplierByUserId(userId);
            if (result.Errors.Any())
                return NotFound(result);

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _repo.GetSupplierById(id);
            if (result.isError) return NotFound(result);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SupplierCreateDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _repo.AddSupplier(model);
            if (result.isError) return BadRequest(result);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SupplierUpdateDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _repo.UpdateSupplier(model);
            if (result.isError) return BadRequest(result);
            return Ok(result);
        }

        [Authorize(Roles = "Admin,Supplier")]
        [HttpPut("update/{userId}")]
        public async Task<IActionResult> UpdateSupplierByUserId(string userId, [FromBody] SupplierUpdateDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Ownership: supplier can only update own profile
            if (User.IsInRole("Supplier"))
            {
                var loggedUserId = User.FindFirst("sub")?.Value;
                if (loggedUserId != userId)
                    return NotFound();
            }

            var result = await _repo.UpdateSupplierByUserId(model, userId);
            if (result.isError)
                return BadRequest(result);

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _repo.DeleteSupplier(id);
            if (result.isError) return BadRequest(result);
            return Ok(result);
        }
    }
}
