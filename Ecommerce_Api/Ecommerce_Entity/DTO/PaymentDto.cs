    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    namespace Ecommerce_Entity.DTO
    {
        public class PaymentDto
        {
            public int Id { get; set; }
            public string PaymentMethod { get; set; }
            public decimal Amount { get; set; }
            public DateTime PaymentDate { get; set; }
            public string Status { get; set; }
            public string TransactionId { get; set; }
            public int OrderId { get; set; }
        }

        public class PaymentCreateDto
        {
            [Required]
            public string PaymentMethod { get; set; }

            // Amount is ignored by the server — the actual order total is used
            public decimal Amount { get; set; }

            [Required]
            public int OrderId { get; set; }
        }
        public class PaymentResponseDto
        {
            public int Id { get; set; }
            public string TransactionId { get; set; }
            public string PaymentMethod { get; set; }
            public decimal Amount { get; set; }
            public DateTime PaymentDate { get; set; }
            public string Status { get; set; }

            public OrderSummaryDto Order { get; set; }
        }

        //public class OrderSummaryDto
        //{
        //    public string OrderNumber { get; set; }
        //    public decimal TotalAmount { get; set; }
        //    public string ShippingAddress { get; set; }
        //    public string Status { get; set; }
        //}
 
        public class PaymentUpdateDto : PaymentCreateDto
        {
            public int Id { get; set; }
        }


    }
