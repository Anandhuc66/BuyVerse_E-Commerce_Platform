using Ecommerce_Entity.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class InvoiceService
{
    public InvoiceService()
    {
        // ❗ REQUIRED — Fixes the license error
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateInvoicePdf(Order order)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontSize(12));

                page.Header()
                    .Text($"INVOICE #{order.OrderNumber}")
                    .Bold().FontSize(18);

                page.Content().Column(col =>
                {
                    col.Item().Text($"Order Date: {order.OrderDate:dd MMM yyyy}");
                    col.Item().Text($"Customer: {order.User.FullName}");
                    col.Item().Text($"Email: {order.User.Email}");

                    col.Item().LineHorizontal(1);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Product");
                            header.Cell().Text("Qty");
                            header.Cell().Text("Price");
                            header.Cell().Text("Total");
                        });

                        foreach (var item in order.OrderDetails)
                        {
                            table.Cell().Text(item.Product.Name);
                            table.Cell().Text(item.Quantity.ToString());
                            table.Cell().Text(item.UnitPrice.ToString("0.00"));
                            table.Cell().Text((item.Quantity * item.UnitPrice).ToString("0.00"));
                        }
                    });

                    col.Item().LineHorizontal(1);

                    col.Item().AlignRight().Text($"Grand Total: ₹ {order.TotalAmount:0.00}")
                       .FontSize(16).Bold();
                });

                page.Footer().AlignCenter()
                    .Text("Thank you for shopping with us!")
                    .FontSize(10);
            });
        }).GeneratePdf();
    }
}
