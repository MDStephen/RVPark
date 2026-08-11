using System.Text;
using System.Text.Json;
using asp_net_web_app.Data;
using asp_net_web_app.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;

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

    // IFilterableListPage
    public List<FilterModel.FilterDefinition> Filters { get; private set; } = [];
    public List<object> Rows { get; private set; } = []; // preview only (capped below)
    public string IdPropertyName => "Id";
    public string? SelectedId { get; set; } // not used for reports; required by the interface

    // Full filtered row count, for an honest "Generate Report (N rows)" label -
    // Rows above is capped to 25 for the preview table.
    public int FilteredCount { get; private set; }

    // trends (screen-only - not part of the PDF/CSV/JSON export)
    public List<MonthlyTrendPoint> Trends { get; private set; } = [];
    public string? TrendDateField { get; private set; }
    public string? TrendAmountField { get; private set; }

    // Properties whose *name* matches this are treated as sensitive and are:
    //   1. excluded from generated Filters (no "Search"/"Dropdown" filter is built for them)
    //   2. excluded from the PDF's column list entirely (no header, no cells)
    //   3. nulled out on every row object immediately after load, via RedactSensitiveProperties,
    //      so any other reader of Rows (e.g. the page's preview table) never sees the raw value.
    // Applied generically across all tables (not just Users) so any future password-like
    // column is covered automatically. Matches "Password", "PasswordHash", "HashedPassword", etc.
    private static bool IsExcludedProperty(string name) =>
        name.Contains("password", StringComparison.OrdinalIgnoreCase);

    // Nulls out sensitive property values in-place on the already-materialized row objects.
    // These rows come from .ToList() in OnGet/OnPostGenerateReport, so this does not touch
    // the database - nothing is persisted.
    private static void RedactSensitiveProperties(List<object> rows)
    {
        if (rows.Count == 0) return;

        var sensitiveProps = rows[0].GetType().GetProperties()
            .Where(p => IsExcludedProperty(p.Name) && p.CanWrite)
            .ToList();

        if (sensitiveProps.Count == 0) return;

        foreach (var row in rows)
        {
            foreach (var prop in sensitiveProps)
            {
                prop.SetValue(row, null);
            }
        }
    }

    private static string CreateTrendChartSvg(
        List<MonthlyTrendPoint> trends,
        string? amountField)
    {
        const int width = 760;
        const int height = 220;

        const int left = 60;
        const int right = 30;
        const int top = 25;
        const int bottom = 55;

        var chartWidth = width - left - right;
        var chartHeight = height - top - bottom;

        var maxCount = Math.Max(1, trends.Max(x => x.Count));

        var maxAmount = amountField != null
            ? Math.Max(1m, trends.Max(x => x.Total ?? 0m))
            : 1m;

        var points = new List<(double X, double Y)>();

        for (var i = 0; i < trends.Count; i++)
        {
            var x = trends.Count == 1
                ? left + chartWidth / 2.0
                : left + i * (chartWidth / (double)(trends.Count - 1));

            var y = top +
                    chartHeight -
                    (trends[i].Count / (double)maxCount * chartHeight);

            points.Add((x, y));
        }

        var svg = new StringBuilder();

        svg.Append($"""
            <svg xmlns="http://www.w3.org/2000/svg"
                width="{width}"
                height="{height}"
                viewBox="0 0 {width} {height}">

                <rect width="100%" height="100%" fill="white"/>

                <line x1="{left}" y1="{top}"
                    x2="{left}" y2="{top + chartHeight}"
                    stroke="#777" stroke-width="1"/>

                <line x1="{left}" y1="{top + chartHeight}"
                    x2="{left + chartWidth}" y2="{top + chartHeight}"
                    stroke="#777" stroke-width="1"/>
            """);

        // Horizontal grid lines
        for (var i = 0; i <= 4; i++)
        {
            var y = top + chartHeight - (i / 4.0 * chartHeight);
            var value = maxCount * i / 4.0;

            svg.Append($"""
                <line x1="{left}" y1="{y:F1}"
                    x2="{left + chartWidth}" y2="{y:F1}"
                    stroke="#e5e5e5" stroke-width="1"/>

                <text x="{left - 8}" y="{y + 4:F1}"
                    text-anchor="end"
                    font-size="11"
                    fill="#555">{value:F0}</text>
                """);
        }

        // Count bars
        for (var i = 0; i < trends.Count; i++)
        {
            var x = points[i].X;

            var barWidth = Math.Min(
                40,
                chartWidth / Math.Max(1, trends.Count) * 0.65);

            var barHeight =
                trends[i].Count / (double)maxCount * chartHeight;

            var y = top + chartHeight - barHeight;

            svg.Append($"""
                <rect x="{x - barWidth / 2:F1}"
                    y="{y:F1}"
                    width="{barWidth:F1}"
                    height="{barHeight:F1}"
                    fill="#0d6efd"
                    opacity="0.7"/>
                """);
        }

        // Optional amount line
        if (amountField != null && trends.Any(t => t.Total.HasValue))
        {
            var amountPoints = new List<string>();

            for (var i = 0; i < trends.Count; i++)
            {
                var x = points[i].X;
                var amount = (double)(trends[i].Total ?? 0m);

                var y = top +
                        chartHeight -
                        (amount / (double)maxAmount * chartHeight);

                amountPoints.Add($"{x:F1},{y:F1}");
            }

            svg.Append($"""
                <polyline
                    points="{string.Join(" ", amountPoints)}"
                    fill="none"
                    stroke="#198754"
                    stroke-width="3"/>
                """);

            foreach (var point in amountPoints)
            {
                var parts = point.Split(',');
                svg.Append($"""
                    <circle cx="{parts[0]}"
                            cy="{parts[1]}"
                            r="4"
                            fill="#198754"/>
                    """);
            }
        }

        // Month labels
        for (var i = 0; i < trends.Count; i++)
        {
            var x = points[i].X;

            svg.Append($"""
                <text x="{x:F1}"
                    y="{height - 18}"
                    text-anchor="middle"
                    font-size="10"
                    fill="#555">
                    {System.Security.SecurityElement.Escape(trends[i].Month)}
                </text>
                """);
        }

        svg.Append("</svg>");

        return svg.ToString();
    }

    public void OnGet()
    {
        if (!TableQueries.ContainsKey(SelectedTable))
            SelectedTable = "Reservations";

        var allRows = TableQueries[SelectedTable](_db).Take(MaxRows).ToList();
        RedactSensitiveProperties(allRows);
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

        var allRows = TableQueries[SelectedTable](_db)
            .Take(MaxRows)
            .ToList();
        RedactSensitiveProperties(allRows);

        var filters = BuildFilters(allRows);
        var rows = ApplyFilters(allRows, filters);

        var (dateField, amountField) = rows.Count > 0
            ? FindTrendFields(rows[0].GetType())
            : (null, null);

        var trends = dateField != null
            ? BuildTrends(rows, dateField, amountField)
            : [];

        return ReportType switch
        {
            "CSV" => File(
                Encoding.UTF8.GetBytes(ToCsv(rows)),
                "text/csv",
                $"{SelectedTable}.csv"),

            "JSON" => File(
                Encoding.UTF8.GetBytes(
                    JsonSerializer.Serialize(
                        rows,
                        new JsonSerializerOptions { WriteIndented = true })),
                "application/json",
                $"{SelectedTable}.json"),

            _ => File(
                GeneratePdf(
                    rows,
                    SelectedTable,
                    trends,
                    dateField,
                    amountField),
                "application/pdf",
                $"{SelectedTable}.pdf")
        };
    }

    // Filtering
    // Mirrors FilterableListPageModel<T>'s logic, but the table type is picked at runtime
    // (SelectedTable), so it can't be a compile-time generic T - we key off the row's actual type instead.

    private static List<FilterModel.FilterDefinition> BuildFilters(List<object> rows)
    {
        if (rows.Count == 0) return [];

        var filters = new List<FilterModel.FilterDefinition>();

        foreach (var prop in rows[0].GetType().GetProperties())
        {
            if (IsExcludedProperty(prop.Name)) continue;

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

    // --- Output formats ---

    private static byte[] GeneratePdf(
        List<object> data,
        string title,
        List<MonthlyTrendPoint> trends,
        string? trendDateField,
        string? trendAmountField)
    {
        var props = data.FirstOrDefault()?.GetType().GetProperties()
            .Where(p => !IsExcludedProperty(p.Name))
            .ToArray();

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);

                page.Header()
                    .Column(header =>
                    {
                        header.Item()
                            .Text($"{title} Report")
                            .FontSize(22)
                            .Bold();

                        header.Item()
                            .Text($"Generated {DateTime.Now:g}")
                            .FontSize(9)
                            .FontColor(Colors.Grey.Medium);
                    });

                page.Content()
                    .Column(col =>
                    {
                        col.Spacing(15);

                        if (props == null || data.Count == 0)
                        {
                            col.Item()
                                .Text("No data available for this report.")
                                .FontSize(12);

                            return;
                        }

                        // Trend graph
                        if (trends.Count > 0 && trendDateField != null)
                        {
                            col.Item()
                                .Text($"{title} Trends")
                                .FontSize(15)
                                .Bold();

                            col.Item()
                                .Text(
                                    $"{trendDateField} by month" +
                                    (trendAmountField != null
                                        ? $", count & {trendAmountField}"
                                        : ", count"))
                                .FontSize(9)
                                .FontColor(Colors.Grey.Medium);

                            col.Item()
                                .Svg(CreateTrendChartSvg(
                                    trends,
                                    trendAmountField));
                        }

                        // Data table
                        col.Item()
                            .Text($"Data {title}")
                            .FontSize(15)
                            .Bold();

                        col.Item()
                            .Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    foreach (var prop in props)
                                    {
                                        columns.RelativeColumn();
                                    }
                                });

                                table.Header(header =>
                                {
                                    foreach (var prop in props)
                                    {
                                        header.Cell()
                                            .Element(HeaderCellStyle)
                                            .Text(prop.Name);
                                    }
                                });

                                for (var rowIndex = 0; rowIndex < data.Count; rowIndex++)
                                {
                                    var row = data[rowIndex];

                                    foreach (var prop in props)
                                    {
                                        var value = prop.GetValue(row);

                                        table.Cell()
                                            .Element(container =>
                                                BodyCellStyle(container, rowIndex))
                                            .Text(FormatReportValue(value))
                                            .FontSize(7);
                                    }
                                }
                            });
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.Span($"{title} Report  •  ");
                        text.CurrentPageNumber();
                        text.Span(" / ");
                        text.TotalPages();
                    });
            });
        }).GeneratePdf();

        static IContainer HeaderCellStyle(IContainer container)
        {
            return container
                .Background(Colors.Blue.Darken2)
                .DefaultTextStyle(x =>
                    x.FontColor(Colors.White)
                    .Bold()
                    .FontSize(9))
                .PaddingVertical(7)
                .PaddingHorizontal(5);
        }

        static IContainer BodyCellStyle(IContainer container, int rowIndex)
        {
            return container
                .Background(
                    rowIndex % 2 == 0
                        ? Colors.Grey.Lighten5
                        : Colors.White)
                .DefaultTextStyle(x => x.FontSize(8))
                .PaddingVertical(5)
                .PaddingHorizontal(5)
                .BorderBottom(0.5f)
                .BorderColor(Colors.Grey.Lighten2);
        }

        static string FormatReportValue(object? value)
        {
            if (value == null)
                return "";

            return value switch
            {
                DateTime date => date.ToString("yyyy-MM-dd HH:mm"),
                decimal money => money.ToString("C"),
                double number => number.ToString("N2"),
                float number => number.ToString("N2"),
                _ => value.ToString() ?? ""
            };
        }
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
