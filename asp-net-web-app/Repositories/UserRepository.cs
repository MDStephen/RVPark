using asp_net_web_app.Data;
using Microsoft.EntityFrameworkCore;

namespace asp_net_web_app.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DatabaseWrapper _db;

        public UserRepository(DatabaseWrapper db)
        {
            _db = db;
        }

        public async Task<List<Users>> GetAllCustomersAsync()
        {
            return await _db.Users
                .OrderBy(u => u.lastName)
                .ThenBy(u => u.firstName)
                .ToListAsync();
        }

        public async Task<Users?> GetByIdAsync(int id)
        {
            return await _db.Users.FindAsync(id);
        }

        public async Task UpdateAsync(Users user)
        {
            var existing = await _db.Users.FindAsync(user.userId);
            if (existing == null) return;

            existing.firstName    = user.firstName;
            existing.lastName     = user.lastName;
            existing.emailAddress = user.emailAddress;
            existing.phoneNumber  = user.phoneNumber;
            existing.address      = user.address;
            existing.middleInitial = user.middleInitial;
            existing.aptSuite      = user.aptSuite;
            existing.LastModifiedAt = DateTime.Now;

            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user != null)
            {
                _db.Users.Remove(user);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<(bool Success, string Message, int UserId)> CreateAsync(Users user, string? username, string? password)
        {
            if (string.IsNullOrWhiteSpace(user.firstName)) return (false, "First name required", 0);
            if (string.IsNullOrWhiteSpace(user.lastName)) return (false, "Last name required", 0);
            if (string.IsNullOrWhiteSpace(username)) return (false, "Username required", 0);
            if (string.IsNullOrWhiteSpace(password)) return (false, "Password required", 0);

            username = username.Trim();
            if (await _db.UserAccounts.AnyAsync(a => a.Username == username))
                return (false, "Username already taken", 0);

            // 1) Profile row, same as the public sign-up flow.
            var now = DateTime.Now;
            var customer = new Customer
            {
                firstName     = user.firstName.Trim(),
                lastName      = user.lastName.Trim(),
                middleInitial = user.middleInitial,
                emailAddress  = user.emailAddress,
                phoneNumber   = user.phoneNumber,
                address       = user.address,
                aptSuite      = user.aptSuite,
                CreatedAt     = now,
                LastModifiedAt = now
            };
            _db.Users.Add(customer);
            await _db.SaveChangesAsync();   // after this, customer.userId is filled in

            // 2) Matching login row. Only the BCrypt hash is stored, never the raw password.
            //    Admin-created accounts are considered verified immediately.
            var account = new UserAccount
            {
                UserId          = customer.userId,
                Username        = username,
                PasswordHash    = BCrypt.Net.BCrypt.HashPassword(password),
                IsEmailVerified = true
            };
            _db.UserAccounts.Add(account);
            await _db.SaveChangesAsync();

            return (true, "success", customer.userId);
        }
    }
}
