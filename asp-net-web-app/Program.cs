using asp_net_web_app.Data;
using asp_net_web_app.Repositories;
using asp_net_web_app.Pages;
using asp_net_web_app.Services;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure; // necessary for LicenseType to be recognized

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<DatabaseWrapper>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<UserLogic>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<CreateEmployeeLogic>();
builder.Services.AddRazorPages();


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
