using System;
using KonferenscentrumVast.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Cosmos;


namespace KonferenscentrumVast.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<Facility> Facilities { get; set; } = null!;
        public DbSet<Booking> Bookings { get; set; } = null!;
        public DbSet<BookingContract> BookingContracts { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Customer -> Customer-container, PK /id
            modelBuilder.Entity<Customer>(b =>
            {
                b.ToContainer("Customer-container");
                b.HasKey(x => x.Id);

                b.Property(x => x.Id)
                .ToJsonProperty("id")      // Cosmos needs the field literally named "id"
                .HasConversion<string>();  // int <-> string for Cosmos JSON

                b.HasPartitionKey(x => x.Id); // your containers use /id
                b.Ignore(x => x.Bookings); 
            });

            // Facility -> Facility-container
            modelBuilder.Entity<Facility>(b =>
            {
                b.ToContainer("Facility-container");
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).ToJsonProperty("id").HasConversion<string>();
                b.HasPartitionKey(x => x.Id);
                 b.Ignore(x => x.Bookings); 
            });

            // Booking -> Booking-container
            modelBuilder.Entity<Booking>(b =>
            {
                b.ToContainer("Booking-container");
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).ToJsonProperty("id").HasConversion<string>();
                b.HasPartitionKey(x => x.Id);

                // Cosmos doesn’t support relational Includes; ignore navs
                b.Ignore(x => x.Customer);
                b.Ignore(x => x.Facility);
                b.Ignore(x => x.Contract);
            });

            // BookingContract -> BookingContract-container
            modelBuilder.Entity<BookingContract>(b =>
            {
                b.ToContainer("BookingContract-container");
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).ToJsonProperty("id").HasConversion<string>();
                b.HasPartitionKey(x => x.Id);
            });
        }
    }
}