using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce_Entity.DTO
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public string UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string DeliveryStatus { get; set; }
        public int ShippingAddressId { get; set; }


        public List<OrderDetailDto> OrderDetails { get; set; }
        public PaymentDto Payment { get; set; }
    }

    public class OrderCreateDto
    {
        public string? UserId { get; set; } // Overridden by server from JWT

        [Required]
        public int ShippingAddressId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Order must contain at least one product.")]
        public List<OrderDetailCreateDto> OrderDetails { get; set; }

        public string? Status { get; set; } // Only "COD" is accepted; anything else defaults to "Pending"
    }
    public class OrderUpdateDto : OrderCreateDto
    {
        public int Id { get; set; }
    }

    public class OrderStatusUpdateDto
    {
        public string Status { get; set; }
    }
    public class OrderDeliveryStatusUpdateDto
    {
        [Required]
        public string DeliveryStatus { get; set; }
    }
    public class OrderSummaryDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string DeliveryStatus { get; set; }

        public string PaymentMethod { get; set; }
        public string ShippingAddress { get; set; }

        public SupplierDto Supplier { get; set; }

        public List<OrderDetailLineDto> Products { get; set; }
    }
    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }

        public SupplierDto Supplier { get; set; }
    }
    public class OrderDetailLineDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public string ProductImage { get; set; }
    }
    public class SupplierOrderDto
    {
        public int SupplierId { get; set; }
        public string BusinessName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }

}
