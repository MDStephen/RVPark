using asp_net_web_app.Data;

namespace asp_net_web_app.Repositories
{
    public interface IUserRepository
    {
        Task<List<Users>> GetAllCustomersAsync();
        Task<Users?> GetByIdAsync(int id);
        Task<Users?> GetByEmailAsync(string email);
        Task<List<Users>> SearchAsync(string query);
        Task<Users> CreateAsync(Users user);
        Task UpdateAsync(Users user);
        Task DeleteAsync(int id);
    }
}
