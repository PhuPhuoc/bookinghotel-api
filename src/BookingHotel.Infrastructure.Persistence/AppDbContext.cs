using BookingHotel.Domain.Bookings;
using BookingHotel.Domain.Hotels;
using Microsoft.EntityFrameworkCore;

namespace BookingHotel.Infrastructure.Persistence;


public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
  public DbSet<Booking> Bookings { get; set; }
  public DbSet<Hotel> Hotels { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    // auto pick up every IEntityTypeConfiguration in this assembly
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
  }
}

