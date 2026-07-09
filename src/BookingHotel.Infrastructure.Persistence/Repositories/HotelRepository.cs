using BookingHotel.Domain.Hotels;
using BookingHotel.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BookingHotel.Infrastructure.Persistence.Repositories;

public class HotelRepository(
  AppDbContext context
    ) : IHotelRepository
{
  private readonly AppDbContext _context = context;

  public void Add(Hotel hotel, CancellationToken cancellationToken = default)
  {
    _context.Hotels.Add(hotel);
  }

  public Task<Hotel?> GetById(HotelId id, CancellationToken cancellationToken = default)
  {
    return _context.Hotels
      .Include(x => x.Rooms)
      .Include(x => x.RoomTypes)
      .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
  }

  public Task<bool> HasRoomNumberAsync(HotelId hotelId, string roomNumber, CancellationToken cancellationToken = default)
  {
    throw new NotImplementedException();
  }

  public void Update(Hotel hotel)
  {
    _context.Hotels.Update(hotel);
  }
}
