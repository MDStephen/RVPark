using Microsoft.EntityFrameworkCore;
using asp_net_web_app.Data;

namespace asp_net_web_app.Data
{
    public class DatabaseWrapper : DbContext
    {
        public DatabaseWrapper(DbContextOptions<DatabaseWrapper> options) : base(options)
        {
        }

        public DbSet<Users> Users { get; set; }

        public DbSet<Employee> Employees { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Users>().ToTable("Users"); 
        }

        public void AddUser(string username)
        {
            Users.Add(new Users
            {
                firstName = username,
                lastName = "",
                emailAddress = "",
                phoneNumber = "",
                address = ""
            });
            SaveChanges();
        }
    }
}