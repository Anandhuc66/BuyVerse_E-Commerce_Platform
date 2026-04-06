using Ecommerce_Common;
using Ecommerce_Entity.DTO;
using Ecommerce_Service.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ecommerce_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class UserAddressController : ControllerBase
    {
        private readonly IUserAddressRepo _repo;

        public UserAddressController(IUserAddressRepo repo)
        {
            _repo = repo;
        }

        private string GetLoggedUserId() => User.FindFirst("sub")?.Value ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _repo.GetAll();
            if (result.isError) return NotFound(result);
            return Ok(result);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(string userId)
        {
            // Ownership: users can only fetch own addresses
            if (!User.IsInRole("Admin"))
            {
                var loggedUserId = GetLoggedUserId();
                if (loggedUserId != userId)
                    return NotFound();
            }

            var result = await _repo.GetByUserId(userId);
            if (result.isError) return NotFound(result);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _repo.GetById(id);
            if (result.isError) return NotFound(result);

            // Ownership check
            if (!User.IsInRole("Admin"))
            {
                var loggedUserId = GetLoggedUserId();
                if (result.Response?.UserId != loggedUserId)
                    return NotFound();
            }

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserAddressCreateDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Force userId from token for non-admin users
            if (!User.IsInRole("Admin"))
                model.UserId = GetLoggedUserId();

            var result = await _repo.Add(model);
            if (result.isError) return BadRequest(result);
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UserAddressUpdateDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verify ownership before update
            if (!User.IsInRole("Admin"))
            {
                var existing = await _repo.GetById(model.Id);
                if (existing.isError || existing.Response?.UserId != GetLoggedUserId())
                    return NotFound();
            }

            var result = await _repo.Update(model);
            if (result.isError) return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Verify ownership before delete
            if (!User.IsInRole("Admin"))
            {
                var existing = await _repo.GetById(id);
                if (existing.isError || existing.Response?.UserId != GetLoggedUserId())
                    return NotFound();
            }

            var result = await _repo.Delete(id);
            if (result.isError) return BadRequest(result);
            return Ok(result);
        }
    }
}
