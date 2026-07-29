using asp_net_web_app.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

public class VerifyEmailModel : PageModel
{
    private readonly DatabaseWrapper _db;

    public VerifyEmailModel(DatabaseWrapper db)
    {
        _db = db;
    }

    // These drive what the page shows the visitor.
    public bool Success { get; private set; }
    public string Message { get; private set; } = string.Empty;

    // Reached by clicking the link in the confirmation email:
    //   /CustomerFacing/VerifyEmail?token=abc123...
    public async Task<IActionResult> OnGetAsync(string? token)
    {
        // No token in the URL at all.
        if (string.IsNullOrWhiteSpace(token))
        {
            Success = false;
            Message = "This verification link is missing its token.";
            return Page();
        }

        // Find the account whose stored token matches the one in the link.
        var account = await _db.UserAccounts
            .FirstOrDefaultAsync(a => a.EmailVerificationToken == token);

        // No match: the token was already used (we clear it after verifying)
        // or it was never valid.
        if (account == null)
        {
            Success = false;
            Message = "This verification link is invalid or has already been used.";
            return Page();
        }

        // Good token: mark the account verified, then null the token so the same
        // link can't be used a second time (one-time use).
        account.IsEmailVerified = true;
        account.EmailVerificationToken = null;
        await _db.SaveChangesAsync();

        Success = true;
        Message = "Your email has been verified. You can now log in.";
        return Page();
    }
}
