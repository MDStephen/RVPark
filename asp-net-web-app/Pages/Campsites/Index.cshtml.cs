using asp_net_web_app.Data;
using asp_net_web_app.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace asp_net_web_app.Pages.Campsites;

public class IndexModel : PageModel
{
    private readonly SiteAvailabilityService _availability;
    private readonly DatabaseWrapper _db;

    public IndexModel(SiteAvailabilityService availability, DatabaseWrapper db)
    {
        _availability = availability;
        _db = db;
    }

    [BindProperty(SupportsGet = true)]
    public DateTime? CheckIn { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? CheckOut { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SiteType { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? RvLength { get; set; }

    [BindProperty(SupportsGet = true)]
    public AvailabilityFilter AvailabilityMode { get; set; } = AvailabilityFilter.EntireRange;

    public List<SiteSearchResult> Results { get; set; } = [];
    public List<SelectListItem> CategoryOptions { get; set; } = [];
    public string? SearchMessage { get; set; }

    public async Task OnGetAsync()
    {
        CategoryOptions = await _db.Sites
            .AsNoTracking()
            .Select(s => s.Category)
            .Distinct()
            .OrderBy(c => c)
            .Select(c => new SelectListItem(c, c))
            .ToListAsync();

        if (CheckIn.HasValue && CheckOut.HasValue)
        {
            if (CheckOut <= CheckIn)
            {
                SearchMessage = "Check-out must be after check-in.";
                return;
            }

            Results = await _availability.SearchSitesAsync(
                CheckIn, CheckOut, SiteType, AvailabilityMode);

            if (Results.Count == 0)
            {
                SearchMessage = "No sites match your search. Try different dates or switch availability mode.";
            }
        }
        else
        {
            var allSites = await _db.Sites.AsNoTracking()
                .Where(s => s.IsAvailable)
                .OrderBy(s => s.SiteNumber)
                .ToListAsync();

            Results = allSites.Select(s => new SiteSearchResult(
                s.Id, s.SiteNumber, s.Category, 0, 0, s.IsAvailable)).ToList();
        }
    }

    public async Task<IActionResult> OnPostStartReservationAsync(int siteId)
    {
        if (!CheckIn.HasValue || !CheckOut.HasValue || CheckOut <= CheckIn)
        {
            return RedirectToPage(new { checkIn = CheckIn, checkOut = CheckOut, siteType = SiteType });
        }

        if (!_availability.IsSiteAvailableForEntireRange(siteId, CheckIn.Value, CheckOut.Value))
        {
            return RedirectToPage(new
            {
                checkIn = CheckIn,
                checkOut = CheckOut,
                siteType = SiteType,
                availabilityMode = AvailabilityMode
            });
        }

        // TODO: Replace guest user with authenticated customer once login/account creation is implemented.
        var guestUserId = await _db.Users
            .OrderBy(u => u.userId)
            .Select(u => u.userId)
            .FirstOrDefaultAsync<int>();

        if (guestUserId == 0)
        {
            return RedirectToPage(new { checkIn = CheckIn, checkOut = CheckOut });
        }

        var totalCost = await _availability.CalculateTotalCostAsync(siteId, CheckIn.Value, CheckOut.Value);

        var reservation = new Reservations
        {
            UserId = guestUserId,
            SiteId = siteId,
            StartDate = CheckIn.Value,
            EndDate = CheckOut.Value,
            Status = "Pending",
            TotalCost = totalCost,
            Adults = 1,
            Children = 0,
            Pets = 0,
            Notes = "Created from public site browse  assign to logged-in customer once auth is ready."
        };

        _db.Reservations.Add(reservation);
        await _db.SaveChangesAsync();

        return RedirectToPage("/CustomerFacing/CompleteReservation", new { id = reservation.Id });
    }
}
