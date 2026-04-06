using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce_Entity.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeliveryStatus",
                table: "OrdersSet",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.CreateIndex(
                name: "IX_CartsSet_UserId",
                table: "CartsSet",
                column: "UserId");

            // Migrate existing data: Map old Status to separate Payment Status + Delivery Status
            // Orders with Status = 'Paid' → PaymentStatus stays 'Paid', DeliveryStatus = 'Pending'
            // Orders with Status = 'COD' → PaymentStatus stays 'COD', DeliveryStatus = 'Pending'
            // Orders with Status = 'Pending' → PaymentStatus stays 'Pending', DeliveryStatus = 'Pending'
            // Orders with delivery statuses (Processing/Shipped/Delivered/Cancelled) → DeliveryStatus = old Status, PaymentStatus = 'Paid'
            migrationBuilder.Sql(@"
                UPDATE OrdersSet SET DeliveryStatus = 
                    CASE 
                        WHEN Status IN ('Processing', 'Shipped', 'Delivered', 'Cancelled') THEN Status
                        ELSE 'Pending'
                    END;
                UPDATE OrdersSet SET Status = 'Paid' 
                    WHERE Status IN ('Processing', 'Shipped', 'Delivered', 'Cancelled');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CartsSet_UserId",
                table: "CartsSet");

            migrationBuilder.DropColumn(
                name: "DeliveryStatus",
                table: "OrdersSet");
        }
    }
}
