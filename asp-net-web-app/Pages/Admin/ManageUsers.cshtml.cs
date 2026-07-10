using asp_net_web_app.Data;
using asp_net_web_app.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace asp_net_web_app.Pages.Admin
{
    public class ManageUsersModel : PageModel
    {
        private readonly IUserRepository _userRepo;

        public ManageUsersModel(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        // Full list shown in the left panel
        public List<Users> Customers { get; set; } = new();

        // The user currently selected / being edited
        [BindProperty]
        public Users? SelectedUser { get; set; }

        // Which user ID is selected (passed via query string or form)
        [BindProperty(SupportsGet = true)]
        public int? SelectedId { get; set; }

        public string? StatusMessage { get; set; }
        public bool IsError { get; set; }

        // ── GET ───────────────────────────────────────────────────────
        public async Task OnGetAsync()
        {
            Customers = await _userRepo.GetAllCustomersAsync();

            if (SelectedId.HasValue)
                SelectedUser = await _userRepo.GetByIdAsync(SelectedId.Value);
            else if (Customers.Any())
            {
                // Auto-select the first customer so the panel is never blank
                SelectedUser = Customers.First();
                SelectedId = SelectedUser.userId;
            }
        }

        // ── POST: Save Changes ────────────────────────────────────────
        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (SelectedUser == null)
                return RedirectToPage();

            // Re-load list for redisplay on validation failure
            Customers = await _userRepo.GetAllCustomersAsync();

            if (!ModelState.IsValid)
            {
                IsError = true;
                StatusMessage = "Please correct the highlighted fields.";
                return Page();
            }

            await _userRepo.UpdateAsync(SelectedUser);

            TempData["StatusMessage"] = "Changes saved successfully.";
            return RedirectToPage(new { selectedId = SelectedUser.userId });
        }

        // ── POST: Delete User ─────────────────────────────────────────
        public async Task<IActionResult> OnPostDeleteAsync()
        {
            if (SelectedUser == null)
                return RedirectToPage();

            await _userRepo.DeleteAsync(SelectedUser.userId);

            TempData["StatusMessage"] = "User deleted.";
            return RedirectToPage();
        }
    }
}
