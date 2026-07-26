using asp_net_web_app.Data;
using asp_net_web_app.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace asp_net_web_app.Pages;

public class IndexModel : PageModel
{
    private readonly DatabaseWrapper _db;
    private readonly SiteAvailabilityService _availability;

    public IndexModel(DatabaseWrapper db, SiteAvailabilityService availability)
    {
        _db = db;
        _availability = availability;
    }

    public int TotalSites { get; set; }
    public int AvailableSitesToday { get; set; }
    public List<DbSite> FeaturedSites { get; set; } = [];

    public async Task OnGetAsync()
    {
        TotalSites = await _db.Sites.CountAsync();
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        var available = await _availability.SearchSitesAsync(today, tomorrow, null);
        AvailableSitesToday = available.Count(r => r.FullyAvailable);

        FeaturedSites = await _db.Sites
            .AsNoTracking()
            .Where(s => s.IsAvailable)
            .OrderBy(s => s.SiteNumber)
            .Take(3)
            .ToListAsync();
    }
}
