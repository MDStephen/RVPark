using System.Linq;

namespace asp_net_web_app.Models;

public class FilterModel
{
    public class FilterDefinition
    {
        private static readonly string[] AcceptableTypes = ["Dropdown", "StartDate", "EndDate", "Search"];

        public string Type { get; }
        public string Name { get; }
        public List<string> Data { get; }

        public FilterDefinition(string type, string propertyName, IEnumerable<object> rows)
        {
            if (!AcceptableTypes.Contains(type))
                throw new ArgumentException($"'{type}' is not a supported filter type.");

            Type = type;
            Name = propertyName;

            Data = Type == "Dropdown"
                ? BuildDropdownOptions(propertyName, rows)
                : [];
        }

        private static List<string> BuildDropdownOptions(string propertyName, IEnumerable<object> rows)
        {
            var options = new List<string>();

            foreach (var row in rows)
            {
                var prop = row.GetType().GetProperty(propertyName);
                var value = prop?.GetValue(row)?.ToString();

                if (value is not null && !options.Contains(value))
                    options.Add(value);
            }

            options.Sort();
            return options;
        }
    }
}
