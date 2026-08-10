using System.ComponentModel.DataAnnotations;

namespace asp_net_web_app.Data
{
    // Holds login credentials for a customer.
    // Kept in its own table so the sensitive password data stays
    // separate from the regular profile data in the Users table.
    public class UserAccount
    {
        [Key]
        public int Id { get; set; }

        // Points back to the matching row in the Users table.
        // (Same "link two tables by an Id" idea Reservations uses.)
        [Required]
        public int UserId { get; set; }

        // The login identifier. For now we store the email here,
        // so email == username. A real username can slot in later
        // without breaking anything.
        [Required]
        public string Username { get; set; } = string.Empty;

        // NEVER the raw password. Only the scrambled (hashed) version.
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        // Email verification. Starts false; flips to true once the
        // customer clicks the link in their confirmation email.
        public bool IsEmailVerified { get; set; } = false;

        // The random token we put in that confirmation link.
        // Nullable because it's cleared/unused once verified.
        public string? EmailVerificationToken { get; set; }
    }
}
