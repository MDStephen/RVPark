using asp_net_web_app.Data;
using asp_net_web_app.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

public class UserCreateAccountPageModel : PageModel
{
    // The database, handed to us automatically by ASP.NET (dependency injection).
    private readonly DatabaseWrapper _db;

    // How we "send" the confirmation email. In development this is DevEmailSender,
    // which just prints the link to the console.
    private readonly IEmailSender _email;

    public UserCreateAccountPageModel(DatabaseWrapper db, IEmailSender email)
    {
        _db = db;
        _email = email;
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

        // Don't allow two accounts with the same email
        // (we're using the email as the username for now).
        bool emailTaken = await _db.UserAccounts.AnyAsync(a => a.Username == Email);
        if (emailTaken)
        {
            ModelState.AddModelError(nameof(Email), "An account with that email already exists.");
            return Page();
        }

        // 1) Create the person's profile row in the Users table (as a Customer).
        var now = DateTime.Now;
        var customer = new Customer
        {
            firstName    = FirstName,
            lastName     = LastName,
            emailAddress = Email,
            phoneNumber  = PhoneNumber,
            address      = Address,
            CreatedAt      = now,
            LastModifiedAt = now
        };
        _db.Users.Add(customer);
        await _db.SaveChangesAsync();   // after this, the database fills in customer.userId

        // 2) Create the matching login row. We store ONLY the hashed password,
        //    never the text the user typed.
        var account = new UserAccount
        {
            UserId                 = customer.userId,
            Username               = Email,
            PasswordHash           = BCrypt.Net.BCrypt.HashPassword(Password),
            IsEmailVerified        = false,
            // A random one-time token that goes in the confirmation link.
            // "N" gives 32 hex chars with no dashes.
            EmailVerificationToken = Guid.NewGuid().ToString("N")
        };
        _db.UserAccounts.Add(account);
        await _db.SaveChangesAsync();

        // 3) Build the absolute verification link and "send" it.
        //    Url.Page(..., protocol: Request.Scheme) produces a full URL like
        //    https://localhost:5046/CustomerFacing/VerifyEmail?token=abc123...
        var verifyLink = Url.Page(
            "/CustomerFacing/VerifyEmail",
            pageHandler: null,
            values: new { token = account.EmailVerificationToken },
            protocol: Request.Scheme);

        await _email.SendVerificationEmailAsync(account.Username, verifyLink!);

        // Send them to the login page. They'll need to click the link in their
        // email (printed to the console for now) before the account is verified.
        return RedirectToPage("/CustomerFacing/UserLoginPage");
    }
}