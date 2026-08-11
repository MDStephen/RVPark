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
    public List<DbSite> FeaturedSites { get; set; } = [];

    // Loaded once in OnGetAsync (Pricing is a single-row table) - the rate
    // properties below all derive from this instead of the old PricingStore.
    private Pricing? _pricing;

    public decimal nightlyRate =>
        _pricing?.baseNightlyRate ?? 0m;

    public decimal nightlyRateDuringHighSeason =>
        (_pricing?.baseNightlyRate ?? 0m) * (_pricing?.seasonMultiplier ?? 1m);

    public decimal nightlyRatePlusUtilities =>
        (_pricing?.baseNightlyRate ?? 0m) * (_pricing?.utilityMultiplier ?? 1m);

    public decimal nightlyRateLargeSite =>
        (_pricing?.baseNightlyRate ?? 0m) * (_pricing?.largeSiteMultiplier ?? 1m);

    public async Task OnGetAsync()
    {
        _pricing = await _db.Pricing.FirstOrDefaultAsync();
    }
}
