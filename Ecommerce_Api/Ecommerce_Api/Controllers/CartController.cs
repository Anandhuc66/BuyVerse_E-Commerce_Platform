using Ecommerce_Common;
using Ecommerce_Entity.Data;
using Ecommerce_Entity.DTO;
using Ecommerce_Entity.Models;
using Ecommerce_Service.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ecommerce_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class CartController : ControllerBase
    {
        private readonly ICartRepo _repo;
        private readonly ApplicationDbContext _context;

        public CartController(ICartRepo repo, ApplicationDbContext context)
        {
            _repo = repo;
            _context = context;
        }

        // ----------------------------------------------------
        // GET ALL CARTS (ADMIN ONLY)
        // ----------------------------------------------------
        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _repo.GetAll();
            if (result.isError) return NotFound(result);
            return Ok(result);
        }

        // ----------------------------------------------------
        // GET CART FOR CURRENT LOGGED-IN USER
        // ----------------------------------------------------
        [Authorize(Roles = "Admin,User")]
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                var result = new Result<string>();
                result.Errors.Add(new Errors
                {
                    ErrorCode = 400,
                    ErrorMessage = "UserId is required"
                });

                return BadRequest(result);
            }

            // If user role, restrict access to own cart only
            if (User.IsInRole("User"))
            {
                var loggedUserId = User.FindFirst("sub")?.Value;

                if (loggedUserId != userId)
                {
                    return NotFound();
                }
            }

            var response = await _repo.GetByUserId(userId);
            if (response.isError) return NotFound(response);

            return Ok(response);
        }



        // ----------------------------------------------------
        // GET CART BY CART ID
        // ----------------------------------------------------
        [Authorize(Roles = "Admin,User")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _repo.GetById(id);
            if (result.isError) return NotFound(result);

            // Ownership check: user can only fetch own cart item
            if (User.IsInRole("User"))
            {
                var loggedUserId = User.FindFirst("sub")?.Value;
                if (result.Response?.UserId != loggedUserId)
                    return NotFound();
            }

            return Ok(result);
        }

        // ----------------------------------------------------
        // ADD TO CART
        // ----------------------------------------------------
        [Authorize(Roles = "User")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CartCreateDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // SECURITY: Force UserId from JWT — never trust client
            model.UserId = User.FindFirst("sub")?.Value;

            var result = await _repo.Add(model);

            if (result.isError) return BadRequest(result);

            return Ok(result);
        }

        // ----------------------------------------------------
        // UPDATE CART (QUANTITY)
        // ----------------------------------------------------
        [Authorize(Roles = "User")]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] CartUpdateDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // SECURITY: Ownership check — user can only update own cart items
            var loggedUserId = User.FindFirst("sub")?.Value;
            var cart = await _context.CartsSet.FindAsync(model.CartId);
            if (cart == null)
                return NotFound(new { message = "Cart not found" });
            if (cart.UserId != loggedUserId)
                return NotFound();

            var result = await _repo.Update(model);

            if (result.isError) return BadRequest(result);

            return Ok(result);
        }

        // ----------------------------------------------------
        // DELETE CART ITEM
        // ----------------------------------------------------
        [Authorize(Roles = "User")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // SECURITY: Ownership check — user can only delete own cart items
            var loggedUserId = User.FindFirst("sub")?.Value;
            var cart = await _context.CartsSet.FindAsync(id);
            if (cart == null)
                return NotFound(new { message = "Cart not found" });
            if (cart.UserId != loggedUserId)
                return NotFound();

            var result = await _repo.Delete(id);

            if (result.isError) return BadRequest(result);

            return Ok(result);
        }

        // ----------------------------------------------------
        // CLEAR ALL CART ITEMS FOR USER
        // ----------------------------------------------------
        [Authorize(Roles = "User")]
        [HttpDelete("clear/{userId}")]
        public async Task<IActionResult> ClearByUser(string userId)
        {
            var loggedUserId = User.FindFirst("sub")?.Value;
            if (loggedUserId != userId)
                return NotFound();

            var result = await _repo.ClearByUserId(userId);
            if (result.isError) return NotFound(result);
            return Ok(result);
        }
    }
}
