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

            // Pull their profile row so we can put a first name in the cookie
            // (used by the nav bar's "Welcome, ___" indicator).
            // Note: filtering with .OfType<Customer>() here doesn't translate against
            // this TPH set - EF throws InvalidOperationException. Filtering on the base
            // Users set does translate, so match on userId there and cast after.
            var customer = await _db.Users
                .FirstOrDefaultAsync(u => u.userId == account.UserId) as Customer;

            await SignInAsync(account.Username, "Customer", account.UserId, customer?.firstName);
            return RedirectToPage("/CustomerFacing/Index");
        }

        // --- 3) Nothing matched. ---
        ModelState.AddModelError(string.Empty, "Invalid username or password.");
        return Page();
    }

    // Builds the login cookie: who they are (Name), their role, and (for customers)
    // their first name for display purposes.
    private async Task SignInAsync(string username, string role, int? userId = null, string? firstName = null)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role)
        };

        // For customers, stash their Users-table id so pages like booking can
        // save records under the right person (read back via User.FindFirst("UserId")).
        if (userId.HasValue)
        {
            claims.Add(new Claim("UserId", userId.Value.ToString()));
        }

        // Drives "Welcome, John" in the nav bar. Falls back to Username in the
        // layout if this isn't present (e.g. staff logins don't set it).
        if (!string.IsNullOrWhiteSpace(firstName))
        {
            claims.Add(new Claim(ClaimTypes.GivenName, firstName));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));
    }
}
