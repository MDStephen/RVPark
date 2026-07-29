using System.Text;
using System.Text.Json;
using asp_net_web_app.Data;
using asp_net_web_app.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace asp_net_web_app.Pages;

public class AdminReportGeneratorModel : PageModel, IFilterableListPage
{
    // Safety cap on how many rows we ever pull into memory for a table. Filtering/export
    // happens against this set, not the raw DB table, so a huge table can't take the page down.
    private const int MaxRows = 5000;

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

    public AdminReportGeneratorModel(DatabaseWrapper db) => _db = db;

    public List<string> AvailableTables { get; } = TableQueries.Keys.OrderBy(k => k).ToList();

    [BindProperty(SupportsGet = true)]
    public string SelectedTable { get; set; } = "Reservations";

    [BindProperty]
    public string ReportType { get; set; } = "PDF";

    // --- IFilterableListPage ---
    public List<FilterModel.FilterDefinition> Filters { get; private set; } = [];
    public List<object> Rows { get; private set; } = []; // preview only (capped below)
    public string IdPropertyName => "Id";
    public string? SelectedId { get; set; } // not used for reports; required by the interface

    // Full filtered row count, for an honest "Generate Report (N rows)" label -
    // Rows above is capped to 25 for the preview table.
    public int FilteredCount { get; private set; }

    // --- Monthly trends (screen-only - not part of the PDF/CSV/JSON export) ---
    public List<MonthlyTrendPoint> Trends { get; private set; } = [];
    public string? TrendDateField { get; private set; }
    public string? TrendAmountField { get; private set; }

    public void OnGet()
    {
        if (!TableQueries.ContainsKey(SelectedTable))
            SelectedTable = "Reservations";

        var allRows = TableQueries[SelectedTable](_db).Take(MaxRows).ToList();
        Filters = BuildFilters(allRows);

        var filtered = ApplyFilters(allRows, Filters);
        FilteredCount = filtered.Count;
        Rows = filtered.Take(25).ToList();

        if (allRows.Count > 0)
        {
            var (dateField, amountField) = FindTrendFields(allRows[0].GetType());
            TrendDateField = dateField;
            TrendAmountField = amountField;
            if (dateField != null)
                Trends = BuildTrends(filtered, dateField, amountField); // trends reflect the active filters
        }
    }

    public IActionResult OnPostGenerateReport()
    {
        if (!TableQueries.ContainsKey(SelectedTable))
            SelectedTable = "Reservations";

        var allRows = TableQueries[SelectedTable](_db).Take(MaxRows).ToList();
        var filters = BuildFilters(allRows);
        var rows = ApplyFilters(allRows, filters);

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

    // --- Filtering ---
    // Mirrors FilterableListPageModel<T>'s logic, but the table type is picked at runtime
    // (SelectedTable), so it can't be a compile-time generic T - we key off the row's actual type instead.

    private static List<FilterModel.FilterDefinition> BuildFilters(List<object> rows)
    {
        if (rows.Count == 0) return [];

        var filters = new List<FilterModel.FilterDefinition>();

        foreach (var prop in rows[0].GetType().GetProperties())
        {
            if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
            {
                filters.Add(new FilterModel.FilterDefinition("StartDate", prop.Name, rows));
                filters.Add(new FilterModel.FilterDefinition("EndDate", prop.Name, rows));
            }
            else if (prop.PropertyType == typeof(string))
            {
                var distinctCount = rows.Select(r => prop.GetValue(r)?.ToString()).Distinct().Count();

                filters.Add(distinctCount <= 15
                    ? new FilterModel.FilterDefinition("Dropdown", prop.Name, rows)
                    : new FilterModel.FilterDefinition("Search", prop.Name, rows));
            }
        }

        return filters;
    }

    private List<object> ApplyFilters(List<object> rows, List<FilterModel.FilterDefinition> filters)
    {
        if (rows.Count == 0) return rows;

        var type = rows[0].GetType();
        var query = rows.AsEnumerable();

        foreach (var filter in filters)
        {
            var paramName = filter.Type is "StartDate" or "EndDate" ? $"{filter.Name}_{filter.Type}" : filter.Name;
            var raw = GetParam(paramName);
            if (string.IsNullOrWhiteSpace(raw)) continue;

            var prop = type.GetProperty(filter.Name)!;

            query = filter.Type switch
            {
                "Dropdown" => query.Where(r => prop.GetValue(r)?.ToString() == raw),
                "Search" => query.Where(r => (prop.GetValue(r)?.ToString() ?? "")
                                    .Contains(raw, StringComparison.OrdinalIgnoreCase)),
                "StartDate" => DateTime.TryParse(raw, out var start)
                                    ? query.Where(r => (DateTime?)prop.GetValue(r) >= start) : query,
                "EndDate" => DateTime.TryParse(raw, out var end)
                                    ? query.Where(r => (DateTime?)prop.GetValue(r) <= end) : query,
                _ => query
            };
        }

        return query.ToList();
    }

    // Filters are submitted via GET (query string) on preview, but forwarded as hidden form
    // fields on the POST that generates the report - this reads whichever is present, so
    // the exact same filtering logic covers both without duplicating it.
    private string? GetParam(string key) =>
        Request.HasFormContentType ? Request.Form[key].ToString() : Request.Query[key].ToString();

    // monthly trends graph
    // Picks the first DateTime column to bucket by month, and (if present) the first
    // numeric column that looks money-shaped, to sum alongside the row count.
    // Tables with no date column (Sites, Pricing, ...) just get no trend chart.

    private static (string? DateField, string? AmountField) FindTrendFields(Type type)
    {
        var props = type.GetProperties();

        var dateField = props
            .FirstOrDefault(p => p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?))
            ?.Name;

        var moneyNames = new[] { "Cost", "Amount", "Price", "Total" };
        var amountField = props
            .FirstOrDefault(p =>
                (p.PropertyType == typeof(decimal) || p.PropertyType == typeof(decimal?) ||
                 p.PropertyType == typeof(double) || p.PropertyType == typeof(double?) ||
                 p.PropertyType == typeof(int) || p.PropertyType == typeof(int?)) &&
                moneyNames.Any(n => p.Name.Contains(n, StringComparison.OrdinalIgnoreCase)))
            ?.Name;

        return (dateField, amountField);
    }

    private static List<MonthlyTrendPoint> BuildTrends(List<object> rows, string dateField, string? amountField)
    {
        if (rows.Count == 0) return [];

        var type = rows[0].GetType();
        var dateProp = type.GetProperty(dateField)!;
        var amountProp = amountField != null ? type.GetProperty(amountField) : null;

        return rows
            .Select(r => new { Date = (DateTime?)dateProp.GetValue(r), Row = r })
            .Where(x => x.Date.HasValue)
            .GroupBy(x => new DateTime(x.Date!.Value.Year, x.Date.Value.Month, 1))
            .OrderBy(g => g.Key)
            .Select(g => new MonthlyTrendPoint(
                g.Key.ToString("MMM yyyy"),
                g.Count(),
                amountProp != null
                    ? g.Sum(x => Convert.ToDecimal(amountProp.GetValue(x.Row) ?? 0m))
                    : null))
            .ToList();
    }

    // --- Output formats (unchanged) ---

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

public record MonthlyTrendPoint(string Month, int Count, decimal? Total);
