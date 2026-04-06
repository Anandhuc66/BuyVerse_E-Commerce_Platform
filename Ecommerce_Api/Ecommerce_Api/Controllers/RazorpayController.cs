using Ecommerce_Entity.Data;
using Ecommerce_Entity.DTO;
using Ecommerce_Service.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "User")]

    public class RazorpayController : ControllerBase
    {
        private readonly IRazorpayService _razorpay;
        private readonly ApplicationDbContext _context;

        public RazorpayController(IRazorpayService razorpay, ApplicationDbContext context)
        {
            _razorpay = razorpay;
            _context = context;
        }

        // -----------------------------------------------------
        // 1. Create Razorpay Order (called before opening widget)
        // -----------------------------------------------------
        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateRazorpayOrderRequest request)
        {
            if (request.OrderId <= 0)
                return BadRequest("Invalid OrderId");

            // Ownership: verify logged-in user owns this order
            var loggedUserId = User.FindFirst("sub")?.Value;
            var order = await _context.OrdersSet.FindAsync(request.OrderId);
            if (order == null || order.UserId != loggedUserId)
                return NotFound(new { message = "Order not found" });

            var response = await _razorpay.CreateOrderAsync(request.OrderId);

            return Ok(response);
        }

        // -----------------------------------------------------
        // 2. Verify Payment + Update DB
        // -----------------------------------------------------
        [HttpPost("verify")]
        public async Task<IActionResult> VerifyPayment([FromBody] RazorpayVerifyRequest request)
        {
            if (string.IsNullOrEmpty(request.RazorpayPaymentId) ||
                string.IsNullOrEmpty(request.RazorpayOrderId) ||
                string.IsNullOrEmpty(request.RazorpaySignature))
            {
                return BadRequest("Missing required fields");
            }

            // SECURITY: Ownership check BEFORE any DB mutation
            var loggedUserId = User.FindFirst("sub")?.Value;
            var order = await _context.OrdersSet
                .FirstOrDefaultAsync(o => o.Id == request.OrderId);

            if (order == null)
                return BadRequest("Order not found");

            if (order.UserId != loggedUserId)
                return NotFound(new { message = "Order not found" });

            // 1. Verify Razorpay Signature
            bool isValid;
            try
            {
                isValid = await _razorpay.VerifySignatureAsync(request);
            }
            catch (Exception)
            {
                return BadRequest("Payment verification failed — could not reach payment provider");
            }

            if (!isValid)
                return BadRequest("Invalid payment signature");

            // 2. Update payment & order status (ownership already verified)
            try
            {
                await _razorpay.UpdateOrderPaymentAsync(request);
            }
            catch (Exception)
            {
                return BadRequest("Payment update failed. Please contact support.");
            }

            string userId = order.UserId;

            // 3. Clear user's cart after successful payment
            try
            {
                var cartItems = _context.CartsSet.Where(c => c.UserId == userId);
                _context.CartsSet.RemoveRange(cartItems);
                await _context.SaveChangesAsync();
            }
            catch
            {
                // Cart clearing is non-critical — payment was already verified successfully
            }

            return Ok(new
            {
                success = true,
                message = "Payment verified, order updated, cart cleared!"
            });
        }

        //[HttpPost("verify")]
        //public async Task<IActionResult> VerifyPayment([FromBody] RazorpayVerifyRequest request)
        //{
        //    if (string.IsNullOrEmpty(request.RazorpayPaymentId) ||
        //        string.IsNullOrEmpty(request.RazorpayOrderId) ||
        //        string.IsNullOrEmpty(request.RazorpaySignature))
        //    {
        //        return BadRequest("Missing required fields");
        //    }

        //    var isValid = await _razorpay.VerifySignatureAsync(request);

        //    if (!isValid)
        //        return BadRequest("Invalid payment signature");

        //    await _razorpay.UpdateOrderPaymentAsync(request);

        //    // ⭐ REMOVE ALL CART ITEMS FOR THIS USER ⭐
        //    var cartItems = _context.CartsSet.Where(c => c.UserId == request.UserId);

        //    _context.CartsSet.RemoveRange(cartItems);
        //    await _context.SaveChangesAsync();

        //    return Ok(new
        //    {
        //        success = true,
        //        message = "Payment verified, order updated, cart cleared!"
        //    });
        //}
    }
}
