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
    public class OrderDetailRepo : IOrderDetailRepo
    {
        private readonly ApplicationDbContext _context;
        public OrderDetailRepo(ApplicationDbContext context) => _context = context;

        public async Task<Result<List<OrderDetail>>> GetAll()
        {
            var result = new Result<List<OrderDetail>>();

            var list = await _context.OrderDetailsSet
                .Include(od => od.Product)                                               
                .Include(od => od.Order)                                 
                .ToListAsync();

            if 
                (list.Any()) result.Response = list;
            else 
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "No order details found" });

            return result;
        }

        public async Task<Result<OrderDetail>> GetById(int id)
        {
            var result = new Result<OrderDetail>();
            var od = await _context.OrderDetailsSet
                .Include(od => od.Product)
                .Include(od => od.Order)
                .FirstOrDefaultAsync(od => od.Id == id);

            if 
                (od != null) result.Response = od;
            else 
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "Order detail not found" });

            return result;
        }

        //public async Task<Result<OrderDetail>> Add(OrderDetailCreateDto model)
        //{
        //    var result = new Result<OrderDetail>();

        //    if (model.Quantity <= 0)
        //    {
        //        result.Errors.Add(new Errors { ErrorCode = 400, ErrorMessage = "Quantity must be greater than zero" });
        //        return result;
        //    }

        //    var product = await _context.ProductsSet.FindAsync(model.ProductId);
        //    if (product == null)
        //    {
        //        result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "Product not found" });
        //        return result;
        //    }

        //    var od = new OrderDetail
        //    {
        //        OrderId = model.OrderId,
        //        ProductId = model.ProductId,
        //        Quantity = model.Quantity,
        //        UnitPrice = model.UnitPrice
        //    };

        //    await _context.OrderDetailsSet.AddAsync(od);
        //    await _context.SaveChangesAsync();

        //    result.Response = od;
        //    result.Message = "Order detail added successfully";
        //    return result;
        //}

        public async Task<Result<OrderDetail>> Update(OrderDetailUpdateDto model)
        {
            var result = new Result<OrderDetail>();
            var od = await _context.OrderDetailsSet.FindAsync(model.Id);
            if (od == null)
            {
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "Order detail not found" });
                return result;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                od.Quantity = model.Quantity;
                od.UnitPrice = model.UnitPrice;
                od.TotalPrice = model.Quantity * model.UnitPrice;

                _context.OrderDetailsSet.Update(od);

                // Recalculate parent order total in same transaction
                var orderDetails = await _context.OrderDetailsSet
                    .Where(d => d.OrderId == od.OrderId)
                    .ToListAsync();
                var order = await _context.OrdersSet.FindAsync(od.OrderId);
                if (order != null)
                {
                    order.TotalAmount = orderDetails.Sum(d => d.TotalPrice);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                result.Response = od;
                result.Message = "Order detail updated successfully";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                result.Errors.Add(new Errors { ErrorCode = 400, ErrorMessage = ex.Message });
            }
            return result;
        }

        public async Task<Result<bool>> Delete(int id)
        {
            var result = new Result<bool>();
            var od = await _context.OrderDetailsSet.FindAsync(id);
            if (od == null)
            {
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "Order detail not found" });
                return result;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var orderId = od.OrderId;
                _context.OrderDetailsSet.Remove(od);

                // Recalculate parent order total in same transaction
                var order = await _context.OrdersSet.FindAsync(orderId);
                if (order != null)
                {
                    var remainingDetails = await _context.OrderDetailsSet
                        .Where(d => d.OrderId == orderId && d.Id != id)
                        .ToListAsync();
                    order.TotalAmount = remainingDetails.Sum(d => d.TotalPrice);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                result.Response = true;
                result.Message = "Order detail deleted successfully";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                result.Errors.Add(new Errors { ErrorCode = 400, ErrorMessage = ex.Message });
            }
            return result;
        }
    }
}
