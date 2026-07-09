using asp_net_web_app.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<DatabaseWrapper>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<UserLogic>();
builder.Services.AddScoped<EmployeeLogic>();
builder.Services.AddRazorPages();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DatabaseWrapper>();
    db.Database.Migrate();

    // Seed staff data so the Manage Staff page has records to perform CRUD on.
    // Only runs when the Employees table is empty - won't create duplicates.
    if (!db.Employees.Any())
    {
        db.Employees.Add(new Employee
        {
            firstName = "Test",
            lastName = "Admin",
            dateOfBirth = new DateTime(1990, 1, 1),
            username = "tadmin",
            password = "1",
            role = "Admin",
            isLocked = false
        });

        db.Employees.Add(new Employee
        {
            firstName = "Jane",
            lastName = "Desk",
            dateOfBirth = new DateTime(1998, 5, 20),
            username = "jdesk",
            password = "2",
            role = "Employee",
            isLocked = false
        });

        db.Employees.Add(new Employee
        {
            firstName = "Mark",
            lastName = "Gate",
            dateOfBirth = new DateTime(1995, 9, 3),
            username = "mgate",
            password = "3",
            role = "Employee",
            isLocked = true   // one pre-locked employee, nice for demoing Unlock
        });

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

app.Run();
