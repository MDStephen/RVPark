using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using asp_net_web_app.Data;

namespace asp_net_web_app.Pages;

public class AdminReportGeneratorModel : PageModel
{
    private readonly DatabaseWrapper _db;

    public AdminReportGeneratorModel(DatabaseWrapper db)
    {
        _db = db;
    }

}
