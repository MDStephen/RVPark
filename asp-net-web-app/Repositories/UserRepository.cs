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

        public async Task<Users?> GetByEmailAsync(string email)
        {
            var all = await _db.Users.ToListAsync();
            return all.FirstOrDefault(u => string.Equals(u.emailAddress, email, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<List<Users>> SearchAsync(string query)
        {
            var q = query.Trim();
            var all = await _db.Users.ToListAsync();
            return all.Where(u =>
                    $"{u.firstName} {u.lastName}".Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    (u.emailAddress != null && u.emailAddress.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                    (u.phoneNumber != null && u.phoneNumber.Contains(q, StringComparison.OrdinalIgnoreCase)))
                .OrderBy(u => u.lastName)
                .ThenBy(u => u.firstName)
                .ToList();
        }

        public async Task<Users> CreateAsync(Users user)
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
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
    }
}
