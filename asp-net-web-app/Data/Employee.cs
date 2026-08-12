using System.ComponentModel.DataAnnotations;

namespace asp_net_web_app.Data
{
    public class Employee
    {
        [Key]
        public int employeeId { get; set; }

        public string firstName { get; set; }
        public string lastName { get; set; }
        public DateTime dateOfBirth { get; set; }

        // team convention: first initial + last name, e.g. "jdoe"
        public string username { get; set; }

        // team convention: initial password = employee ID (prototype only)
        public string password { get; set; }

        // access level: "Admin" or "Employee"
        public string role { get; set; }

        // true = locked out of the system
        public bool isLocked { get; set; }

        // Set once, at creation, and never touched again.
        public DateTime CreatedAt { get; set; }

        // Bumped on every change to this record (edits, lock/unlock).
        public DateTime LastModifiedAt { get; set; }
    }
}
