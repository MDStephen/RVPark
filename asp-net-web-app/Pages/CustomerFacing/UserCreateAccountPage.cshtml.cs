using asp_net_web_app.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

public class UserCreateAccountPageModel : PageModel
{
    // The database, handed to us automatically by ASP.NET (dependency injection).
    private readonly DatabaseWrapper _db;

    public UserCreateAccountPageModel(DatabaseWrapper db)
    {
        _db = db;
    }

    [BindProperty]
    [Required]
    public string FirstName { get; set; }

    [BindProperty]
    [Required]
    public string LastName { get; set; }

    [BindProperty]
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [BindProperty]
    [Required]
    [Phone]
    public string PhoneNumber { get; set; }

    [BindProperty]
    [Required]
    public string Address { get; set; }

    [BindProperty]
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [BindProperty]
    [Required]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        // If any field failed its rules (required, matching passwords, etc.),
        // redraw the page with the error messages.
        if (!ModelState.IsValid)
        {
            return Page();
        }

<<<<<<< HEAD
        // Don't allow two accounts with the same email
        // (we're using the email as the username for now).
        bool emailTaken = await _db.UserAccounts.AnyAsync(a => a.Username == Email);
        if (emailTaken)
        {
            ModelState.AddModelError(nameof(Email), "An account with that email already exists.");
            return Page();
        }
=======
        // TODO: create user record with hashed password (BCrypt/ASP.NET Identity PasswordHasher)
        // TODO: add username/password columns to Users table once schema is finalized
        // TODO: sign in and redirect to customer dashboard
>>>>>>> Mahlon-Final-Pages

        // 1) Create the person's profile row in the Users table (as a Customer).
        var customer = new Customer
        {
            firstName    = FirstName,
            lastName     = LastName,
            emailAddress = Email,
            phoneNumber  = PhoneNumber,
            address      = Address
        };
        _db.Users.Add(customer);
        await _db.SaveChangesAsync();   // after this, the database fills in customer.userId

        // 2) Create the matching login row. We store ONLY the hashed password,
        //    never the text the user typed.
        var account = new UserAccount
        {
            UserId          = customer.userId,
            Username        = Email,
            PasswordHash    = BCrypt.Net.BCrypt.HashPassword(Password),
            IsEmailVerified = false
        };
        _db.UserAccounts.Add(account);
        await _db.SaveChangesAsync();

        // For now, send them to the login page.
        // (Email verification is the next piece we'll build.)
        return RedirectToPage("/CustomerFacing/UserLoginPage");
    }
}