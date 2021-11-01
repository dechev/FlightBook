using FlightBook.DomainModel;
using Microsoft.EntityFrameworkCore;

namespace FlightBook.Persistence.EFCore
{
    public class FlightBookContext : DbContext
    {
        public DbSet<Flight> Flights { get; set; }
        public DbSet<Passenger> Passengers { get; set; }
        public DbSet<FlightRegistration> FlightRegistrations { get; set; }
        public DbSet<LuggagePiece> LuggagePieces { get; set; }

        public FlightBookContext(DbContextOptions<FlightBookContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Flight>()
                .Property(f => f.FlightTotalLuggageWeightLimit)
                .HasPrecision(10, 3);

            modelBuilder.Entity<Flight>()
                .Property(f => f.PerPassengerLuggageWeightLimit)
                .HasPrecision(10, 3);

            modelBuilder.Entity<LuggagePiece>()
                .Property(f => f.WeightInKg)
                .HasPrecision(10, 3);

            modelBuilder.Entity<Flight>()
                .HasMany(x => x.LuggagePieces)
                .WithOne(x => x.Flight)
                .OnDelete(DeleteBehavior.NoAction);

        }
    }
}