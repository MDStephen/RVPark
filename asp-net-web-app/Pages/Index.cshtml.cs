using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using asp_net_web_app.Data;

namespace asp_net_web_app.Pages;

public class IndexModel : PageModel
{
    private readonly DatabaseWrapper _db;
    private readonly UserLogic _userLogic;

    public IndexModel(DatabaseWrapper db, UserLogic userLogic)
    {
        _db = db;
        _userLogic = userLogic;
    }

    public List<Users> UserList { get; set; } = new();

    [BindProperty]
    public string Username { get; set; } = "";

    public async Task OnGetAsync()
    {
        UserList = await _db.Users.ToListAsync();
    }

    public IActionResult OnPost()
    {
        _userLogic.AddUser(Username);
        return RedirectToPage();
    }
}
