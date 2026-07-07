using Microsoft.EntityFrameworkCore;

namespace asp_net_web_app.Data
{
    public class DatabaseWrapper : DbContext
    {
        public DatabaseWrapper(DbContextOptions<DatabaseWrapper> options) : base(options)
        {
        }

        public DbSet<Users> Users { get; set; }
        public DbSet<Site> Sites { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Users>().ToTable("Users");
            modelBuilder.Entity<Site>().ToTable("Sites");
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