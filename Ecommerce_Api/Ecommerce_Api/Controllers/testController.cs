using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Ecommerce_Entity.Models;

namespace Ecommerce_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class testController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public testController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // TEMPORARY: One-time fix endpoint - remove after use
        [AllowAnonymous]
        [HttpGet("fix-admin-role")]
        public async Task<IActionResult> FixAdminRole()
        {
            var user = await _userManager.FindByEmailAsync("admin@ecommerce.com");
            if (user == null)
                return NotFound("User not found");

            var roles = await _userManager.GetRolesAsync(user);

            // Remove all roles
            if (roles.Any())
                await _userManager.RemoveFromRolesAsync(user, roles);

            // Add only Admin role
            await _userManager.AddToRoleAsync(user, "Admin");

            var newRoles = await _userManager.GetRolesAsync(user);
            return Ok(new { message = "Fixed", previousRoles = roles, currentRoles = newRoles });
        }
    }
}
