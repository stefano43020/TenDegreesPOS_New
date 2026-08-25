using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TenDegreesPOS_New.Models;
using System.Globalization;

namespace TenDegreesPOS_New.Services
{
    public class PDFExportService
    {
        static PDFExportService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GenerateSalesReportPDF(
            DateTime startDate,
            DateTime endDate,
            decimal totalSales,
            int totalItemsSold,
            int totalTransactions,
            List<SalesProductPerformance> productPerformance,
            List<SalesChartDataPoint> chartData)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    // Header
                    page.Header()
                        .ShowOnce()
                        .Column(col =>
                        {
                            col.Item().Text("TEN DEGREES POS")
                                .FontSize(20)
                                .Bold()
                                .FontColor(Colors.Grey.Darken4)
                                .AlignCenter();

                            col.Item().Text("Sales Report")
                                .FontSize(16)
                                .Bold()
                                .AlignCenter()
                                .FontColor(Colors.Blue.Medium);

                            col.Item().Text($"Period: {startDate:MMMM dd, yyyy} - {endDate:MMMM dd, yyyy}")
                                .FontSize(10)
                                .AlignCenter()
                                .FontColor(Colors.Grey.Darken2);

                            col.Item().PaddingTop(10).LineHorizontal(1);
                        });

                    // Content
                    page.Content().Column(col =>
                    {
                        // Summary Cards Section
                        col.Item().PaddingTop(10).Row(row =>
                        {
                            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Column(summary =>
                            {
                                summary.Item().Text("Total Sales").FontSize(9).FontColor(Colors.Grey.Darken2);
                                summary.Item().Text($"₱{totalSales:N2}").FontSize(14).Bold().FontColor(Colors.Green.Medium);
                            });

                            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Column(summary =>
                            {
                                summary.Item().Text("Items Sold").FontSize(9).FontColor(Colors.Grey.Darken2);
                                summary.Item().Text($"{totalItemsSold:N0}").FontSize(14).Bold().FontColor(Colors.Blue.Medium);
                            });

                            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Column(summary =>
                            {
                                summary.Item().Text("Transactions").FontSize(9).FontColor(Colors.Grey.Darken2);
                                summary.Item().Text($"{totalTransactions:N0}").FontSize(14).Bold().FontColor(Colors.Orange.Medium);
                            });

                            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Column(summary =>
                            {
                                decimal averageTransaction = totalTransactions > 0 ? totalSales / totalTransactions : 0;
                                summary.Item().Text("Average Transaction").FontSize(9).FontColor(Colors.Grey.Darken2);
                                summary.Item().Text($"₱{averageTransaction:N2}").FontSize(14).Bold().FontColor(Colors.Purple.Medium);
                            });
                        });

                        // Sales Trend Section
                        if (chartData != null && chartData.Any())
                        {
                            col.Item().PaddingTop(15).Text("Sales Trend").FontSize(12).Bold().FontColor(Colors.Grey.Darken3);
                            col.Item().PaddingTop(5).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(1);
                                });

                                // Headers
                                table.Header(header =>
                                {
                                    header.Cell().Text("Date").Bold().FontSize(9);
                                    header.Cell().Text("Sales Amount").Bold().FontSize(9).AlignRight();
                                });

                                decimal maxSales = chartData.Max(d => d.Sales);
                                foreach (var data in chartData)
                                {
                                    table.Cell().Text(data.Date.ToString("MMM dd, yyyy")).FontSize(9);
                                    table.Cell().Text($"₱{data.Sales:N2}").FontSize(9).AlignRight()
                                        .FontColor(data.Sales > 0 ? Colors.Black : Colors.Grey.Medium);
                                }
                            });
                        }

                        // Product Performance Table
                        col.Item().PaddingTop(15).Text("Product Performance").FontSize(12).Bold().FontColor(Colors.Grey.Darken3);
                        col.Item().PaddingTop(5).Border(1).BorderColor(Colors.Grey.Lighten2).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Product").Bold().FontSize(9);
                                header.Cell().Text("Units Sold").Bold().FontSize(9).AlignRight();
                                header.Cell().Text("Revenue").Bold().FontSize(9).AlignRight();
                                header.Cell().Text("% of Sales").Bold().FontSize(9).AlignRight();
                                header.Cell().Text("Performance").Bold().FontSize(9).AlignCenter();
                            });

                            foreach (var product in productPerformance)
                            {
                                table.Cell().Text(product.ProductName).FontSize(9);
                                table.Cell().Text($"{product.UnitsSold:N0}").FontSize(9).AlignRight();
                                table.Cell().Text($"₱{product.Revenue:N2}").FontSize(9).AlignRight();
                                table.Cell().Text($"{product.PercentOfSales:F2}%").FontSize(9).AlignRight();
                                table.Cell().Text(product.PerformanceRating).FontSize(9).AlignCenter()
                                    .FontColor(product.PerformanceRating == "Best Seller" ? Colors.Green.Medium :
                                              product.PerformanceRating == "Good Seller" ? Colors.Blue.Medium : Colors.Orange.Medium);
                            }
                        });

                        // Summary Footer
                        col.Item().PaddingTop(20).AlignCenter().Text($"Report generated on {DateTime.Now:MMMM dd, yyyy HH:mm:ss}")
                            .FontSize(8).FontColor(Colors.Grey.Medium);
                    });

                    // Footer
                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                        });
                });
            });

            return document.GeneratePdf();
        }

        public byte[] GenerateInventoryReportPDF(
            List<InventoryItemReport> inventoryItems,
            int totalIngredients,
            int lowStockCount,
            int outOfStockCount)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    // Header
                    page.Header()
                        .ShowOnce()
                        .Column(col =>
                        {
                            col.Item().Text("TEN DEGREES POS")
                                .FontSize(20)
                                .Bold()
                                .FontColor(Colors.Grey.Darken4)
                                .AlignCenter();

                            col.Item().Text("Inventory Status Report")
                                .FontSize(16)
                                .Bold()
                                .AlignCenter()
                                .FontColor(Colors.Blue.Medium);

                            col.Item().Text($"Generated on {DateTime.Now:MMMM dd, yyyy HH:mm:ss}")
                                .FontSize(10)
                                .AlignCenter()
                                .FontColor(Colors.Grey.Darken2);

                            col.Item().PaddingTop(10).LineHorizontal(1);
                        });

                    // Content
                    page.Content().Column(col =>
                    {
                        // Summary Cards Section
                        col.Item().PaddingTop(10).Row(row =>
                        {
                            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Column(summary =>
                            {
                                summary.Item().Text("Total Ingredients").FontSize(9).FontColor(Colors.Grey.Darken2);
                                summary.Item().Text($"{totalIngredients:N0}").FontSize(14).Bold().FontColor(Colors.Blue.Medium);
                            });

                            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Column(summary =>
                            {
                                summary.Item().Text("Low Stock").FontSize(9).FontColor(Colors.Grey.Darken2);
                                summary.Item().Text($"{lowStockCount:N0}").FontSize(14).Bold().FontColor(Colors.Orange.Medium);
                            });

                            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Column(summary =>
                            {
                                summary.Item().Text("Out of Stock").FontSize(9).FontColor(Colors.Grey.Darken2);
                                summary.Item().Text($"{outOfStockCount:N0}").FontSize(14).Bold().FontColor(Colors.Red.Medium);
                            });

                            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Column(summary =>
                            {
                                decimal inStockPercentage = totalIngredients > 0 ? ((decimal)(totalIngredients - outOfStockCount) / totalIngredients) * 100 : 0;
                                summary.Item().Text("In Stock %").FontSize(9).FontColor(Colors.Grey.Darken2);
                                summary.Item().Text($"{inStockPercentage:F1}%").FontSize(14).Bold().FontColor(Colors.Green.Medium);
                            });
                        });

                        // Inventory Details Table
                        col.Item().PaddingTop(15).Text("Current Inventory Status").FontSize(12).Bold().FontColor(Colors.Grey.Darken3);
                        col.Item().PaddingTop(5).Border(1).BorderColor(Colors.Grey.Lighten2).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Ingredient").Bold().FontSize(9);
                                header.Cell().Text("Current Stock").Bold().FontSize(9).AlignRight();
                                header.Cell().Text("Unit").Bold().FontSize(9).AlignCenter();
                                header.Cell().Text("Min Threshold").Bold().FontSize(9).AlignRight();
                                header.Cell().Text("Status").Bold().FontSize(9).AlignCenter();
                            });

                            foreach (var item in inventoryItems)
                            {
                                table.Cell().Text(item.IngredientName).FontSize(9);
                                table.Cell().Text($"{item.CurrentStock:F1}").FontSize(9).AlignRight();
                                table.Cell().Text(item.Unit).FontSize(9).AlignCenter();
                                table.Cell().Text($"{item.MinimumStock}").FontSize(9).AlignRight();
                                table.Cell().Text(item.Status).FontSize(9).AlignCenter()
                                    .FontColor(item.Status == "In Stock" ? Colors.Green.Medium :
                                              item.Status == "Low Stock" ? Colors.Orange.Medium : Colors.Red.Medium);
                            }
                        });

                        // Footer summary
                        col.Item().PaddingTop(15).BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(5).Text(text =>
                        {
                            text.Span("Notes: ");
                            text.Span("Low Stock = Below minimum threshold. Out of Stock = No inventory available.")
                                .FontSize(8).FontColor(Colors.Grey.Medium);
                        });
                    });

                    // Footer
                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                        });
                });
            });

            return document.GeneratePdf();
        }
    }

    // Helper Classes for PDF Export
    public class SalesProductPerformance
    {
        public string ProductName { get; set; } = string.Empty;
        public int UnitsSold { get; set; }
        public decimal Revenue { get; set; }
        public decimal PercentOfSales { get; set; }
        public string PerformanceRating { get; set; } = string.Empty;
    }

    public class SalesChartDataPoint
    {
        public DateTime Date { get; set; }
        public decimal Sales { get; set; }
    }

    public class InventoryItemReport
    {
        public string IngredientName { get; set; } = string.Empty;
        public decimal CurrentStock { get; set; }
        public string Unit { get; set; } = string.Empty;
        public int MinimumStock { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}