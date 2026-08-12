using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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

    // Drives the admin-login modal in Pricing.cshtml - true whenever the current
    // visitor isn't signed in as Admin (staff, customer, or anonymous alike).
    public bool RequiresAdminLogin { get; set; }
    public string? AdminLoginError { get; set; }

    [BindProperty]
    public string? AdminUsername { get; set; }

    [BindProperty]
    public string? AdminPassword { get; set; }

    private bool IsAdmin() =>
        User.Identity?.IsAuthenticated == true && User.IsInRole("Admin");

    public async Task<IActionResult> OnGetAsync()
    {
        if (!IsAdmin())
        {
            RequiresAdminLogin = true;
            return Page();
        }

        var pricing = await _db.Pricing.FirstOrDefaultAsync();
        if (pricing == null) return Page(); // no row yet - form just shows zeros until saved once

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

        return Page();
    }

    // The "Save" form - only reachable if the page already thinks you're Admin, but
    // re-checked here too since a POST can be replayed/forged after a session ends.
    public async Task<IActionResult> OnPostAsync()
    {
        if (!IsAdmin())
        {
            RequiresAdminLogin = true;
            return Page();
        }

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

    // The modal's own form posts here (asp-page-handler="AdminLogin"). On success,
    // upgrades the current session to that admin (same SignInAsync pattern the real
    // login page uses) and reloads the page - which now passes IsAdmin() and shows
    // the real pricing form. On failure, re-shows the modal with an error.
    public async Task<IActionResult> OnPostAdminLoginAsync()
    {
        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.username == AdminUsername);

        if (employee == null || employee.isLocked || employee.role != "Admin" || employee.password != AdminPassword)
        {
            RequiresAdminLogin = true;
            AdminLoginError = "Invalid admin username or password.";
            return Page();
        }

        await SignInAsync(employee.username, employee.role);
        return RedirectToPage();
    }

    // Mirrors UserLoginPageModel's SignInAsync so this page and the real login page
    // build the exact same kind of cookie.
    private async Task SignInAsync(string username, string role)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));
    }
}
