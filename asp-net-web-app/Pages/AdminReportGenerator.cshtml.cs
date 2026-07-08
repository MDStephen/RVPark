using asp_net_web_app.Data;
using asp_net_web_app.Models;

namespace asp_net_web_app.Pages;

public class AdminReportGeneratorModel : FilterableListPageModel<Reservations> // TODO swap out with input from a dropdown list of available tables
{
    public AdminReportGeneratorModel(DatabaseWrapper db) : base(db) { }

    protected override IEnumerable<Reservations> GetTable() => Db.Set<Reservations>();
}
