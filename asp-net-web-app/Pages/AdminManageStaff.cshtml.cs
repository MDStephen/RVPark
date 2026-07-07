using asp_net_web_app.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class AdminManageStaffModel : PageModel
{
    private readonly EmployeeLogic _logic;

    public AdminManageStaffModel(EmployeeLogic logic)
    {
        _logic = logic;
    }

    // ---------- state the page reads ----------
    public List<Employee> Employees { get; set; } = new();
    public Employee? Selected { get; set; }
    public string Mode { get; set; } = "edit";      // "edit" or "create"
    public string StatusMessage { get; set; } = "";

    // ---------- GET: load the page ----------
    // /AdminManageStaff                -> edit mode, nothing selected
    // /AdminManageStaff?mode=create    -> create mode
    // /AdminManageStaff?id=3           -> edit mode, employee 3 selected
    public void OnGet(string? mode, int? id, string? status)
    {
        Mode = mode == "create" ? "create" : "edit";
        Employees = _logic.GetAllEmployees();

        if (Mode == "edit" && id != null)
        {
            Selected = _logic.GetEmployee(id.Value);
        }

        StatusMessage = status ?? "";
    }

    // ---------- POST: Create User button ----------
    public IActionResult OnPostCreate(string firstName, string lastName, DateTime dateOfBirth, string role)
    {
        var result = _logic.CreateEmployee(firstName, lastName, dateOfBirth, role);

        if (result == "success")
        {
            return RedirectToPage(new { mode = "edit", status = "Staff member created successfully." });
        }

        // validation failed - stay in create mode and show the reason
        return RedirectToPage(new { mode = "create", status = "Could not create: " + result });
    }

    // ---------- POST: Save Changes button ----------
    public IActionResult OnPostSave(int id, string firstName, string lastName, DateTime dateOfBirth, string role)
    {
        var result = _logic.UpdateEmployee(id, firstName, lastName, dateOfBirth, role);

        var message = result == "success" ? "Changes saved." : "Could not save: " + result;
        return RedirectToPage(new { mode = "edit", id, status = message });
    }

    // ---------- POST: Lock User button ----------
    public IActionResult OnPostLock(int id)
    {
        _logic.SetLock(id, true);
        return RedirectToPage(new { mode = "edit", id, status = "User locked. They can no longer access the system." });
    }

    // ---------- POST: Unlock User button ----------
    public IActionResult OnPostUnlock(int id)
    {
        _logic.SetLock(id, false);
        return RedirectToPage(new { mode = "edit", id, status = "User unlocked. Access restored." });
    }

    // ---------- POST: Delete User button ----------
    public IActionResult OnPostDelete(int id)
    {
        _logic.DeleteEmployee(id);
        return RedirectToPage(new { mode = "edit", status = "Staff member deleted." });
    }
}
