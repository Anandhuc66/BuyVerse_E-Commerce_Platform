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
    public class AuthController : ControllerBase
    {
        private readonly IUserRepo _userRepo;

        public AuthController(IUserRepo userRepo)
        {
            _userRepo = userRepo;
        }

        // ------------------------------------------
        // Register Normal User
        // ------------------------------------------
        [AllowAnonymous]
        [HttpPost("register-user")]
        [Consumes("application/json")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userRepo.RegisterUserAsync(model);

            if (result.isError)
                return BadRequest(result);

            return Ok(result);
        }

        // ------------------------------------------
        // Register Supplier
        // ------------------------------------------
        [AllowAnonymous]
        [HttpPost("register-supplier")]
        [Consumes("application/json")]
        public async Task<IActionResult> RegisterSupplier([FromBody] SupplierCreateDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userRepo.RegisterSupplierAsync(model);

            if (result.isError)
                return BadRequest(result);

            return Ok(result);
        }

        // ------------------------------------------
        // Login
        // ------------------------------------------
        [AllowAnonymous]
        [HttpPost("login")]
        [Consumes("application/json")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userRepo.LoginAsync(model);

            if (result.isError)
                return BadRequest(result);

            return Ok(result);
        }

        // ------------------------------------------
        // Admin: Get All Users
        // ------------------------------------------
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var result = await _userRepo.GetAllUsersAsync();
            return Ok(result);
        }

        // ------------------------------------------
        // Change Password (any authenticated user)
        // ------------------------------------------
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst("sub")?.Value
                      ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _userRepo.ChangePasswordAsync(userId, model);
            if (result.isError)
                return BadRequest(result);
            return Ok(result);
        }

        // ------------------------------------------
        // Update Profile (any authenticated user)
        // ------------------------------------------
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst("sub")?.Value
                      ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _userRepo.UpdateProfileAsync(userId, model);
            if (result.isError)
                return BadRequest(result);
            return Ok(result);
        }
    }
}
