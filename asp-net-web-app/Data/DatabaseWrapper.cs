using Microsoft.EntityFrameworkCore;

namespace asp_net_web_app.Data
{
    public class DatabaseWrapper : DbContext
    {
        public DatabaseWrapper(DbContextOptions<DatabaseWrapper> options) : base(options)
        {
        }

        public DbSet<Users> Users { get; set; }
        
        public DbSet<DbSite> Sites { get; set; }
        public DbSet<DbSitePhoto> SitePhotos { get; set; }
        public DbSet<DbSitePrice> SitePrices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Users>().ToTable("Users");
            
            modelBuilder.Entity<DbSite>().ToTable("Sites");
            modelBuilder.Entity<DbSitePhoto>().ToTable("SitePhotos");
            modelBuilder.Entity<DbSitePrice>().ToTable("SitePrices");
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

    public class DbSite
    {
        public int Id { get; set; }
        public string SiteNumber { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsAvailable { get; set; } = true;
        
        public List<DbSitePhoto> Photos { get; set; } = new();
        public List<DbSitePrice> PriceRules { get; set; } = new();
    }

    public class DbSitePhoto
    {
        public int Id { get; set; }
        public int DbSiteId { get; set; }
        public string PhotoUrl { get; set; } = string.Empty;
    }

    public class DbSitePrice
    {
        public int Id { get; set; }
        public int DbSiteId { get; set; }
        public decimal Cost { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}