using Microsoft.EntityFrameworkCore;

namespace asp_net_web_app.Data
{
    public class DatabaseWrapper : DbContext
    {
        public DatabaseWrapper(DbContextOptions<DatabaseWrapper> options) : base(options)
        {
        }

        public DbSet<Users>         Users           { get; set; }
        public DbSet<Reservations>  Reservations    { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Users>().ToTable("Users");
            modelBuilder.Entity<Reservations>().ToTable("Reservations"); 
        }

        // This is test code, TODO remove
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


        public void AddReservation(string s)
        {
            Reservations.Add(new Reservations
            {
                startDate   = s,
                endDate     = s,
                status      = s,
                totalCost   = 0,
                isEligible  = false
            });
            SaveChanges();
        }
    }
}
