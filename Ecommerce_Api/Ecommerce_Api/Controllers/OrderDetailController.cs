using Ecommerce_Common;
using Ecommerce_Entity.Data;
using Ecommerce_Entity.DTO;
using Ecommerce_Service.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class OrderDetailController : ControllerBase
    {
        private readonly IOrderDetailRepo _repo;
        private readonly ApplicationDbContext _context;
        public OrderDetailController(IOrderDetailRepo repo, ApplicationDbContext context)
        {
            _repo = repo;
            _context = context;
        }

        // -------------------------------
        // ADMIN ONLY: Get all order details
        // -------------------------------
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _repo.GetAll();
            return Ok(result);
        }

        // -------------------------------
        // User/Admin/Supplier: Get detail by ID
        // Users must ONLY access their own order details
        // -------------------------------
        [Authorize(Roles = "User,Admin,Supplier")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _repo.GetById(id);
            if (result.isError) return NotFound(result);

            // Validate user ownership
            if (User.IsInRole("User"))
            {
                var loggedUserId = User.FindFirst("sub")?.Value;

                if (result.Response.Order.UserId != loggedUserId)
                    return NotFound(result); // 404 to prevent enumeration
            }

            // Validate supplier ownership — supplier can only see details for their products
            if (User.IsInRole("Supplier"))
            {
                var loggedUserId = User.FindFirst("sub")?.Value;
                var supplier = await _context.SuppliersSet.FirstOrDefaultAsync(s => s.UserId == loggedUserId);
                if (supplier == null || result.Response.Product?.SupplierId != supplier.Id)
                    return NotFound(result); // 404 to prevent enumeration
            }

            return Ok(result);
        }

        // -------------------------------
        // ADMIN ONLY: Create order detail
        // -------------------------------
        //[Authorize(Roles = "Admin")]
        //[HttpPost]
        //public async Task<IActionResult> Create([FromBody] OrderDetailCreateDto model)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    var result = await _repo.Add(model);
        //    return result.isError ? BadRequest(result) : Ok(result);
        //}

        // -------------------------------
        // ADMIN ONLY: Update order detail
        // -------------------------------
        [Authorize(Roles = "Admin")]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] OrderDetailUpdateDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _repo.Update(model);
            return result.isError ? BadRequest(result) : Ok(result);
        }

        // -------------------------------
        // ADMIN ONLY: Delete order detail
        // -------------------------------
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _repo.Delete(id);
            return result.isError ? BadRequest(result) : Ok(result);
        }
    }
}
