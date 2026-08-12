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
            // Clean the database of any non-essential data before reseeding.
            _db.Payments.RemoveRange(_db.Payments);
            _db.Reservations.RemoveRange(_db.Reservations);
            _db.SitePhotos.RemoveRange(_db.SitePhotos);
            _db.SitePrices.RemoveRange(_db.SitePrices);
            _db.Sites.RemoveRange(_db.Sites);
            _db.Pricing.RemoveRange(_db.Pricing);
            _db.Employees.RemoveRange(_db.Employees);
            _db.UserAccounts.RemoveRange(_db.UserAccounts);
            _db.Users.RemoveRange(_db.Users);
            await _db.SaveChangesAsync();

            // Seed the seed customer. Every pre-seeded reservation below books
            // under this single user, per the demo setup requirements.
            var seedCustomer = new Users
            {
                userId = 1,
                firstName = "John",
                lastName = "Smith",
                emailAddress = "johnsmith@example.com",
                phoneNumber = "555-0101",
                address = "123 Main St, Ogden, UT"
            };
            _db.Users.Add(seedCustomer);

            const string testPassword = "password123";
            var seedCustomerAccount = new UserAccount
            {
                UserId = seedCustomer.userId,
                Username = seedCustomer.emailAddress,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(testPassword),
                IsEmailVerified = true
            };
            _db.UserAccounts.Add(seedCustomerAccount);

            // One admin and one employee account.
            // NOTE: dateOfBirth is a non-nullable DateTime with no default set leaving it
            // unset means DateTime.MinValue (0001-01-01), which SQL Server's `datetime` column
            // can't store (valid range starts 1753-01-01). This was the actual seeding failure.
            var employees = new List<Employee>
            {
                new() { employeeId = 1, firstName = "Admin", lastName = "User", dateOfBirth = new DateTime(1985, 3, 14), username = "admin", password = "admin123", role = "Admin" },
                new() { employeeId = 2, firstName = "Staff", lastName = "Member", dateOfBirth = new DateTime(1992, 7, 22), username = "staff", password = "staff123", role = "Staff" }
            };
            _db.Employees.AddRange(employees);

            // Seed Pricing - all fees and multipliers in one row (single-row table).
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

            // Seed Sites - every site type represented, 2-3 sites each.
            var sites = new List<DbSite>
            {
                // Full Hookup (3)
                new() { SiteNumber = "A1", Category = "Full Hookup", IsAvailable = true },
                new() { SiteNumber = "A2", Category = "Full Hookup", IsAvailable = true },
                new() { SiteNumber = "A3", Category = "Full Hookup", IsAvailable = false },
                // Standard (3)
                new() { SiteNumber = "B1", Category = "Standard", IsAvailable = true },
                new() { SiteNumber = "B2", Category = "Standard", IsAvailable = true },
                new() { SiteNumber = "B3", Category = "Standard", IsAvailable = true },
                // Premium (2)
                new() { SiteNumber = "C1", Category = "Premium", IsAvailable = true },
                new() { SiteNumber = "C2", Category = "Premium", IsAvailable = false },
                // Tent (2)
                new() { SiteNumber = "T1", Category = "Tent", IsAvailable = true },
                new() { SiteNumber = "T2", Category = "Tent", IsAvailable = true },
                // Overflow (2)
                new() { SiteNumber = "O1", Category = "Overflow", IsAvailable = true },
                new() { SiteNumber = "O2", Category = "Overflow", IsAvailable = true },
                // Dry Storage (2)
                new() { SiteNumber = "S1", Category = "Dry Storage", IsAvailable = true },
                new() { SiteNumber = "S2", Category = "Dry Storage", IsAvailable = true }
            };
            _db.Sites.AddRange(sites);
            await _db.SaveChangesAsync(); // sites now have real DB-generated Ids

            // Seed Site Photos (uses the real generated Site Ids, not assumed literals)
            var photos = new List<DbSitePhoto>
            {
                new() { DbSiteId = sites[0].Id, PhotoUrl = "/images/site-a1.jpg" },
                new() { DbSiteId = sites[1].Id, PhotoUrl = "/images/site-a2.jpg" },
                new() { DbSiteId = sites[2].Id, PhotoUrl = "/images/site-a3.jpg" },
                new() { DbSiteId = sites[3].Id, PhotoUrl = "/images/site-b1.jpg" },
                new() { DbSiteId = sites[4].Id, PhotoUrl = "/images/site-b2.jpg" },
                new() { DbSiteId = sites[5].Id, PhotoUrl = "/images/site-b3.jpg" },
                new() { DbSiteId = sites[6].Id, PhotoUrl = "/images/site-c1.jpg" },
                new() { DbSiteId = sites[7].Id, PhotoUrl = "/images/site-c2.jpg" },
                new() { DbSiteId = sites[8].Id, PhotoUrl = "/images/site-t1.jpg" },
                new() { DbSiteId = sites[9].Id, PhotoUrl = "/images/site-t1.jpg" },
                new() { DbSiteId = sites[10].Id, PhotoUrl = "/images/site-o1.jpg" },
                new() { DbSiteId = sites[11].Id, PhotoUrl = "/images/site-o1.jpg" },
                new() { DbSiteId = sites[12].Id, PhotoUrl = "/images/site-s1.jpg" },
                new() { DbSiteId = sites[13].Id, PhotoUrl = "/images/site-s1.jpg" }
            };
            _db.SitePhotos.AddRange(photos);

            // Seed Site Prices (same - real generated Site Ids)
            var sitePrices = new List<DbSitePrice>
            {
                new() { DbSiteId = sites[0].Id, Cost = 45.00m, Start = DateTime.Today, End = DateTime.Today.AddMonths(6) },
                new() { DbSiteId = sites[3].Id, Cost = 40.00m, Start = DateTime.Today, End = DateTime.Today.AddMonths(6) },
                new() { DbSiteId = sites[6].Id, Cost = 55.00m, Start = DateTime.Today, End = DateTime.Today.AddMonths(6) },
                new() { DbSiteId = sites[8].Id, Cost = 30.00m, Start = DateTime.Today, End = DateTime.Today.AddMonths(6) },
                new() { DbSiteId = sites[10].Id, Cost = 25.00m, Start = DateTime.Today, End = DateTime.Today.AddMonths(6) },
                new() { DbSiteId = sites[12].Id, Cost = 60.00m, Start = DateTime.Today, End = DateTime.Today.AddMonths(6) }
            };
            _db.SitePrices.AddRange(sitePrices);
            await _db.SaveChangesAsync();

            // Seed Reservations - 12, all under the single seed customer, spanning
            // past/current/future dates across every status the filter system needs
            // to demonstrate (Completed, Cancelled, Confirmed, Pending, Upcoming).
            var today = DateTime.Today;
            var uid = seedCustomer.userId;
            var reservations = new List<Reservations>
            {
                new() { UserId = uid, SiteId = sites[0].Id,  StartDate = today.AddDays(-30), EndDate = today.AddDays(-28), Status = "Completed", TotalCost = 90.00m,  Adults = 2, Children = 0, Pets = 1, Notes = "Past stay" },
                new() { UserId = uid, SiteId = sites[3].Id,  StartDate = today.AddDays(-20), EndDate = today.AddDays(-18), Status = "Completed", TotalCost = 80.00m,  Adults = 1, Children = 2, Pets = 0, Notes = "" },
                new() { UserId = uid, SiteId = sites[8].Id,  StartDate = today.AddDays(-15), EndDate = today.AddDays(-13), Status = "Cancelled", TotalCost = 60.00m,  Adults = 2, Children = 0, Pets = 0, Notes = "Cancelled by guest" },
                new() { UserId = uid, SiteId = sites[6].Id,  StartDate = today.AddDays(8),   EndDate = today.AddDays(10),  Status = "Cancelled", TotalCost = 110.00m, Adults = 2, Children = 1, Pets = 0, Notes = "Cancelled - schedule conflict" },
                new() { UserId = uid, SiteId = sites[4].Id,  StartDate = today.AddDays(-2),  EndDate = today.AddDays(2),   Status = "Confirmed", TotalCost = 160.00m, Adults = 2, Children = 1, Pets = 2, Notes = "Extended stay" },
                new() { UserId = uid, SiteId = sites[1].Id,  StartDate = today.AddDays(5),   EndDate = today.AddDays(7),   Status = "Confirmed", TotalCost = 90.00m,  Adults = 2, Children = 0, Pets = 1, Notes = "Early arrival requested" },
                new() { UserId = uid, SiteId = sites[9].Id,  StartDate = today.AddDays(12),  EndDate = today.AddDays(14),  Status = "Confirmed", TotalCost = 60.00m,  Adults = 1, Children = 0, Pets = 0, Notes = "" },
                new() { UserId = uid, SiteId = sites[10].Id, StartDate = today.AddDays(10),  EndDate = today.AddDays(12),  Status = "Pending",   TotalCost = 50.00m,  Adults = 1, Children = 2, Pets = 0, Notes = "" },
                new() { UserId = uid, SiteId = sites[12].Id, StartDate = today.AddDays(25),  EndDate = today.AddDays(27),  Status = "Pending",   TotalCost = 120.00m, Adults = 1, Children = 0, Pets = 0, Notes = "Storage booking" },
                new() { UserId = uid, SiteId = sites[7].Id,  StartDate = today.AddDays(15),  EndDate = today.AddDays(17),  Status = "Upcoming",  TotalCost = 110.00m, Adults = 1, Children = 0, Pets = 0, Notes = "" },
                new() { UserId = uid, SiteId = sites[2].Id,  StartDate = today.AddDays(20),  EndDate = today.AddDays(25),  Status = "Upcoming",  TotalCost = 225.00m, Adults = 3, Children = 2, Pets = 1, Notes = "Family reunion" },
                new() { UserId = uid, SiteId = sites[5].Id,  StartDate = today.AddDays(40),  EndDate = today.AddDays(45),  Status = "Upcoming",  TotalCost = 200.00m, Adults = 2, Children = 0, Pets = 0, Notes = "Booked far in advance" }
            };
            _db.Reservations.AddRange(reservations);
            await _db.SaveChangesAsync(); // reservations now have real generated Ids

            var payments = new List<asp_net_web_app.Data.Payment>
            {
                new() { amount = 90.00m,  paidAt = DateTime.UtcNow, stripeId = "pi_test_001", paymentStatus = "paid", ReservationId = reservations[0].Id },
                new() { amount = 160.00m, paidAt = DateTime.UtcNow, stripeId = "pi_test_002", paymentStatus = "paid", ReservationId = reservations[4].Id }
            };
            _db.Payments.AddRange(payments);

            await _db.SaveChangesAsync();

            Message = $"Successfully seeded test data: 1 admin, 1 employee, 1 seed customer, {sites.Count} sites, {reservations.Count} reservations, {payments.Count} payments.";
            Success = true;
        }
        catch (Exception ex)
        {
            // the actual SQL error is in the inner exception chain, if occurring
            var innermost = ex;
            while (innermost.InnerException != null)
                innermost = innermost.InnerException;

            Message = $"Error seeding test data: {innermost.Message}";
            Success = false;
        }

        return Page();
    }
}
