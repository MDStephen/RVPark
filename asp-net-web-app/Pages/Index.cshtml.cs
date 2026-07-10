using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using asp_net_web_app.Data;

namespace asp_net_web_app.Pages;

public class IndexModel : PageModel
{
    private readonly DatabaseWrapper _db;

    public IndexModel(DatabaseWrapper db)
    {
        _db = db;
    }
}
