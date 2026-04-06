using Ecommerce_Common;
using Ecommerce_Entity.Data;
using Ecommerce_Entity.DTO;
using Ecommerce_Entity.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce_Service.Repository
{
    public class PaymentRepo : IPaymentRepo
    {
        private readonly ApplicationDbContext _context;
        public PaymentRepo(ApplicationDbContext context) => _context = context;

        public async Task<Result<List<Payment>>> GetAll()
        {
            var result = new Result<List<Payment>>();
            var list = await _context.PaymentsSet.AsNoTracking()
                                                 .Include(p => p.Order)
                                                 .ThenInclude(o => o.User)
                                                 .ToListAsync();
            if (list.Any()) result.Response = list;
            else result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "No payments found" });
            return result;
        }

        public async Task<Result<Payment>> GetById(int id)
        {
            var result = new Result<Payment>();
            var payment = await _context.PaymentsSet.Include(p => p.Order)
                                                    .ThenInclude(o => o.User)
                                                    .FirstOrDefaultAsync(p => p.Id == id);
            if (payment != null) result.Response = payment;
            else result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "Payment not found" });
            return result;
        }

        public async Task<Result<Payment>> Add(PaymentCreateDto model)
        {
            var result = new Result<Payment>();
            var payment = new Payment
            {
                PaymentMethod = model.PaymentMethod,
                Amount = model.Amount,
                OrderId = model.OrderId,
                Status = "Pending",
                PaymentDate = System.DateTime.UtcNow,
                TransactionId = Guid.NewGuid().ToString()
            };

            await _context.PaymentsSet.AddAsync(payment);
            await _context.SaveChangesAsync();
            result.Response = payment;
            result.Message = "Payment added successfully";
            return result;
        }

        public async Task<Result<Payment>> Update(PaymentUpdateDto model)
        {
            var result = new Result<Payment>();
            var payment = await _context.PaymentsSet.FindAsync(model.Id);
            if (payment == null)
            {
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "Payment not found" });
                return result;
            }

            payment.PaymentMethod = model.PaymentMethod;
            // Do NOT allow changing OrderId or Amount — they are immutable after creation

            _context.PaymentsSet.Update(payment);
            await _context.SaveChangesAsync();
            result.Response = payment;
            result.Message = "Payment updated successfully";
            return result;
        }

        public async Task<Result<bool>> Delete(int id)
        {
            var result = new Result<bool>();
            var payment = await _context.PaymentsSet.FindAsync(id);
            if (payment == null)
            {
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "Payment not found" });
                return result;
            }

            _context.PaymentsSet.Remove(payment);
            await _context.SaveChangesAsync();
            result.Response = true;
            result.Message = "Payment deleted successfully";
            return result;
        }
    }
}
