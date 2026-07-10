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

        // Site, Lot, StorageContainer all in sites now
        public DbSet<Site> Sites { get; set; }

        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Pricing> Pricing { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Users>().ToTable("Users");
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Staff>();
            modelBuilder.Entity<Admin>();
 
            modelBuilder.Entity<Site>().ToTable("Sites");
            modelBuilder.Entity<Lot>();
            modelBuilder.Entity<StorageContainer>();
            modelBuilder.Entity<Reservation>().ToTable("Reservations");
            modelBuilder.Entity<Payment>().ToTable("Payments");
            modelBuilder.Entity<Pricing>().ToTable("Pricing");
        }
/*
        // updated to match new schma
        public void AddUser(string firstName, string lastName)
        {
            Users.Add(new Customer
            {
                firstName = firstName,
                lastName = lastName,
                email = "",
                phoneNumber = "",
                streetAddress = "",
                city = "",
                state = "",
                zip = "",
                isBanned = false
            });
            SaveChanges();
        }

        public void AddReservation(DateTime start, DateTime end, string status)
        {
            Reservations.Add(new Reservation
            {
                startDate = start,
                endDate = end,
                status = status,
                totalCost = 0,
                isEligible = false
            });
            SaveChanges();
        }
*/
    }
}
