using asp_net_web_app.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace asp_net_web_app.Pages.Admin;

public class SeedTestDataModel : PageModel
{
    private readonly DatabaseWrapper _db;

    public SeedTestDataModel(DatabaseWrapper db)
    {
        _db = db;
    }

    public string? Message { get; set; }
    public bool Success { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            // Clear existing data
            _db.Payments.RemoveRange(_db.Payments);
            _db.Reservations.RemoveRange(_db.Reservations);
            _db.SitePhotos.RemoveRange(_db.SitePhotos);
            _db.SitePrices.RemoveRange(_db.SitePrices);
            _db.Sites.RemoveRange(_db.Sites);
            _db.Pricing.RemoveRange(_db.Pricing);
            _db.Employees.RemoveRange(_db.Employees);
            _db.Users.RemoveRange(_db.Users);
            await _db.SaveChangesAsync();

            // Seed Users
            var users = new List<Users>
            {
                new() { userId = 1, firstName = "John", lastName = "Smith", emailAddress = "john.smith@example.com", phoneNumber = "555-0101", address = "123 Main St, Ogden, UT"},
                new() { userId = 2, firstName = "Jane", lastName = "Doe", emailAddress = "jane.doe@example.com", phoneNumber = "555-0102", address = "456 Oak Ave, Layton, UT"},
                new() { userId = 3, firstName = "Robert", lastName = "Johnson", emailAddress = "robert.j@example.com", phoneNumber = "555-0103", address = "789 Pine Rd, Clearfield, UT"},
                new() { userId = 4, firstName = "Emily", lastName = "Davis", emailAddress = "emily.d@example.com", phoneNumber = "555-0104", address = "321 Elm St, Roy, UT"},
                new() { userId = 5, firstName = "Michael", lastName = "Wilson", emailAddress = "michael.w@example.com", phoneNumber = "555-0105", address = "654 Maple Dr, Syracuse, UT"}
            };
            _db.Users.AddRange(users);

            // TODO seed data for the UserAccounts table that correspond with the accounts above

            // Seed Employees
            var employees = new List<Employee>
            {
                new() { employeeId = 1, firstName = "Admin", lastName = "User", username = "admin", password = "admin123", role = "Admin" },
                new() { employeeId = 2, firstName = "Staff", lastName = "Member", username = "staff", password = "staff123", role = "Staff" }
            };
            _db.Employees.AddRange(employees);

            // Seed Pricing
            var pricing = new Pricing
            {
                pricingId = 1,
                baseNightlyRate = 45.00m,
                baseMonthlyRateStorage = 60.00m,
                seasonMultiplier = 1.0m,
                largeSiteMultiplier = 1.2m,
                utilityMultiplier = 1.1m,
                lastUpdated = DateTime.Now,
                cancellationFee = 25.00m,
                earlyCheckInFee = 15.00m,
                lateCheckOutFee = 20.00m,
                specialEventMultiplier = 1.5m
            };
            _db.Pricing.Add(pricing);

            // Seed Sites
            var sites = new List<DbSite>
            {
                new() { SiteNumber = "A1", Category = "Full Hookup", IsAvailable = true },
                new() { SiteNumber = "A2", Category = "Full Hookup", IsAvailable = true },
                new() { SiteNumber = "A3", Category = "Full Hookup", IsAvailable = false },
                new() { SiteNumber = "B1", Category = "Standard", IsAvailable = true },
                new() { SiteNumber = "B2", Category = "Standard", IsAvailable = true },
                new() { SiteNumber = "B3", Category = "Standard", IsAvailable = true },
                new() { SiteNumber = "C1", Category = "Premium", IsAvailable = true },
                new() { SiteNumber = "C2", Category = "Premium", IsAvailable = false },
                new() { SiteNumber = "T1", Category = "Tent", IsAvailable = true },
                new() { SiteNumber = "T2", Category = "Tent", IsAvailable = true },
                new() { SiteNumber = "O1", Category = "Overflow", IsAvailable = true },
                new() { SiteNumber = "S1", Category = "Dry Storage", IsAvailable = true }
            };
            _db.Sites.AddRange(sites);
            await _db.SaveChangesAsync();

            // Seed Site Photos (must be after Sites are saved to get valid IDs)
            var photos = new List<DbSitePhoto>
            {
                new() { DbSiteId = 1, PhotoUrl = "/images/site-a1.jpg" },
                new() { DbSiteId = 2, PhotoUrl = "/images/site-a2.jpg" },
                new() { DbSiteId = 4, PhotoUrl = "/images/site-b1.jpg" },
                new() { DbSiteId = 7, PhotoUrl = "/images/site-c1.jpg" }
            };
            _db.SitePhotos.AddRange(photos);

            // Seed Site Prices (must be after Sites are saved to get valid IDs)
            var sitePrices = new List<DbSitePrice>
            {
                new() { DbSiteId = 1, Cost = 45.00m, Start = DateTime.Today, End = DateTime.Today.AddMonths(6) },
                new() { DbSiteId = 2, Cost = 45.00m, Start = DateTime.Today, End = DateTime.Today.AddMonths(6) },
                new() { DbSiteId = 4, Cost = 40.00m, Start = DateTime.Today, End = DateTime.Today.AddMonths(6) },
                new() { DbSiteId = 7, Cost = 55.00m, Start = DateTime.Today, End = DateTime.Today.AddMonths(6) }
            };
            _db.SitePrices.AddRange(sitePrices);
            await _db.SaveChangesAsync();

            // Seed Reservations
            var today = DateTime.Today;
            var reservations = new List<Reservations>
            {
                new() { UserId = 1, SiteId = 1, StartDate = today.AddDays(5), EndDate = today.AddDays(7), Status = "Confirmed", TotalCost = 90.00m, Adults = 2, Children = 0, Pets = 1, Notes = "Early arrival requested" },
                new() { UserId = 2, SiteId = 4, StartDate = today.AddDays(10), EndDate = today.AddDays(12), Status = "Pending", TotalCost = 80.00m, Adults = 1, Children = 2, Pets = 0, Notes = "" },
                new() { UserId = 3, SiteId = 5, StartDate = today.AddDays(-2), EndDate = today.AddDays(2), Status = "Confirmed", TotalCost = 160.00m, Adults = 2, Children = 1, Pets = 2, Notes = "Extended stay" },
                new() { UserId = 4, SiteId = 7, StartDate = today.AddDays(15), EndDate = today.AddDays(17), Status = "Upcoming", TotalCost = 110.00m, Adults = 1, Children = 0, Pets = 0, Notes = "" },
                new() { UserId = 5, SiteId = 2, StartDate = today.AddDays(20), EndDate = today.AddDays(25), Status = "Upcoming", TotalCost = 225.00m, Adults = 3, Children = 2, Pets = 1, Notes = "Family reunion" }
            };
            _db.Reservations.AddRange(reservations);

            // Seed Payments
            var payments = new List<asp_net_web_app.Data.Payment>
            {
                new() { amount = 90.00m, paidAt = DateTime.UtcNow, stripeId = "pi_test_001", paymentStatus = "paid", ReservationId = 1 },
                new() { amount = 160.00m, paidAt = DateTime.UtcNow, stripeId = "pi_test_002", paymentStatus = "paid", ReservationId = 3 }
            };
            _db.Payments.AddRange(payments);

            await _db.SaveChangesAsync();

            Message = $"Successfully seeded test data: {users.Count} users, {employees.Count} employees, {sites.Count} sites, {reservations.Count} reservations, {payments.Count} payments.";
            Success = true;
        }
        catch (Exception ex)
        {
            Message = $"Error seeding test data: {ex.Message}";
            Success = false;
        }

        return Page();
    }
}
