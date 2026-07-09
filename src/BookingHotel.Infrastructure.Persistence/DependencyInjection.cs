using BookingHotel.Domain.Repositories;
using BookingHotel.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wolverine.EntityFrameworkCore;

namespace BookingHotel.Infrastructure.Persistence;

public static class DependencyInjection
{
  public static IServiceCollection AddPersistence(
      this IServiceCollection services,
      IConfiguration configuration)
  {

    var connectionString = configuration.GetConnectionString("DefaultConnection") ?? "Host=localhost;Port=5432;Database=booking_hotel;Username=postgres;Password=postgres";

    // services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString)); // đã thay bằng wolverine ở dưới rồi
    services.AddDbContextWithWolverineIntegration<AppDbContext>(options =>
     {
       options.UseNpgsql(connectionString);
     });

    services.AddScoped<IHotelRepository, HotelRepository>();

    return services;
  }
}
