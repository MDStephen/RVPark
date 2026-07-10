using asp_net_web_app.Data;

public class EmployeeLogic
{
    private readonly DatabaseWrapper _db;

    public EmployeeLogic(DatabaseWrapper db)
    {
        _db = db;
    }

    // list all staff for the left panel
    public List<Employee> GetAllEmployees()
    {
        return _db.Employees.OrderBy(e => e.lastName).ThenBy(e => e.firstName).ToList();
    }

    public Employee? GetEmployee(int id)
    {
        return _db.Employees.Find(id);
    }

    // Create Staff (mockup 2)
    public string CreateEmployee(string firstName, string lastName, DateTime dateOfBirth, string role)
    {
        if (string.IsNullOrWhiteSpace(firstName)) return "first name required";
        if (string.IsNullOrWhiteSpace(lastName)) return "last name required";
        if (string.IsNullOrWhiteSpace(role)) return "role required";
        if (dateOfBirth == default) return "date of birth required";

        firstName = firstName.Trim();
        lastName = lastName.Trim();

        var employee = new Employee
        {
            firstName = firstName,
            lastName = lastName,
            dateOfBirth = dateOfBirth,
            // team convention: username = first initial + last name
            username = (firstName.Substring(0, 1) + lastName).ToLower().Replace(" ", ""),
            role = role,
            isLocked = false,
            password = "" // filled in after save, once the ID exists
        };

        _db.Employees.Add(employee);
        _db.SaveChanges();

        // team convention: initial password = employee ID
        employee.password = employee.employeeId.ToString();
        _db.SaveChanges();

        return "success";
    }

    // Save Changes (mockup 1)
    public string UpdateEmployee(int id, string firstName, string lastName, DateTime dateOfBirth, string role)
    {
        var employee = _db.Employees.Find(id);
        if (employee == null) return "not found";

        if (string.IsNullOrWhiteSpace(firstName)) return "first name required";
        if (string.IsNullOrWhiteSpace(lastName)) return "last name required";
        if (string.IsNullOrWhiteSpace(role)) return "role required";

        employee.firstName = firstName.Trim();
        employee.lastName = lastName.Trim();
        employee.dateOfBirth = dateOfBirth;
        employee.role = role;

        _db.SaveChanges();
        return "success";
    }

    // Lock / Unlock — the graded "lock employees out" requirement
    public string SetLock(int id, bool locked)
    {
        var employee = _db.Employees.Find(id);
        if (employee == null) return "not found";

        employee.isLocked = locked;
        _db.SaveChanges();
        return "success";
    }

    // Delete User
    public string DeleteEmployee(int id)
    {
        var employee = _db.Employees.Find(id);
        if (employee == null) return "not found";

        _db.Employees.Remove(employee);
        _db.SaveChanges();
        return "success";
    }
}