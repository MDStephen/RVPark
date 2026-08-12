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

    // True when Selected is a blank, not-yet-saved employee (id=0 sentinel)
    public bool IsNew { get; set; }

    // ---------- GET: load the page ----------
    // /AdminManageStaff        -> list, nothing selected
    // /AdminManageStaff?id=3   -> employee 3 selected for editing
    // /AdminManageStaff?id=0   -> blank, editable "create new" form
    public void OnGet(int? id, string? status)
    {
        Employees = _logic.GetAllEmployees();

        if (id == 0)
        {
            Selected = new Employee { role = "Employee" };
            IsNew = true;
        }
        else if (id != null)
        {
            Selected = _logic.GetEmployee(id.Value);
        }
        else if (Employees.Any())
        {
            // Auto-select the first employee so the panel is never blank
            Selected = Employees.First();
        }

        StatusMessage = status ?? "";
    }

    // ---------- POST: Save Changes button ----------
    public IActionResult OnPostSave(int id, string firstName, string lastName, DateTime dateOfBirth, string role, string? username, string? password)
    {
        string result;
        int redirectId = id;

        if (id == 0)
        {
            result = _logic.CreateEmployeeWithCredentials(firstName, lastName, dateOfBirth, role, username, password, out int newId);
            redirectId = newId;
        }
        else
        {
            result = _logic.UpdateEmployee(id, firstName, lastName, dateOfBirth, role);
        }

        var message = result == "success" ? "Changes saved." : "Could not save: " + result;
        return RedirectToPage(new { id = redirectId, status = message });
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
        if (id == 0)
        {
            // Nothing was ever saved - just discard the blank "create" form.
            return RedirectToPage(new { status = "New staff member discarded." });
        }

        _logic.DeleteEmployee(id);
        return RedirectToPage(new { status = "Staff member deleted." });
    }
}