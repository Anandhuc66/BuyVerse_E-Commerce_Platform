using Ecommerce_Common;
using Ecommerce_Entity.Data;
using Ecommerce_Entity.DTO;
using Ecommerce_Entity.Models;
using Ecommerce_Service.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderRepo _repo;
        private readonly InvoiceService _invoiceService;
        private readonly ApplicationDbContext _context;
        public OrderController(IOrderRepo repo, InvoiceService invoiceService, ApplicationDbContext context)
        {
            _repo = repo;
            _invoiceService = invoiceService;
            _context = context;

        }
        // ===============================
        // ADMIN: GET ALL ORDERS (Detailed)
        // ===============================
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? pageNumber, [FromQuery] int? pageSize)
        {
            if (pageNumber.HasValue && pageSize.HasValue)
                return Ok(await _repo.GetAllPaged(pageNumber.Value, pageSize.Value));

            var result = await _repo.GetAll();
            return Ok(result);
        }

        // =======================================
        // GET ORDER BY ID  (User/Admin/Supplier)
        // =======================================
        [Authorize(Roles = "User,Admin,Supplier")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _repo.GetOrderDetailsById(id);

            if (result.isError)
                return NotFound(result);

            // USER ACCESS CHECK
            if (User.IsInRole("User"))
            {
                var loggedUserId = User.FindFirst("sub")?.Value;

                if (result.Response?.User == null || result.Response.User.UserId != loggedUserId)
                    return NotFound();
            }

            // SUPPLIER ACCESS CHECK — can only view orders containing their products
            if (User.IsInRole("Supplier"))
            {
                var loggedUserId = User.FindFirst("sub")?.Value;
                var supplier = await _context.SuppliersSet.FirstOrDefaultAsync(s => s.UserId == loggedUserId);
                if (supplier == null || result.Response?.Products == null || !result.Response.Products.Any(p => p.Supplier?.SupplierId == supplier.Id))
                    return NotFound();
            }

            return Ok(result);
        }

        // ====================
        // USER: CREATE ORDER
        // ====================
        [Authorize(Roles = "User")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OrderCreateDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // SECURITY: Override UserId from JWT — never trust client
            model.UserId = User.FindFirst("sub")?.Value;

            // SECURITY: Only allow "COD" or default to "Pending" — ignore arbitrary status
            model.Status = model.Status == "COD" ? "COD" : "Pending";

            var result = await _repo.Add(model);

            if (result.isError)
                return BadRequest(result);

            // Clear cart after successful COD order (Razorpay flow clears cart in RazorpayController.Verify)
            if (model.Status == "COD")
            {
                try
                {
                    var cartItems = _context.CartsSet.Where(c => c.UserId == model.UserId);
                    _context.CartsSet.RemoveRange(cartItems);
                    await _context.SaveChangesAsync();
                }
                catch
                {
                    // Cart clearing is non-critical — order was already created successfully
                }
            }

            return Ok(result);
        }

        // ====================
        // ADMIN: UPDATE ORDER
        // ====================
        [Authorize(Roles = "Admin")]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] OrderUpdateDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _repo.Update(model);

            if (result.isError)
                return BadRequest(result);

            return Ok(result);
        }

        // ====================
        // ADMIN/USER: DELETE ORDER
        // ====================
        [Authorize(Roles = "Admin,User")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // USER: Can only delete their own cancelled orders
            if (User.IsInRole("User"))
            {
                var loggedUserId = User.FindFirst("sub")?.Value;
                var order = await _context.OrdersSet.FirstOrDefaultAsync(o => o.Id == id);

                if (order == null) return NotFound();
                if (order.UserId != loggedUserId) return NotFound();
                if (order.DeliveryStatus != "Cancelled")
                    return BadRequest(new { message = "Users can only delete cancelled orders" });
            }

            var result = await _repo.Delete(id);

            if (result.isError)
                return BadRequest(result);

            return Ok(result);
        }

        // ============================
        // USER — VIEW OWN ORDERS
        // ============================
        [Authorize(Roles = "User")]
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(string userId, [FromQuery] int? pageNumber, [FromQuery] int? pageSize)
        {
            var loggedUserId = User.FindFirst("sub")?.Value;

            if (loggedUserId != userId)
                return NotFound();

            if (pageNumber.HasValue && pageSize.HasValue)
                return Ok(await _repo.GetOrdersByUserPaged(userId, pageNumber.Value, pageSize.Value));

            var result = await _repo.GetOrdersByUser(userId);
            return Ok(result);
        }

        // ============================
        // SUPPLIER — VIEW SOLD ORDERS
        // ============================
        [Authorize(Roles = "Supplier")]
        [HttpGet("supplier/{supplierId}")]
        public async Task<IActionResult> GetBySupplier(int supplierId)
        {
            // Verify the logged-in supplier owns this ID
            var loggedUserId = User.FindFirst("sub")?.Value;
            var supplier = await _context.SuppliersSet.FirstOrDefaultAsync(s => s.UserId == loggedUserId);
            if (supplier == null || supplier.Id != supplierId)
                return NotFound();

            var result = await _repo.GetOrdersBySupplier(supplierId);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("{orderId}/invoice")]
        public async Task<IActionResult> DownloadInvoice(int orderId)
        {
            var order = await _context.OrdersSet
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return NotFound("Order not found");

            // Ownership check: user can only download own invoice
            if (User.IsInRole("User"))
            {
                var loggedUserId = User.FindFirst("sub")?.Value;
                if (order.UserId != loggedUserId)
                    return NotFound();
            }

            // Supplier can only download invoices for orders containing their products
            if (User.IsInRole("Supplier"))
            {
                var loggedUserId = User.FindFirst("sub")?.Value;
                var supplier = await _context.SuppliersSet.FirstOrDefaultAsync(s => s.UserId == loggedUserId);
                if (supplier == null || !order.OrderDetails.Any(od => od.Product?.SupplierId == supplier.Id))
                    return NotFound();
            }

            var invoicePdf = _invoiceService.GenerateInvoicePdf(order);

            return File(invoicePdf, "application/pdf", $"Invoice_{order.OrderNumber}.pdf");
        }

        // ============================
        // UPDATE DELIVERY STATUS
        // ============================
        [Authorize(Roles = "Admin,Supplier,User")]
        [HttpPut("{orderId}/status")]
        public async Task<IActionResult> UpdateDeliveryStatus(int orderId, [FromBody] OrderDeliveryStatusUpdateDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // USER: Can only cancel their own orders if delivery is still Pending/Processing
            if (User.IsInRole("User"))
            {
                var loggedUserId = User.FindFirst("sub")?.Value;
                var order = await _context.OrdersSet.FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null) return NotFound();
                if (order.UserId != loggedUserId) return NotFound();

                // Users can only cancel, not change to other delivery statuses
                if (model.DeliveryStatus != "Cancelled")
                    return BadRequest(new { message = "Users can only cancel orders" });

                // Only allow cancellation if delivery is still Pending or Processing
                if (order.DeliveryStatus != "Pending" && order.DeliveryStatus != "Processing")
                    return BadRequest(new { message = "Cannot cancel order that is already shipped or delivered" });
            }

            // Supplier can only update orders containing their products
            if (User.IsInRole("Supplier"))
            {
                var loggedUserId = User.FindFirst("sub")?.Value;
                var supplier = await _context.SuppliersSet.FirstOrDefaultAsync(s => s.UserId == loggedUserId);
                if (supplier == null) return NotFound();

                var order = await _context.OrdersSet
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null) return NotFound();
                if (!order.OrderDetails.Any(od => od.Product.SupplierId == supplier.Id))
                    return NotFound();
            }

            var result = await _repo.UpdateOrderStatus(orderId, model.DeliveryStatus);
            if (result.isError) return BadRequest(result);
            return Ok(result);
        }


    }
}
