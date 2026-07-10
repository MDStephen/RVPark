using System;
using Microsoft.EntityFrameworkCore;

namespace asp_net_web_app.Data
{
    public class DatabaseWrapper : DbContext
    {
        public DatabaseWrapper(DbContextOptions<DatabaseWrapper> options) : base(options)
        {
        }

        // Users, Customer, Staff, Admin all in users now
        public DbSet<Users> Users { get; set; }
        public DbSet<Employee> Employees { get; set; }

        public DbSet<DbSite> Sites { get; set; }
        public DbSet<DbSitePhoto> SitePhotos { get; set; }
        public DbSet<DbSitePrice> SitePrices { get; set; }
        public DbSet<Reservations> Reservations { get; set; }

        // NOTE: renamed from sites - temporary collision fix
        public DbSet<Site> SiteModels { get; set; }

        // NOTE: renamed from reservations - temporary collision fix
        public DbSet<Reservation> ReservationModels { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Pricing> Pricing { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Users>().ToTable("Users");

            // NOTE: renamed from sites - temporary collision fix
            modelBuilder.Entity<Site>().ToTable("SiteModels");
            modelBuilder.Entity<Lot>();
            modelBuilder.Entity<StorageContainer>();

            // NOTE: renamed from sites - temporary collision fix
            modelBuilder.Entity<Reservation>().ToTable("ReservationModels");
            modelBuilder.Entity<Payment>().ToTable("Payments");
            modelBuilder.Entity<Pricing>().ToTable("Pricing");
            modelBuilder.Entity<Employee>().ToTable("Employees");

            modelBuilder.Entity<DbSite>().ToTable("Sites");
            modelBuilder.Entity<DbSitePhoto>().ToTable("SitePhotos");
            modelBuilder.Entity<DbSitePrice>().ToTable("SitePrices");
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

    public class Reservations
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int SiteId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "Upcoming";
        public decimal TotalCost { get; set; }
    }
}
