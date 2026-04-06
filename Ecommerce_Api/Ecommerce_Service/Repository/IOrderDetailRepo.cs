using Ecommerce_Common;
using Ecommerce_Entity.DTO;
using Ecommerce_Entity.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ecommerce_Service.Repository
{
    public interface IOrderDetailRepo
    {
        Task<Result<List<OrderDetail>>> GetAll();
        Task<Result<OrderDetail>> GetById(int id);
        //Task<Result<OrderDetail>> Add(OrderDetailCreateDto model);
        Task<Result<OrderDetail>> Update(OrderDetailUpdateDto model);
        Task<Result<bool>> Delete(int id);
    }
}
