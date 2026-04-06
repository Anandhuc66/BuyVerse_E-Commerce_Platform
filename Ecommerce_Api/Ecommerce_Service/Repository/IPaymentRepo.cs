using Ecommerce_Common;
using Ecommerce_Entity.DTO;
using Ecommerce_Entity.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ecommerce_Service.Repository
{
    public interface IPaymentRepo
    {

        Task<Result<List<Payment>>> GetAll();
        Task<Result<Payment>> GetById(int id);
        Task<Result<Payment>> Add(PaymentCreateDto model);
        Task<Result<Payment>> Update(PaymentUpdateDto model);
        Task<Result<bool>> Delete(int id);
    }
}
