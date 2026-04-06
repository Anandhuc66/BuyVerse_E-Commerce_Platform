using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce_Entity.DTO
{
    public class CreateRazorpayOrderResponse
    {
        public string RazorpayOrderId { get; set; }
        public long Amount { get; set; } // in paise
        public string Currency { get; set; }
        public string KeyId { get; set; } // frontend uses this
    }
}
