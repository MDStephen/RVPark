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

    public decimal nightlyRate =>
        PricingStore.BaseRate;

    public decimal nightlyRateDuringHighSeason =>
        PricingStore.BaseRate *
        PricingStore.SeasonMult;

    public decimal nightlyRatePlusUtilities =>
        PricingStore.BaseRate *
        PricingStore.UtilMult;

    public decimal nightlyRateLargeSite => 
        PricingStore.BaseRate *
        PricingStore.LargeSiteMult;

    public async Task OnGetAsync()
    {
    }
}
