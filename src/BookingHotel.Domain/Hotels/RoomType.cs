using BookingHotel.Domain.Common;
using ErrorOr;

namespace BookingHotel.Domain.Hotels;

public sealed class RoomType : Entity<RoomTypeId>
{
  public HotelId HotelId { get; private set; }

  public string Name { get; private set; } = "";

  public string Description { get; private set; } = "";

  public decimal BasePrice { get; private set; }

  public int MaxAdults { get; private set; }

  public int MaxChildren { get; private set; }

  public DateTime CreatedAt { get; private set; }

  public DateTime UpdatedAt { get; private set; }

  private RoomType()
  {
    // Required by EF Core
  }

  private RoomType(
      RoomTypeId id,
      HotelId hotelId,
      string name,
      string description,
      decimal basePrice,
      int maxAdults,
      int maxChildren,
      DateTime createdAt,
      DateTime updatedAt)
  {
    Id = id;
    HotelId = hotelId;
    Name = name;
    Description = description;
    BasePrice = basePrice;
    MaxAdults = maxAdults;
    MaxChildren = maxChildren;
    CreatedAt = createdAt;
    UpdatedAt = updatedAt;
  }

  internal static ErrorOr<RoomType> Create(
      HotelId hotelId,
      string name,
      string description,
      decimal basePrice,
      int maxAdults,
      int maxChildren)
  {
    if (string.IsNullOrWhiteSpace(name)) return HotelErrors.InvalidRoomTypeName;
    if (basePrice < 0) return HotelErrors.InvalidPrice;
    if (maxAdults < 1) return HotelErrors.InvalidCapacity;

    var now = DateTime.UtcNow;
    return new RoomType(
        RoomTypeId.New(),
        hotelId,
        name,
        description,
        basePrice,
        maxAdults,
        maxChildren,
        now,
        now);
  }

  internal ErrorOr<Success> Update(
      string name,
      string description,
      decimal basePrice,
      int maxAdults,
      int maxChildren)
  {
    if (string.IsNullOrWhiteSpace(name)) return HotelErrors.InvalidRoomTypeName;
    if (basePrice < 0) return HotelErrors.InvalidPrice;
    if (maxAdults < 1) return HotelErrors.InvalidCapacity;

    Name = name;
    Description = description;
    BasePrice = basePrice;
    MaxAdults = maxAdults;
    MaxChildren = maxChildren;
    UpdatedAt = DateTime.UtcNow;

    return Result.Success;
  }
}
