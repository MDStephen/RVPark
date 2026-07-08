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
            _db.Users.Update(user);
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
