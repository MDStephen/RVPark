using asp_net_web_app.Data;
using asp_net_web_app.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace asp_net_web_app.Pages;

public class AdminReportGeneratorModel : FilterableListPageModel<Reservations>
{
    public AdminReportGeneratorModel(DatabaseWrapper db) : base(db) { }

    protected override IEnumerable<Reservations> GetTable() => Db.Set<Reservations>();

    public List<string> AvailableTables { get; private set; } = [];

    [BindProperty]
    public string? SelectedTable { get; set; }

    [BindProperty]
    public string ReportType { get; set; } = "PDF";

    protected override void BeforeOnGet()
    {
        AvailableTables = Db.Model
            .GetEntityTypes()
            .Select(t => t.ClrType.Name)
            .OrderBy(n => n)
            .ToList();

        SelectedTable ??= AvailableTables.FirstOrDefault();
    }

    public IActionResult OnPostGenerateReport() // I used chatgpt to help me get this working properly with QuestPDF since I've never used it before
    {
        // to get around filters not being set on time of report generation
        var table = GetTable().ToList();
        Filters = BuildFilters(table);
        Rows = ApplyFilters(table, Filters).Cast<object>().ToList();

        var data = Rows;

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(20);
                page.Header()
                    .Text($"{SelectedTable} Report")
                    .FontSize(20)
                    .Bold();
                page.Content().Table(table =>
                {
                    var props = data.FirstOrDefault()?.GetType().GetProperties();
                    if (props == null)
                        return;

                    table.ColumnsDefinition(columns =>
                    {
                        foreach (var _ in props)
                            columns.RelativeColumn();
                    });
                    table.Header(header =>
                    {
                        foreach (var prop in props)
                            header.Cell().Text(prop.Name).Bold();
                    });
                    foreach (var row in data)
                    {
                        foreach (var prop in props)
                        {
                            table.Cell().Text(prop.GetValue(row)?.ToString() ?? "");
                        }
                    }
                });
            });
        }).GeneratePdf();
        return File(pdf, "application/pdf", $"{SelectedTable}.pdf");
    }
}
