using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using asp_net_web_app.Data;

namespace asp_net_web_app.Models;

public abstract class FilterableListPageModel<T> : PageModel, IFilterableListPage where T : class
{
    protected readonly DatabaseWrapper Db;

    public List<FilterModel.FilterDefinition> Filters { get; protected set; } = [];
    public List<object> Rows { get; protected set; } = [];

    [BindProperty]
    public string? SelectedId { get; set; } // sent back to the page via post, for selction

    public virtual string IdPropertyName => "Id";

    protected FilterableListPageModel(DatabaseWrapper db)
    {
        Db = db;
    }

    protected abstract IEnumerable<T> GetTable();

    public void OnGet()
    {
        BeforeOnGet();

        var table = GetTable().ToList();
        Filters = BuildFilters(table);
        Rows = ApplyFilters(table, Filters).Cast<object>().ToList();
    }

    protected virtual void BeforeOnGet(){}

    protected T? GetSelectedRow() // call from onpost to get the row the user selected
    {
        if (string.IsNullOrEmpty(SelectedId))
            return null;

        var idProp = typeof(T).GetProperty(IdPropertyName)
            ?? throw new InvalidOperationException(
                $"{typeof(T).Name} has no property named '{IdPropertyName}'. " +
                $"Override IdPropertyName if your key field is named differently.");

        return GetTable().FirstOrDefault(r => idProp.GetValue(r)?.ToString() == SelectedId);
    }

    protected static List<FilterModel.FilterDefinition> BuildFilters(List<T> rows)
    {
        var filters = new List<FilterModel.FilterDefinition>();
        var rowObjects = rows.Cast<object>().ToList();

        foreach (var prop in typeof(T).GetProperties())
        {
            if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
            {
                filters.Add(new FilterModel.FilterDefinition("StartDate", prop.Name, rowObjects));
                filters.Add(new FilterModel.FilterDefinition("EndDate", prop.Name, rowObjects));
            }
            else if (prop.PropertyType == typeof(string))
            {
                var distinctCount = rowObjects
                    .Select(r => prop.GetValue(r)?.ToString())
                    .Distinct()
                    .Count();

                filters.Add(distinctCount <= 15
                    ? new FilterModel.FilterDefinition("Dropdown", prop.Name, rowObjects)
                    : new FilterModel.FilterDefinition("Search", prop.Name, rowObjects));
            }
        }

        return filters;
    }

    protected List<T> ApplyFilters(List<T> rows, List<FilterModel.FilterDefinition> filters)
    {
        var query = rows.AsEnumerable();

        foreach (var filter in filters)
        {
            var queryKey = filter.Type is "StartDate" or "EndDate"
                ? $"{filter.Name}_{filter.Type}"
                : filter.Name;

            if (!Request.Query.TryGetValue(queryKey, out var raw) || string.IsNullOrWhiteSpace(raw))
                continue;

            var prop = typeof(T).GetProperty(filter.Name)!;
            var rawValue = raw.ToString();

            query = filter.Type switch
            {
                "Dropdown" => query.Where(r => prop.GetValue(r)?.ToString() == rawValue),
                "Search" => query.Where(r => (prop.GetValue(r)?.ToString() ?? "")
                                    .Contains(rawValue, StringComparison.OrdinalIgnoreCase)),
                "StartDate" => query.Where(r => (DateTime?)prop.GetValue(r) >= DateTime.Parse(rawValue)),
                "EndDate" => query.Where(r => (DateTime?)prop.GetValue(r) <= DateTime.Parse(rawValue)),
                _ => query
            };
        }

        return query.ToList();
    }
}
