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
    public string StatusMessage { get; set; } = "";

    // ---------- GET: load the page ----------
    // /AdminManageStaff        -> list, nothing selected
    // /AdminManageStaff?id=3   -> employee 3 selected for editing
    public void OnGet(int? id, string? status)
    {
        Employees = _logic.GetAllEmployees();

        if (id != null)
        {
            Selected = _logic.GetEmployee(id.Value);
        }

        StatusMessage = status ?? "";
    }

    // ---------- POST: Save Changes button ----------
    public IActionResult OnPostSave(int id, string firstName, string lastName, DateTime dateOfBirth, string role)
    {
        var result = _logic.UpdateEmployee(id, firstName, lastName, dateOfBirth, role);

        var message = result == "success" ? "Changes saved." : "Could not save: " + result;
        return RedirectToPage(new { id, status = message });
    }

    // ---------- POST: Lock User button ----------
    public IActionResult OnPostLock(int id)
    {
        _logic.SetLock(id, true);
        return RedirectToPage(new { id, status = "User locked. They can no longer access the system." });
    }

    // ---------- POST: Unlock User button ----------
    public IActionResult OnPostUnlock(int id)
    {
        _logic.SetLock(id, false);
        return RedirectToPage(new { id, status = "User unlocked. Access restored." });
    }

    // ---------- POST: Delete User button ----------
    public IActionResult OnPostDelete(int id)
    {
        _logic.DeleteEmployee(id);
        return RedirectToPage(new { status = "Staff member deleted." });
    }
}