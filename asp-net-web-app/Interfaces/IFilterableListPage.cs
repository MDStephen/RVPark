namespace asp_net_web_app.Models;

public interface IFilterableListPage
{
    List<FilterModel.FilterDefinition> Filters { get; }
    List<object> Rows { get; }

    string IdPropertyName { get; } // override if the id is called anything other than Id

    string? SelectedId { get; set; }
}
