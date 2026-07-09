using BookingHotel.Domain.Common;
using ErrorOr;

namespace BookingHotel.Domain.Hotels;

public sealed class Room : Entity<RoomId>
{
  public HotelId HotelId { get; private set; }

  public RoomTypeId RoomTypeId { get; private set; }

  public string RoomNumber { get; private set; } = "";

  public int Floor { get; private set; }

  public RoomStatus Status { get; private set; } = RoomStatus.Available;

  public DateTime CreatedAt { get; private set; }

  public DateTime UpdatedAt { get; private set; }

  private Room()
  {
    // Required by EF Core
  }

  private Room(
      RoomId id,
      HotelId hotelId,
      RoomTypeId roomTypeId,
      string roomNumber,
      int floor,
      RoomStatus status,
      DateTime createdAt,
      DateTime updatedAt)
  {
    Id = id;
    HotelId = hotelId;
    RoomTypeId = roomTypeId;
    RoomNumber = roomNumber;
    Floor = floor;
    Status = status;
    CreatedAt = createdAt;
    UpdatedAt = updatedAt;
  }

  internal static ErrorOr<Room> Create(HotelId hotelId, RoomTypeId roomTypeId, string roomNumber, int floor)
  {
    if (string.IsNullOrWhiteSpace(roomNumber)) return HotelErrors.InvalidRoomNumber;

    var now = DateTime.UtcNow;
    return new Room(
        RoomId.New(),
        hotelId,
        roomTypeId,
        roomNumber,
        floor,
        RoomStatus.Maintenance,
        now,
        now);
  }

  internal ErrorOr<Success> UpdateStatus(RoomStatus status)
  {
    if (!Enum.IsDefined(typeof(RoomStatus), status))
    {
      return HotelErrors.InvalidRoomStatus;
    }

    Status = status;
    UpdatedAt = DateTime.UtcNow;

    return Result.Success;
  }
}
