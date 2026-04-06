using Ecommerce_Common;
using Ecommerce_Entity.Data;
using Ecommerce_Entity.DTO;
using Ecommerce_Service.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Ecommerce_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentRepo _repo;
        private readonly ApplicationDbContext _context;
        public PaymentController(IPaymentRepo repo, ApplicationDbContext context)
        {
            _repo = repo;
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _repo.GetAll());

        [Authorize(Roles = "Admin,User")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _repo.GetById(id);
            if (result.isError) return NotFound(result);

            // SECURITY: Users can only view payments for their own orders
            if (User.IsInRole("User"))
            {
                var loggedUserId = User.FindFirst("sub")?.Value;
                if (result.Response?.Order?.UserId != loggedUserId)
                    return NotFound(new Result<object> { Errors = { new Errors { ErrorCode = 404, ErrorMessage = "Payment not found" } } });
            }

            return Ok(result);
        }

        [Authorize(Roles = "User")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PaymentCreateDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // SECURITY: Verify order exists and belongs to logged-in user
            var loggedUserId = User.FindFirst("sub")?.Value;
            var order = await _context.OrdersSet
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == model.OrderId);

            if (order == null)
                return NotFound(new { message = "Order not found" });

            if (order.UserId != loggedUserId)
                return NotFound();

            // SECURITY: Prevent duplicate payment
            if (order.Payment != null && order.Payment.Status == "Completed")
                return BadRequest(new { message = "Payment already completed for this order" });

            // SECURITY: Override client amount with server-side order total
            model.Amount = order.TotalAmount;

            var result = await _repo.Add(model);
            if (result.isError) return BadRequest(result);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] PaymentUpdateDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _repo.Update(model);
            if (result.isError) return BadRequest(result);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _repo.Delete(id);
            if (result.isError) return BadRequest(result);
            return Ok(result);
        }
    }
}
