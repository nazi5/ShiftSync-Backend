using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ShiftSync.Api.Models;

namespace ShiftSync.Api.Services
{
    public class ShiftReportGenerator
    {
        public static byte[] GeneratePdf(Shift shift)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Shift Handover Report").FontSize(24).SemiBold().FontColor(Colors.Blue.Darken2);
                            col.Item().Text($"Shift: {shift.Name}").FontSize(14);
                            col.Item().Text($"Generated: {DateTime.UtcNow:g}").FontSize(10).FontColor(Colors.Grey.Medium);
                        });
                    });

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                    {
                        column.Spacing(20);

                        column.Item().Text("Handover Summary").SemiBold().FontSize(16);

                        // Shift Details
                        column.Item().Text($"Status: {shift.Status}");
                        column.Item().Text($"Claimed At: {shift.ClaimedAt?.ToString("g") ?? "N/A"}");
                        column.Item().Text($"Closed At: {shift.ClosedAt?.ToString("g") ?? "N/A"}");
                        column.Item().Text($"Total Logs Recorded: {shift.ShiftLogs.Count}");

                        column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        // The Logs Table
                        column.Item().Table(table =>
                        {
                            // Define columns
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(120); // Time
                                columns.ConstantColumn(100); // Type
                                columns.RelativeColumn();    // Description
                            });

                            // Table Header
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Time");
                                header.Cell().Element(CellStyle).Text("Log Type");
                                header.Cell().Element(CellStyle).Text("Description");

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                                }
                            });

                            // Table Rows
                            if (!shift.ShiftLogs.Any())
                            {
                                table.Cell().ColumnSpan(3).PaddingVertical(10).Text("No logs recorded for this shift.").Italic().FontColor(Colors.Grey.Medium);
                            }
                            else
                            {
                                foreach (var log in shift.ShiftLogs.OrderBy(l => l.TimeStamp))
                                {
                                    table.Cell().PaddingVertical(5).Text(log.TimeStamp.ToString("g"));
                                    table.Cell().PaddingVertical(5).Text(log.LogType.ToString());
                                    table.Cell().PaddingVertical(5).Text(log.Description);
                                }
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
                });
            });
            return document.GeneratePdf();
        }
    }
}
