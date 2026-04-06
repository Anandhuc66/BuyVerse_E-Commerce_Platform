using Ecommerce_Entity.Data;
using Ecommerce_Entity.DTO;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Ecommerce_Entity.Models;

// Alias Razorpay classes
using RazorpayOrder = Razorpay.Api.Order;
using RazorpayClient = Razorpay.Api.RazorpayClient;
using RazorpayUtils = Razorpay.Api.Utils;

// Alias your own models to avoid clash
using DbOrder = Ecommerce_Entity.Models.Order;
using DbPayment = Ecommerce_Entity.Models.Payment;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ecommerce_Service.Repository
{
    public class RazorpayServiceRepo : IRazorpayService
    {
        private readonly string _keyId;
        private readonly string _keySecret;
        private readonly ApplicationDbContext _context;

        public RazorpayServiceRepo(IConfiguration config, ApplicationDbContext context)
        {
            _keyId = config["Razorpay:KeyId"];
            _keySecret = config["Razorpay:KeySecret"];
            _context = context;
        }

        // -------------------------
        // 1. Create Razorpay Order
        // -------------------------
        public async Task<CreateRazorpayOrderResponse> CreateOrderAsync(int orderId)
        {
            DbOrder order = await _context.OrdersSet
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                throw new Exception("Order not found");

            if (order.TotalAmount <= 0)
                throw new Exception("Order amount is invalid");

            var client = new RazorpayClient(_keyId, _keySecret);

            long amountInPaise = (long)(order.TotalAmount * 100);

            var options = new Dictionary<string, object>
            {
                { "amount", amountInPaise },
                { "currency", "INR" },
                { "receipt", order.OrderNumber },
                { "payment_capture", 1 }
            };

            RazorpayOrder razorOrder = client.Order.Create(options);

            return new CreateRazorpayOrderResponse
            {
                RazorpayOrderId = razorOrder["id"],
                Amount = amountInPaise,
                Currency = "INR",
                KeyId = _keyId
            };
        }

        // -------------------------
        // 2. Verify Razorpay Signature
        // -------------------------
        public Task<bool> VerifySignatureAsync(RazorpayVerifyRequest request)
        {
            var attributes = new Dictionary<string, string>
            {
                { "razorpay_order_id", request.RazorpayOrderId },
                { "razorpay_payment_id", request.RazorpayPaymentId },
                { "razorpay_signature", request.RazorpaySignature }
            };

            try
            {
                RazorpayUtils.verifyPaymentSignature(attributes);
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        // -------------------------
        // 3. Update DB status
        // -------------------------
        public async Task UpdateOrderPaymentAsync(RazorpayVerifyRequest request)
        {
            DbOrder order = await _context.OrdersSet
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId);

            if (order == null)
                throw new Exception("Order not found");

            if (order.Payment == null)
            {
                order.Payment = new DbPayment
                {
                    OrderId = order.Id,
                    PaymentMethod = "Razorpay",
                    Amount = order.TotalAmount,
                    Status = "Completed",
                    TransactionId = request.RazorpayPaymentId,
                    PaymentDate = DateTime.UtcNow
                };
                _context.PaymentsSet.Add(order.Payment);
            }
            else
            {
                order.Payment.PaymentMethod = "Razorpay";
                order.Payment.Status = "Completed";
                order.Payment.TransactionId = request.RazorpayPaymentId;
                order.Payment.PaymentDate = DateTime.UtcNow;
            }

            order.Status = "Paid";
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }
}
