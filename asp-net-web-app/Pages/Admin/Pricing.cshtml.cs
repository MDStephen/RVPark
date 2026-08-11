using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using asp_net_web_app.Data;

namespace asp_net_web_app.Pages;

public class PricingModel : PageModel
{
    private readonly DatabaseWrapper _db;

    public PricingModel(DatabaseWrapper db){
        _db = db;
    }

    [BindProperty]
    public decimal BaseNightlyRate { get; set; }

    [BindProperty]
    public decimal BaseMonthlyRateStorage { get; set; }

    [BindProperty]
    public decimal SeasonMultiplier { get; set; }

    [BindProperty]
    public decimal LargeSiteMultiplier { get; set; }

    [BindProperty]
    public decimal UtilityMultiplier { get; set; }

    [BindProperty]
    public decimal SpecialEventMultiplier { get; set; }

    [BindProperty]
    public decimal CancellationFee { get; set; }

    [BindProperty]
    public decimal EarlyCheckInFee { get; set; }

    [BindProperty]
    public decimal LateCheckOutFee { get; set; }

    public DateTime LastUpdated { get; set; }

    public async Task OnGetAsync()
    {
        var pricing = await _db.Pricing.FirstOrDefaultAsync();
        if (pricing == null) return; // no row yet - form just shows zeros until saved once

        BaseNightlyRate = pricing.baseNightlyRate;
        BaseMonthlyRateStorage = pricing.baseMonthlyRateStorage;
        SeasonMultiplier = pricing.seasonMultiplier;
        LargeSiteMultiplier = pricing.largeSiteMultiplier;
        UtilityMultiplier = pricing.utilityMultiplier;
        SpecialEventMultiplier = pricing.specialEventMultiplier;
        CancellationFee = pricing.cancellationFee;
        EarlyCheckInFee = pricing.earlyCheckInFee;
        LateCheckOutFee = pricing.lateCheckOutFee;
        LastUpdated = pricing.lastUpdated;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var pricing = await _db.Pricing.FirstOrDefaultAsync();
        if (pricing == null)
        {
            pricing = new Pricing();
            _db.Pricing.Add(pricing);
        }

        pricing.baseNightlyRate = BaseNightlyRate;
        pricing.baseMonthlyRateStorage = BaseMonthlyRateStorage;
        pricing.seasonMultiplier = SeasonMultiplier;
        pricing.largeSiteMultiplier = LargeSiteMultiplier;
        pricing.utilityMultiplier = UtilityMultiplier;
        pricing.specialEventMultiplier = SpecialEventMultiplier;
        pricing.cancellationFee = CancellationFee;
        pricing.earlyCheckInFee = EarlyCheckInFee;
        pricing.lateCheckOutFee = LateCheckOutFee;
        pricing.lastUpdated = DateTime.Now;

        await _db.SaveChangesAsync();

        return RedirectToPage();
    }
}
