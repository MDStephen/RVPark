using asp_net_web_app.Data;
using Microsoft.EntityFrameworkCore;

namespace asp_net_web_app.Services;

public enum AvailabilityFilter
{
    /// <summary>Site is free for the entire requested date range.</summary>
    EntireRange,
    /// <summary>Site has at least one open day within the requested date range.</summary>
    AnyDuringRange
}

public record SiteSearchResult(
    int SiteId,
    string SiteNumber,
    string Category,
    decimal EstimatedTotal,
    int Nights,
    bool FullyAvailable);

public class SiteAvailabilityService
{
    private readonly DatabaseWrapper _db;

    private static readonly Dictionary<string, decimal> DefaultNightlyRates = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Standard"] = 45m,
        ["Premium"] = 55m,
        ["Full Hookup"] = 45m,
        ["Tent"] = 25m,
        ["Overflow"] = 25m,
        ["Dry Storage"] = 2m,
    };

    public SiteAvailabilityService(DatabaseWrapper db)
    {
        _db = db;
    }

    public async Task<List<SiteSearchResult>> SearchSitesAsync(
        DateTime? checkIn,
        DateTime? checkOut,
        string? category,
        AvailabilityFilter filter = AvailabilityFilter.EntireRange)
    {
        var sites = await _db.Sites.AsNoTracking().ToListAsync();
        if (!string.IsNullOrWhiteSpace(category))
        {
            sites = sites.Where(s =>
                s.Category.Contains(category, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (!checkIn.HasValue || !checkOut.HasValue || checkOut <= checkIn)
        {
            return sites.Select(s => new SiteSearchResult(
                s.Id, s.SiteNumber, s.Category, 0, 0, s.IsAvailable)).ToList();
        }

        var activeReservations = await _db.Reservations
            .AsNoTracking()
            .Where(r => r.Status != "Cancelled")
            .ToListAsync();

        var nights = (checkOut.Value - checkIn.Value).Days;
        var results = new List<SiteSearchResult>();

        foreach (var site in sites.Where(s => s.IsAvailable))
        {
            var siteReservations = activeReservations.Where(r => r.SiteId == site.Id).ToList();
            var fullyAvailable = !siteReservations.Any(r =>
                RangesOverlap(checkIn.Value, checkOut.Value, r.StartDate, r.EndDate));

            var hasPartialAvailability = HasOpenDayInRange(
                checkIn.Value, checkOut.Value, siteReservations);

            var matches = filter switch
            {
                AvailabilityFilter.EntireRange => fullyAvailable,
                AvailabilityFilter.AnyDuringRange => hasPartialAvailability,
                _ => fullyAvailable
            };

            if (!matches)
                continue;

            var total = await CalculateTotalCostAsync(site.Id, checkIn.Value, checkOut.Value);
            results.Add(new SiteSearchResult(
                site.Id, site.SiteNumber, site.Category, total, nights, fullyAvailable));
        }

        return results.OrderBy(r => r.SiteNumber).ToList();
    }

    public bool IsSiteAvailableForEntireRange(int siteId, DateTime checkIn, DateTime checkOut)
    {
        return !_db.Reservations
            .AsNoTracking()
            .Any(r =>
                r.SiteId == siteId &&
                r.Status != "Cancelled" &&
                checkIn < r.EndDate &&
                r.StartDate < checkOut);
    }

    public async Task<decimal> CalculateTotalCostAsync(int siteId, DateTime checkIn, DateTime checkOut)
    {
        var nights = (checkOut - checkIn).Days;
        if (nights <= 0)
            return 0;

        var priceRules = await _db.SitePrices
            .AsNoTracking()
            .Where(p => p.DbSiteId == siteId)
            .ToListAsync();

        if (priceRules.Count > 0)
        {
            decimal total = 0;
            for (var day = checkIn.Date; day < checkOut.Date; day = day.AddDays(1))
            {
                var rule = priceRules.FirstOrDefault(p => day >= p.Start.Date && day <= p.End.Date);
                total += rule?.Cost ?? GetDefaultNightlyRate(siteId);
            }
            return total;
        }

        var site = await _db.Sites.AsNoTracking().FirstOrDefaultAsync(s => s.Id == siteId);
        var nightly = site != null ? GetDefaultNightlyRate(site.Category) : 45m;
        return nightly * nights;
    }

    private decimal GetDefaultNightlyRate(int siteId)
    {
        var site = _db.Sites.AsNoTracking().FirstOrDefault(s => s.Id == siteId);
        return site != null ? GetDefaultNightlyRate(site.Category) : 45m;
    }

    private static decimal GetDefaultNightlyRate(string category)
    {
        if (DefaultNightlyRates.TryGetValue(category, out var rate))
            return rate;

        return 45m;
    }

    private static bool RangesOverlap(DateTime start1, DateTime end1, DateTime start2, DateTime end2)
    {
        return start1 < end2 && start2 < end1;
    }

    private static bool HasOpenDayInRange(
        DateTime checkIn, DateTime checkOut, List<Reservations> reservations)
    {
        for (var day = checkIn.Date; day < checkOut.Date; day = day.AddDays(1))
        {
            var dayEnd = day.AddDays(1);
            var booked = reservations.Any(r =>
                RangesOverlap(day, dayEnd, r.StartDate, r.EndDate));
            if (!booked)
                return true;
        }
        return false;
    }
}
