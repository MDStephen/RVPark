using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

public class UserLoginPageModel : PageModel
{
    [BindProperty]
    [Required]
    public string Username { get; set; }

    [BindProperty]
    [Required]
    public string Password { get; set; }

    public void OnGet() { }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // TODO: validate credentials, hash password, sign in with cookie auth, redirect to customer dashboard
        // TODO: wire up IUserRepository + password hashing once account creation strategy is decided

        return RedirectToPage("/Index");
    }
}
