using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using asp_net_web_app.Data;

namespace asp_net_web_app.Pages;

public class AdminManageSiteModel : PageModel
{
    private readonly SiteLogic _siteLogic;

    public AdminManageSiteModel(SiteLogic siteLogic)
    {
        _siteLogic = siteLogic;
    }

    public List<Site> SiteList { get; set; } = new();

    public async Task OnGetAsync()
    {
        SiteList = await _siteLogic.GetAllSitesAsync();
    }

    // Create — no fields to collect yet, just adds a blank Site row.
    public IActionResult OnPostCreate()
    {
        _siteLogic.AddSite();
        return RedirectToPage();
    }

    // Edit — stub only, since Site has no fields yet to edit.
    public IActionResult OnPostEdit(int id)
    {
        _siteLogic.EditSite(id);
        return RedirectToPage();
    }

    public IActionResult OnPostDelete(int id)
    {
        _siteLogic.DeleteSite(id);
        return RedirectToPage();
    }
}
