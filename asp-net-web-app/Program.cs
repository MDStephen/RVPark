using asp_net_web_app.Data;
using asp_net_web_app.Repositories;
using asp_net_web_app.Services;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DatabaseWrapper>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<CreateEmployeeLogic>();
builder.Services.AddScoped<EmployeeLogic>();
builder.Services.AddScoped<SiteAvailabilityService>();
builder.Services.AddScoped<IEmailService, ConsoleEmailService>();
builder.Services.AddScoped<IPaymentService, StripePaymentService>();
builder.Services.AddRazorPages();

var stripeSecretKey = builder.Configuration["Stripe:SecretKey"];
if (!string.IsNullOrWhiteSpace(stripeSecretKey))
{
    StripeConfiguration.ApiKey = stripeSecretKey;
}

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DatabaseWrapper>();
    db.Database.Migrate();
    if (!db.Users.Any())
    {
        db.Users.AddRange(
            new Users { firstName = "Jane", lastName = "Doe", emailAddress = "jane@example.com", phoneNumber = "555 555-1234", address = "123 Main St" },
            new Users { firstName = "John", lastName = "Smith", emailAddress = "john@example.com", phoneNumber = "555 555-5678", address = "456 Oak Ave" },
            new Users { firstName = "Bob", lastName = "Johnson", emailAddress = "bob@example.com", phoneNumber = "555 555-2468", address = "987 Center St" }
        );
        db.SaveChanges();
    }

    if (!db.Reservations.Any())
    {
        if (!db.Sites.Any())
        {
            db.Sites.AddRange(
                new DbSite { SiteNumber = "A1", Category = "Standard", IsAvailable = true },
                new DbSite { SiteNumber = "A2", Category = "Standard", IsAvailable = true },
                new DbSite { SiteNumber = "B1", Category = "Premium", IsAvailable = true }
            );
            db.SaveChanges();
        }

        var seededSites = db.Sites.OrderBy(s => s.Id).Take(3).Select(s => s.Id).ToList();
        var firstUserId = db.Users.OrderBy(u => u.userId).Select(u => u.userId).FirstOrDefault();

        db.Reservations.AddRange(
            new Reservations
            {
                UserId = firstUserId,
                SiteId = seededSites.Count > 0 ? seededSites[0] : 1,
                StartDate = DateTime.Today.AddDays(7),
                EndDate = DateTime.Today.AddDays(10),
                Status = "Upcoming",
                TotalCost = 150.00m
            },
            new Reservations
            {
                UserId = firstUserId,
                SiteId = seededSites.Count > 1 ? seededSites[1] : 2,
                StartDate = DateTime.Today.AddDays(14),
                EndDate = DateTime.Today.AddDays(16),
                Status = "Upcoming",
                TotalCost = 90.00m
            },
            new Reservations
            {
                UserId = firstUserId,
                SiteId = seededSites.Count > 2 ? seededSites[2] : 3,
                StartDate = DateTime.Today.AddDays(21),
                EndDate = DateTime.Today.AddDays(25),
                Status = "Upcoming",
                TotalCost = 220.00m
            }
        );
        db.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

QuestPDF.Settings.License = LicenseType.Community;
app.Run();
