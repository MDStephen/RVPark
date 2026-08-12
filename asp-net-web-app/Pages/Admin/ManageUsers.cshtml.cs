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

        // Only used while creating a new customer (SelectedId == 0); the login
        // credentials live in UserAccounts, not on the Users model itself.
        [BindProperty]
        public string? NewUsername { get; set; }

        [BindProperty]
        public string? NewPassword { get; set; }

        public string? StatusMessage { get; set; }
        public bool IsError { get; set; }

        // True when SelectedUser is a blank, not-yet-saved customer (selectedId=0 sentinel)
        public bool IsNew { get; set; }

        // ── GET ───────────────────────────────────────────────────────
        // /ManageUsers               -> list, auto-selects first customer
        // /ManageUsers?selectedId=7  -> customer 7 selected for editing
        // /ManageUsers?selectedId=0  -> blank, editable "create new" form
        public async Task OnGetAsync()
        {
            Customers = await _userRepo.GetAllCustomersAsync();

            if (SelectedId == 0)
            {
                SelectedUser = new Customer();
                IsNew = true;
            }
            else if (SelectedId.HasValue)
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

            IsNew = SelectedUser.userId == 0;

            // Re-load list for redisplay on validation failure
            Customers = await _userRepo.GetAllCustomersAsync();

            if (!ModelState.IsValid)
            {
                IsError = true;
                StatusMessage = "Please correct the highlighted fields.";
                return Page();
            }

            if (IsNew)
            {
                var (success, message, newId) = await _userRepo.CreateAsync(SelectedUser, NewUsername, NewPassword);
                if (!success)
                {
                    IsError = true;
                    StatusMessage = message;
                    return Page();
                }

                TempData["StatusMessage"] = "Customer created.";
                return RedirectToPage(new { selectedId = newId });
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

            if (SelectedUser.userId == 0)
            {
                // Nothing was ever saved - just discard the blank "create" form.
                TempData["StatusMessage"] = "New customer discarded.";
                return RedirectToPage();
            }

            await _userRepo.DeleteAsync(SelectedUser.userId);

            TempData["StatusMessage"] = "User deleted.";
            return RedirectToPage();
        }
    }
}
