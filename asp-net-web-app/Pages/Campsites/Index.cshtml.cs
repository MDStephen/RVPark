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

            var photos = await _db.SitePhotos
                .AsNoTracking()
                .ToListAsync();

            var photosBySite = photos
                .GroupBy(p => p.DbSiteId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(p => p.PhotoUrl).ToList());

            Results = allSites.Select(s => new SiteSearchResult(
                s.Id,
                s.SiteNumber,
                s.Category,
                0,
                0,
                s.IsAvailable)
            {
                PhotoUrls = photosBySite.TryGetValue(s.Id, out var sitePhotos)
                    ? sitePhotos
                    : []
            }).ToList();
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

        // Use the logged-in customer's id (from their login cookie) so the
        // reservation is saved under THEM, not a default user.
        var userIdClaim = User.FindFirst("UserId");
        if (userIdClaim == null)
        {
            // Not logged in as a customer — send them to log in first.
            return RedirectToPage("/CustomerFacing/UserLoginPage");
        }
        int currentUserId = int.Parse(userIdClaim.Value);

        var totalCost = await _availability.CalculateTotalCostAsync(siteId, CheckIn.Value, CheckOut.Value);

        var reservation = new Reservations
        {
            UserId = currentUserId,
            SiteId = siteId,
            StartDate = CheckIn.Value,
            EndDate = CheckOut.Value,
            Status = "Pending",
            TotalCost = totalCost,
            Adults = 1,
            Children = 0,
            Pets = 0,
            Notes = "Created from site browse."
        };

        _db.Reservations.Add(reservation);
        await _db.SaveChangesAsync();

        return RedirectToPage("/CustomerFacing/CompleteReservation", new { id = reservation.Id });
    }
}
