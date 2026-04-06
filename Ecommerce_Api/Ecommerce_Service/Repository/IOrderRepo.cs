using Ecommerce_Common;
using Ecommerce_Entity.DTO;
using Ecommerce_Entity.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ecommerce_Service.Repository
{
    public interface IOrderRepo
    {
        // Admin: Get all orders (detailed)
        Task<Result<List<OrderDetailedDto>>> GetAll();
        Task<Result<PagedResult<OrderDetailedDto>>> GetAllPaged(int pageNumber, int pageSize);

        // Get order by ID (detailed)
        Task<Result<OrderDetailedDto>> GetOrderDetailsById(int id);

        // Create order
        Task<Result<Order>> Add(OrderCreateDto model);

        // Update order
        Task<Result<Order>> Update(OrderUpdateDto model);

        // Delete order
        Task<Result<bool>> Delete(int id);

        // User: Get user’s orders
        Task<Result<List<OrderSummaryDto>>> GetOrdersByUser(string userId);
        Task<Result<PagedResult<OrderSummaryDto>>> GetOrdersByUserPaged(string userId, int pageNumber, int pageSize);

        // Supplier: Get supplier related orders
        Task<Result<List<OrderSummaryDto>>> GetOrdersBySupplier(int supplierId);

        // Update order status only
        Task<Result<Order>> UpdateOrderStatus(int orderId, string status);


    }
}
