using asp_net_web_app.Data;
using asp_net_web_app.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace asp_net_web_app.Pages;

public class IndexModel : PageModel
{
    private readonly DatabaseWrapper _db;

    public IndexModel(DatabaseWrapper db, SiteAvailabilityService availability)
    {
        _db = db;
    }

    public int TotalSites { get; set; }
    public int AvailableSitesToday { get; set; }
    public Pricing? Pricing { get; set; }
    public List<DbSite> FeaturedSites { get; set; } = [];

    public async Task OnGetAsync()
    {
        Pricing = await _db.Pricing
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }
}
