using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Ecommerce_Entity.Models;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace Ecommerce_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class testController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Cloudinary _cloudinary;

        public testController(UserManager<ApplicationUser> userManager, Cloudinary cloudinary)
        {
            _userManager = userManager;
            _cloudinary = cloudinary;
        }

        // TEMPORARY: Test Cloudinary connectivity
        [AllowAnonymous]
        [HttpGet("test-cloudinary")]
        public async Task<IActionResult> TestCloudinary()
        {
            try
            {
                var result = await _cloudinary.PingAsync();
                return Ok(new { status = "OK", cloudName = _cloudinary.Api.Account.Cloud, result = result.StatusCode });
            }
            catch (Exception ex)
            {
                return Ok(new { status = "FAILED", error = ex.Message, cloudName = _cloudinary.Api.Account.Cloud ?? "NULL" });
            }
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
