using asp_net_web_app.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

public class UserLoginPageModel : PageModel
{
    private readonly DatabaseWrapper _db;

    public UserLoginPageModel(DatabaseWrapper db)
    {
        _db = db;
    }

    [BindProperty]
    [Required]
    public string Username { get; set; }

    [BindProperty]
    [Required]
    public string Password { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // --- 1) Try staff/admin first (Employees table). ---
        // Note: employee passwords are stored as plain text right now (the team's
        // prototype convention). Eventually these should be hashed like customers.
        var employee = await _db.Employees
            .FirstOrDefaultAsync(e => e.username == Username);

        if (employee != null)
        {
            if (employee.isLocked)
            {
                ModelState.AddModelError(string.Empty, "This account is locked.");
                return Page();
            }

            if (employee.password == Password)
            {
                // Their real role ("Admin"/"Staff") goes in the cookie so we can
                // check it later. Either way they're not a customer, so they go
                // to the dashboard.
                await SignInAsync(employee.username, employee.role);
                return RedirectToPage("/Admin/Dashboard");
            }

            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return Page();
        }

        // --- 2) Otherwise try a customer (UserAccounts table). ---
        var account = await _db.UserAccounts
            .FirstOrDefaultAsync(a => a.Username == Username);

        // Verify against the BCrypt hash (never the raw password).
        if (account != null && BCrypt.Net.BCrypt.Verify(Password, account.PasswordHash))
        {
            // To REQUIRE a verified email before allowing login, uncomment this:
            // if (!account.IsEmailVerified)
            // {
            //     ModelState.AddModelError(string.Empty, "Please verify your email before logging in.");
            //     return Page();
            // }

            await SignInAsync(account.Username, "Customer");
            return RedirectToPage("/CustomerFacing/Index");
        }

        // --- 3) Nothing matched. ---
        ModelState.AddModelError(string.Empty, "Invalid username or password.");
        return Page();
    }

    // Builds the login cookie: who they are (Name) and their role.
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
