using Ecommerce_Entity.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce_Service.Repository
{
    public interface IRazorpayService
    {
        Task<CreateRazorpayOrderResponse> CreateOrderAsync(int orderId);
        Task<bool> VerifySignatureAsync(RazorpayVerifyRequest request);
        Task UpdateOrderPaymentAsync(RazorpayVerifyRequest request);
    }
}
