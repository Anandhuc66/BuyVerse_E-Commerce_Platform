using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce_Entity.DTO
{
    public class RazorpayVerifyRequest
    {
        public string RazorpayPaymentId { get; set; }
        public string RazorpayOrderId { get; set; }
        public string RazorpaySignature { get; set; }
        public int OrderId { get; set; }  // your DB Order ID
    }
}
