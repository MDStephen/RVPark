using System.Text;
using System.Text.Json;
using asp_net_web_app.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace asp_net_web_app.Pages;

public class AdminReportGeneratorModel : PageModel
{
    private readonly DatabaseWrapper _db;

    private static readonly Dictionary<string, Func<DatabaseWrapper, IQueryable<object>>> TableQueries = new()
    {
        ["Reservations"] = db => db.Reservations.Select(r => (object)r),
        ["Sites"] = db => db.Sites.Select(s => (object)s),
        ["Users"] = db => db.Users.Select(u => (object)u),
        ["Employees"] = db => db.Employees.Select(e => (object)e),
        ["Payments"] = db => db.Payments.Select(p => (object)p),
        ["SitePhotos"] = db => db.SitePhotos.Select(p => (object)p),
        ["SitePrices"] = db => db.SitePrices.Select(p => (object)p),
        ["Pricing"] = db => db.Pricing.Select(p => (object)p),
    };

    public AdminReportGeneratorModel(DatabaseWrapper db)
    {
        _db = db;
    }

    public List<string> AvailableTables { get; } = TableQueries.Keys.OrderBy(k => k).ToList();

    [BindProperty]
    public string SelectedTable { get; set; } = "Reservations";

    [BindProperty]
    public string ReportType { get; set; } = "PDF";

    public List<object> PreviewRows { get; private set; } = [];
    public List<string> ColumnNames { get; private set; } = [];

    public void OnGet()
    {
        LoadPreview();
    }

    public IActionResult OnPostGenerateReport()
    {
        if (!TableQueries.ContainsKey(SelectedTable))
        {
            SelectedTable = "Reservations";
        }

        var rows = TableQueries[SelectedTable](_db).Take(500).ToList();

        return ReportType switch
        {
            "CSV" => File(Encoding.UTF8.GetBytes(ToCsv(rows)), "text/csv", $"{SelectedTable}.csv"),
            "JSON" => File(
                Encoding.UTF8.GetBytes(JsonSerializer.Serialize(rows, new JsonSerializerOptions { WriteIndented = true })),
                "application/json",
                $"{SelectedTable}.json"),
            _ => File(GeneratePdf(rows, SelectedTable), "application/pdf", $"{SelectedTable}.pdf")
        };
    }

    private void LoadPreview()
    {
        if (!TableQueries.ContainsKey(SelectedTable))
            SelectedTable = "Reservations";

        PreviewRows = TableQueries[SelectedTable](_db).Take(25).ToList();
        ColumnNames = PreviewRows.FirstOrDefault()?.GetType().GetProperties()
            .Select(p => p.Name).ToList() ?? [];
    }

    private static byte[] GeneratePdf(List<object> data, string title)
    {
        var props = data.FirstOrDefault()?.GetType().GetProperties();

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(20);
                page.Header().Text($"{title} Report").FontSize(20).Bold();
                page.Content().Column(col =>
                {
                    if (props == null || data.Count == 0)
                    {
                        col.Item().Text("No data available for this report.").FontSize(12);
                        return;
                    }

                    col.Item().Table(table =>
                    {
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
            });
        }).GeneratePdf();
    }

    private static string ToCsv(List<object> data)
    {
        var props = data.FirstOrDefault()?.GetType().GetProperties();
        if (props == null || props.Length == 0)
            return "No data";

        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", props.Select(p => EscapeCsv(p.Name))));

        foreach (var row in data)
        {
            sb.AppendLine(string.Join(",", props.Select(p => EscapeCsv(p.GetValue(row)?.ToString() ?? ""))));
        }

        return sb.ToString();
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
