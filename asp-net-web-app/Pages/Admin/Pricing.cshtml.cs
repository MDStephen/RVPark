using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace asp_net_web_app.Pages;
public class PricingModel : PageModel
{
    [BindProperty]
    public decimal BaseRate { get; set; }

    [BindProperty]
    public decimal SeasonMult { get; set; }

    [BindProperty]
    public decimal LargeSiteMult { get; set; }

    [BindProperty]
    public decimal UtilMult { get; set; }

    [BindProperty]
    public DateTime UpdatedAt { get; set; }

    public void OnGet()
    {
        BaseRate = PricingStore.BaseRate;
        SeasonMult = PricingStore.SeasonMult;
        LargeSiteMult = PricingStore.LargeSiteMult;
        UtilMult = PricingStore.UtilMult;
        UpdatedAt = PricingStore.UpdatedAt;
    }

    public IActionResult OnPost()
    {
        PricingStore.BaseRate = BaseRate;
        PricingStore.SeasonMult = SeasonMult;
        PricingStore.LargeSiteMult = LargeSiteMult;
        PricingStore.UtilMult = UtilMult;
        PricingStore.UpdatedAt = DateTime.Now;
        return RedirectToPage();
    }
}