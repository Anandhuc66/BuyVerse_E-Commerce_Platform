using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce_Entity.DTO
{
    public class OrderDetailDto
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public string OrderNumber { get; set; }

        public int ProductId { get; set; }
        public string ProductName { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class OrderDetailCreateDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, 10000, ErrorMessage = "Quantity must be between 1 and 10000.")]
        public int Quantity { get; set; }
    }

    public class OrderDetailUpdateDto
    {
        public int Id { get; set; } // required
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
    public class OrderDetailedDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string DeliveryStatus { get; set; }

        public UserShortInfoDto User { get; set; }
        public ShippingAddressDto ShippingAddress { get; set; }

        public List<OrderProductWithSupplierDto> Products { get; set; }
        public PaymentShortDto Payment { get; set; }
    }

    public class UserShortInfoDto
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class ShippingAddressDto
    {
        public int AddressId { get; set; }
        public string FullAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }
    }

    public class OrderProductWithSupplierDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;

        public SupplierOrderDto Supplier { get; set; }
    }

    public class PaymentShortDto
    {
        public int PaymentId { get; set; }
        public string TransactionId { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; }
    }

}
