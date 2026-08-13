using asp_net_web_app.Data;
using asp_net_web_app.Repositories;
using asp_net_web_app.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
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
builder.Services.AddScoped<IEmailSender, DevEmailSender>();

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin", "EmployeeOnly");
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("EmployeeOnly", policy =>
    {
        policy.RequireRole("Admin", "Staff", "admin", "staff");
    });
});

// Cookie authentication: after a successful login we drop a cookie in the
// browser, and it's sent back on every request that's what keeps someone
// logged in from page to page without signing in again each time.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Where to send people who need to log in.
        options.LoginPath = "/CustomerFacing/UserLoginPage";
        options.AccessDeniedPath = "/CustomerFacing/AccessDenied";
    });

var stripeSecretKey = builder.Configuration["Stripe:SecretKey"];
if (!string.IsNullOrWhiteSpace(stripeSecretKey))
{
    StripeConfiguration.ApiKey = stripeSecretKey;
}

var app = builder.Build();

app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// seed the pricing table
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DatabaseWrapper>();

    db.Database.Migrate();

    if (!db.Pricing.Any())
    {
        db.Pricing.Add(new Pricing
        {
            pricingId = 1,
            baseNightlyRate = 25m,
            baseMonthlyRateStorage = 125m,
            seasonMultiplier = 1.25m,
            largeSiteMultiplier = 1.15m,
            utilityMultiplier = 1.80m,
            cancellationFee = 20m,
            earlyCheckInFee = 10m,
            lateCheckOutFee = 10m,
            specialEventMultiplier = 1.50m,
            lastUpdated = DateTime.Now
        });

        db.SaveChanges();
    }
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DatabaseWrapper>();

    if (!db.Employees.Any(e => e.username == "admin"))
    {
        db.Employees.Add(new Employee
        {
            firstName = "Admin",
            lastName = "Account",
            username = "admin",
            password = "admin123",
            role = "Admin",
            isLocked = false,
            dateOfBirth = new DateTime(2000, 1, 1),
            CreatedAt = DateTime.UtcNow,
            LastModifiedAt = DateTime.UtcNow
        });
        db.SaveChanges();
    }
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();   // works out WHO you are (reads the login cookie)
app.UseAuthorization();    // works out WHAT you're allowed to do

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

QuestPDF.Settings.EnableDebugging = true; // debugging, remove before pushing to server

QuestPDF.Settings.License = LicenseType.Community;
app.Run();
