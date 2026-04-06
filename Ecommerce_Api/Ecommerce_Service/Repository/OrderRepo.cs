using Ecommerce_Common;
using Ecommerce_Entity.Data;
using Ecommerce_Entity.DTO;
using Ecommerce_Entity.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce_Service.Repository
{
    public class OrderRepo : IOrderRepo
    {
        private readonly ApplicationDbContext _context;
        public OrderRepo(ApplicationDbContext context) => _context = context;

        public async Task<Result<List<OrderDetailedDto>>> GetAll()
        {
            var result = new Result<List<OrderDetailedDto>>();

            var orders = await _context.OrdersSet
                .AsNoTracking()
                .Include(o => o.User)
                .Include(o => o.ShippingAddress)
                .Include(o => o.Payment)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.Supplier)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            if (!orders.Any())
            {
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "No orders found" });
                return result;
            }

            result.Response = orders.Select(o => new OrderDetailedDto
            {
                OrderId = o.Id,
                OrderNumber = o.OrderNumber,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                DeliveryStatus = o.DeliveryStatus,

                User = new UserShortInfoDto
                {
                    UserId = o.User.Id,
                    FullName = o.User.FullName,
                    Email = o.User.Email,
                    PhoneNumber = o.User.PhoneNumber
                },

                ShippingAddress = o.ShippingAddress == null ? null : new ShippingAddressDto
                {
                    AddressId = o.ShippingAddress.Id,
                    FullAddress = o.ShippingAddress.FullAddress,
                    City = o.ShippingAddress.City,
                    State = o.ShippingAddress.State,
                    ZipCode = o.ShippingAddress.ZipCode,
                    Country = o.ShippingAddress.Country
                },

                Products = o.OrderDetails.Select(od => new OrderProductWithSupplierDto
                {
                    ProductId = od.Product.Id,
                    Name = od.Product.Name,
                    Quantity = od.Quantity,
                    UnitPrice = od.UnitPrice,

                    Supplier = od.Product.Supplier == null ? null : new SupplierOrderDto
                    {
                        SupplierId = od.Product.Supplier.Id,
                        BusinessName = od.Product.Supplier.CompanyName,
                        Email = od.Product.Supplier.ContactEmail,
                        PhoneNumber = od.Product.Supplier.Phone
                    }
                }).ToList(),

                Payment = o.Payment == null ? null : new PaymentShortDto
                {
                    PaymentId = o.Payment.Id,
                    TransactionId = o.Payment.TransactionId,
                    PaymentMethod = o.Payment.PaymentMethod,
                    PaymentDate = o.Payment.PaymentDate,
                    Status = o.Payment.Status
                }

            }).ToList();

            return result;
        }

        public async Task<Result<PagedResult<OrderDetailedDto>>> GetAllPaged(int pageNumber, int pageSize)
        {
            var result = new Result<PagedResult<OrderDetailedDto>>();

            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _context.OrdersSet
                .AsNoTracking()
                .Include(o => o.User)
                .Include(o => o.ShippingAddress)
                .Include(o => o.Payment)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.Supplier);

            var totalCount = await _context.OrdersSet.CountAsync();

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = orders.Select(o => new OrderDetailedDto
            {
                OrderId = o.Id,
                OrderNumber = o.OrderNumber,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                DeliveryStatus = o.DeliveryStatus,

                User = new UserShortInfoDto
                {
                    UserId = o.User.Id,
                    FullName = o.User.FullName,
                    Email = o.User.Email,
                    PhoneNumber = o.User.PhoneNumber
                },

                ShippingAddress = o.ShippingAddress == null ? null : new ShippingAddressDto
                {
                    AddressId = o.ShippingAddress.Id,
                    FullAddress = o.ShippingAddress.FullAddress,
                    City = o.ShippingAddress.City,
                    State = o.ShippingAddress.State,
                    ZipCode = o.ShippingAddress.ZipCode,
                    Country = o.ShippingAddress.Country
                },

                Products = o.OrderDetails.Select(od => new OrderProductWithSupplierDto
                {
                    ProductId = od.Product.Id,
                    Name = od.Product.Name,
                    Quantity = od.Quantity,
                    UnitPrice = od.UnitPrice,

                    Supplier = od.Product.Supplier == null ? null : new SupplierOrderDto
                    {
                        SupplierId = od.Product.Supplier.Id,
                        BusinessName = od.Product.Supplier.CompanyName,
                        Email = od.Product.Supplier.ContactEmail,
                        PhoneNumber = od.Product.Supplier.Phone
                    }
                }).ToList(),

                Payment = o.Payment == null ? null : new PaymentShortDto
                {
                    PaymentId = o.Payment.Id,
                    TransactionId = o.Payment.TransactionId,
                    PaymentMethod = o.Payment.PaymentMethod,
                    PaymentDate = o.Payment.PaymentDate,
                    Status = o.Payment.Status
                }
            }).ToList();

            result.Response = new PagedResult<OrderDetailedDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            return result;
        }


        //public async Task<Result<Order>> GetById(int id)
        //{
        //    var result = new Result<Order>();
        //    var order = await _context.OrdersSet
        //                              .Include(o => o.OrderDetails)
        //                              .ThenInclude(od => od.Product)
        //                              .Include(o => o.Payment)
        //                              .Include(o => o.User)
        //                              .FirstOrDefaultAsync(o => o.Id == id);
        //    if (order != null) result.Response = order;
        //    else result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "Order not found" });
        //    return result;
        //}

        //public async Task<Result<Order>> Add(OrderCreateDto model)
        //{
        //    var result = new Result<Order>();

        //    // ✅ Create the main order record
        //    var order = new Order
        //    {
        //        UserId = model.UserId,
        //        ShippingAddress = model.ShippingAddress,
        //        TotalAmount = model.TotalAmount,
        //        Status = model.Status ?? "Pending",
        //        OrderDate = DateTime.UtcNow,

        //        // 👇 FIX: Generate a unique order number (required by DB)
        //        OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmssfff}"
        //    };

        //    await _context.OrdersSet.AddAsync(order);
        //    await _context.SaveChangesAsync();

        //    // ✅ Add OrderDetails
        //    if (model.OrderDetails != null && model.OrderDetails.Any())
        //    {
        //        foreach (var od in model.OrderDetails)
        //        {
        //            var orderDetail = new OrderDetail
        //            {
        //                OrderId = order.Id,
        //                ProductId = od.ProductId,
        //                Quantity = od.Quantity,
        //                UnitPrice = od.UnitPrice
        //            };
        //            await _context.OrderDetailsSet.AddAsync(orderDetail);
        //        }

        //        await _context.SaveChangesAsync();
        //    }

        //    result.Response = order;
        //    result.Message = "Order created successfully";
        //    return result;
        public async Task<Result<Order>> Add(OrderCreateDto model)
        {
            var result = new Result<Order>();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = new Order
                {
                    UserId = model.UserId,
                    ShippingAddressId = model.ShippingAddressId,
                    Status = model.Status == "COD" ? "COD" : "Pending",
                    DeliveryStatus = "Pending",
                    OrderDate = DateTime.UtcNow,
                    OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Random.Shared.Next(1000, 9999)}",
                    TotalAmount = 0
                };

                await _context.OrdersSet.AddAsync(order);
                await _context.SaveChangesAsync();

                if (model.OrderDetails == null || !model.OrderDetails.Any())
                    throw new Exception("Order must contain at least one product.");

                foreach (var od in model.OrderDetails)
                {
                    var product = await _context.ProductsSet.FindAsync(od.ProductId);
                    if (product == null)
                        throw new Exception($"Product with ID {od.ProductId} not found.");

                    // SECURITY: Atomic stock deduction — prevents race condition (overselling)
                    var rowsAffected = await _context.Database.ExecuteSqlRawAsync(
                        "UPDATE ProductsSet SET StockQuantity = StockQuantity - {0}, UpdatedAt = {1} WHERE Id = {2} AND StockQuantity >= {0}",
                        od.Quantity, DateTime.UtcNow, od.ProductId);

                    if (rowsAffected == 0)
                        throw new Exception($"Not enough stock for product: {product.Name}");

                    // Refresh entity from DB to get the updated stock
                    await _context.Entry(product).ReloadAsync();

                    var unitPrice = product.Price;

                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.Id,
                        ProductId = product.Id,
                        Quantity = od.Quantity,
                        UnitPrice = unitPrice,
                        TotalPrice = unitPrice * od.Quantity
                    };

                    order.TotalAmount += unitPrice * od.Quantity;

                    await _context.OrderDetailsSet.AddAsync(orderDetail);
                }

                // Create Payment record based on order status
                var payment = new Payment
                {
                    OrderId = order.Id,
                    Amount = order.TotalAmount,
                    PaymentDate = DateTime.UtcNow,
                    PaymentMethod = model.Status == "COD" ? "Cash on Delivery" : "Online Payment",
                    Status = model.Status == "COD" ? "Pending" : "Awaiting Payment",
                    TransactionId = ""
                };
                await _context.PaymentsSet.AddAsync(payment);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                result.Response = order;
                result.Message = "Order created successfully, and stock updated.";
                return result;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                result.Errors.Add(new Errors { ErrorCode = 400, ErrorMessage = ex.Message });
                return result;
            }
        }




        //public async Task<Result<Order>> Update(OrderUpdateDto model)
        //{
        //    var result = new Result<Order>();
        //    var order = await _context.OrdersSet.Include(o => o.OrderDetails)
        //                                        .FirstOrDefaultAsync(o => o.Id == model.Id);
        //    if (order == null)
        //    {
        //        result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "Order not found" });
        //        return result;
        //    }

        //    order.ShippingAddress = model.ShippingAddress;
        //    order.Status = model.Status;
        //    order.TotalAmount = model.TotalAmount;

        //    _context.OrdersSet.Update(order);
        //    await _context.SaveChangesAsync();
        //    result.Response = order;
        //    result.Message = "Order updated successfully";
        //    return result;
        //}
        public async Task<Result<Order>> Update(OrderUpdateDto model)
        {
            var result = new Result<Order>();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = await _context.OrdersSet
                    .Include(o => o.OrderDetails)
                    .FirstOrDefaultAsync(o => o.Id == model.Id);

                if (order == null)
                {
                    result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "Order not found" });
                    return result;
                }

                // Only update shipping address and payment status
                // DeliveryStatus is managed by the dedicated UpdateOrderStatus endpoint
                order.ShippingAddressId = model.ShippingAddressId;

                // Validate payment status values
                var validPaymentStatuses = new[] { "Pending", "Paid", "COD", "Cancelled" };
                if (!string.IsNullOrEmpty(model.Status) && validPaymentStatuses.Contains(model.Status))
                {
                    order.Status = model.Status;
                }

                order.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                result.Response = order;
                result.Message = "Order updated successfully";

                return result;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                result.Errors.Add(new Errors { ErrorCode = 400, ErrorMessage = ex.Message });
                return result;
            }
        }


        public async Task<Result<bool>> Delete(int id)
        {
            var result = new Result<bool>();
            var order = await _context.OrdersSet
                .Include(o => o.OrderDetails)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "Order not found" });
                return result;
            }

            // Restore stock if order was NOT already cancelled
            if (order.DeliveryStatus != "Cancelled" && order.OrderDetails != null)
            {
                foreach (var detail in order.OrderDetails)
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "UPDATE ProductsSet SET StockQuantity = StockQuantity + {0}, UpdatedAt = {1} WHERE Id = {2}",
                        detail.Quantity, DateTime.UtcNow, detail.ProductId);
                }
            }

            // Remove related entities first
            if (order.OrderDetails != null && order.OrderDetails.Any())
                _context.OrderDetailsSet.RemoveRange(order.OrderDetails);

            if (order.Payment != null)
                _context.PaymentsSet.Remove(order.Payment);

            _context.OrdersSet.Remove(order);
            await _context.SaveChangesAsync();
            result.Response = true;
            result.Message = "Order deleted successfully";
            return result;
        }
        public async Task<Result<List<OrderSummaryDto>>> GetOrdersByUser(string userId)
        {
            var result = new Result<List<OrderSummaryDto>>();

            var orders = await _context.OrdersSet
                .AsNoTracking()
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.Images)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product.Supplier)
                .Include(o => o.Payment)
                .Include(o => o.ShippingAddress)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            result.Response = orders.Select(o => new OrderSummaryDto
            {
                OrderId = o.Id,
                OrderNumber = o.OrderNumber,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                DeliveryStatus = o.DeliveryStatus,
                PaymentMethod = o.Payment?.PaymentMethod ?? "N/A",
                ShippingAddress = o.ShippingAddress != null 
                    ? $"{o.ShippingAddress.FullAddress}, {o.ShippingAddress.City}, {o.ShippingAddress.State} - {o.ShippingAddress.ZipCode}"
                    : "N/A",

                Supplier = o.OrderDetails.Any() && o.OrderDetails.First().Product?.Supplier != null
                    ? new SupplierDto
                    {
                        SupplierId = o.OrderDetails.First().Product.Supplier.Id,
                        CompanyName = o.OrderDetails.First().Product.Supplier.CompanyName,
                        ContactEmail = o.OrderDetails.First().Product.Supplier.ContactEmail,
                        Phone = o.OrderDetails.First().Product.Supplier.Phone
                    }
                    : null,

                Products = o.OrderDetails.Select(od => new OrderDetailLineDto
                {
                    ProductId = od.ProductId,
                    ProductName = od.Product.Name,
                    Quantity = od.Quantity,
                    UnitPrice = od.UnitPrice,
                    ProductImage = od.Product.Images.FirstOrDefault()?.ImageUrl
                }).ToList()
            }).ToList();

            return result;
        }

        public async Task<Result<PagedResult<OrderSummaryDto>>> GetOrdersByUserPaged(string userId, int pageNumber, int pageSize)
        {
            var result = new Result<PagedResult<OrderSummaryDto>>();

            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _context.OrdersSet
                .AsNoTracking()
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.Images)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product.Supplier)
                .Include(o => o.Payment)
                .Include(o => o.ShippingAddress)
                .Where(o => o.UserId == userId);

            var totalCount = await _context.OrdersSet.CountAsync(o => o.UserId == userId);

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = orders.Select(o => new OrderSummaryDto
            {
                OrderId = o.Id,
                OrderNumber = o.OrderNumber,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                DeliveryStatus = o.DeliveryStatus,
                PaymentMethod = o.Payment?.PaymentMethod ?? "N/A",
                ShippingAddress = o.ShippingAddress != null
                    ? $"{o.ShippingAddress.FullAddress}, {o.ShippingAddress.City}, {o.ShippingAddress.State} - {o.ShippingAddress.ZipCode}"
                    : "N/A",

                Supplier = o.OrderDetails.Any() && o.OrderDetails.First().Product?.Supplier != null
                    ? new SupplierDto
                    {
                        SupplierId = o.OrderDetails.First().Product.Supplier.Id,
                        CompanyName = o.OrderDetails.First().Product.Supplier.CompanyName,
                        ContactEmail = o.OrderDetails.First().Product.Supplier.ContactEmail,
                        Phone = o.OrderDetails.First().Product.Supplier.Phone
                    }
                    : null,

                Products = o.OrderDetails.Select(od => new OrderDetailLineDto
                {
                    ProductId = od.ProductId,
                    ProductName = od.Product.Name,
                    Quantity = od.Quantity,
                    UnitPrice = od.UnitPrice,
                    ProductImage = od.Product.Images.FirstOrDefault()?.ImageUrl
                }).ToList()
            }).ToList();

            result.Response = new PagedResult<OrderSummaryDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            return result;
        }

        public async Task<Result<List<OrderSummaryDto>>> GetOrdersBySupplier(int supplierId)
        {
            var result = new Result<List<OrderSummaryDto>>();

            var orders = await _context.OrdersSet
                .AsNoTracking()
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.Images)
                .Include(o => o.Payment)
                .Include(o => o.ShippingAddress)
                .Where(o => o.OrderDetails.Any(od => od.Product.SupplierId == supplierId))
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            result.Response = orders.Select(o => new OrderSummaryDto
            {
                OrderId = o.Id,
                OrderNumber = o.OrderNumber,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                DeliveryStatus = o.DeliveryStatus,
                PaymentMethod = o.Payment?.PaymentMethod ?? "N/A",
                ShippingAddress = o.ShippingAddress != null
                    ? $"{o.ShippingAddress.FullAddress}, {o.ShippingAddress.City}, {o.ShippingAddress.State} - {o.ShippingAddress.ZipCode}"
                    : "N/A",

                Products = o.OrderDetails
                    .Where(od => od.Product.SupplierId == supplierId)
                    .Select(od => new OrderDetailLineDto
                    {
                        ProductId = od.ProductId,
                        ProductName = od.Product.Name,
                        Quantity = od.Quantity,
                        UnitPrice = od.UnitPrice,
                        ProductImage = od.Product.Images.FirstOrDefault()?.ImageUrl
                    }).ToList()
            }).ToList();

            return result;
        }
        public async Task<Result<OrderDetailedDto>> GetOrderDetailsById(int id)
        {
            var result = new Result<OrderDetailedDto>();

            var order = await _context.OrdersSet
                .AsNoTracking()
                .Include(o => o.User)
                .Include(o => o.ShippingAddress)
                .Include(o => o.Payment)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.Supplier)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "Order not found" });
                return result;
            }

            var dto = new OrderDetailedDto
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                DeliveryStatus = order.DeliveryStatus,

                User = new UserShortInfoDto
                {
                    UserId = order.User.Id,
                    FullName = order.User.FullName,
                    Email = order.User.Email,
                    PhoneNumber = order.User.PhoneNumber
                },

                ShippingAddress = order.ShippingAddress == null ? null : new ShippingAddressDto
                {
                    AddressId = order.ShippingAddress.Id,
                    FullAddress = order.ShippingAddress.FullAddress,
                    City = order.ShippingAddress.City,
                    State = order.ShippingAddress.State,
                    ZipCode = order.ShippingAddress.ZipCode,
                    Country = order.ShippingAddress.Country
                },

                Products = order.OrderDetails.Select(od => new OrderProductWithSupplierDto
                {
                    ProductId = od.Product.Id,
                    Name = od.Product.Name,
                    UnitPrice = od.UnitPrice,
                    Quantity = od.Quantity,

                    Supplier = od.Product.Supplier == null ? null : new SupplierOrderDto
                    {
                        SupplierId = od.Product.Supplier.Id,
                        BusinessName = od.Product.Supplier.CompanyName,
                        Email = od.Product.Supplier.ContactEmail,
                        PhoneNumber = od.Product.Supplier.Phone
                    }
                }).ToList(),

                Payment = order.Payment == null ? null : new PaymentShortDto
                {
                    PaymentId = order.Payment.Id,
                    TransactionId = order.Payment.TransactionId,
                    PaymentMethod = order.Payment.PaymentMethod,
                    PaymentDate = order.Payment.PaymentDate,
                    Status = order.Payment.Status
                }
            };

            result.Response = dto;
            return result;
        }

        public async Task<Result<Order>> UpdateOrderStatus(int orderId, string status)
        {
            var result = new Result<Order>();
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = await _context.OrdersSet
                    .Include(o => o.OrderDetails)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "Order not found" });
                    return result;
                }

                var validStatuses = new[] { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };
                if (!validStatuses.Contains(status))
                {
                    result.Errors.Add(new Errors { ErrorCode = 400, ErrorMessage = $"Invalid delivery status. Valid values: {string.Join(", ", validStatuses)}" });
                    return result;
                }

                var oldDeliveryStatus = order.DeliveryStatus;

                // Restore stock if cancelling a non-cancelled order
                if (oldDeliveryStatus != "Cancelled" && status == "Cancelled")
                {
                    foreach (var detail in order.OrderDetails)
                    {
                        // Atomic stock restoration — mirrors deduction pattern in Add()
                        await _context.Database.ExecuteSqlRawAsync(
                            "UPDATE ProductsSet SET StockQuantity = StockQuantity + {0}, UpdatedAt = {1} WHERE Id = {2}",
                            detail.Quantity, DateTime.UtcNow, detail.ProductId);
                    }

                    // Also mark payment as cancelled
                    var payment = await _context.PaymentsSet.FirstOrDefaultAsync(p => p.OrderId == orderId);
                    if (payment != null)
                    {
                        payment.Status = "Cancelled";
                    }
                }

                // Prevent resurrecting cancelled orders
                if (oldDeliveryStatus == "Cancelled" && status != "Cancelled")
                {
                    result.Errors.Add(new Errors { ErrorCode = 400, ErrorMessage = "Cannot change status of a cancelled order" });
                    return result;
                }

                order.DeliveryStatus = status;
                order.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                result.Response = order;
                result.Message = status == "Cancelled" 
                    ? "Order cancelled and stock restored."
                    : $"Delivery status updated to {status}.";
                return result;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                result.Errors.Add(new Errors { ErrorCode = 400, ErrorMessage = ex.Message });
                return result;
            }
        }


    }
}
