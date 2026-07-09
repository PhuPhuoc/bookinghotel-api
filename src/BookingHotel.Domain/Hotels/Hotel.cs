using BookingHotel.Domain.Common;
using ErrorOr;

namespace BookingHotel.Domain.Hotels;

public sealed class Hotel : AggregateRoot<HotelId>
{
  private readonly List<RoomType> _roomTypes = [];
  private readonly List<Room> _rooms = [];

  public string Name { get; private set; } = "";

  public string Address { get; private set; } = "";

  public string Phone { get; private set; } = "";

  public string Email { get; private set; } = "";

  public string Description { get; private set; } = "";

  public byte StarRating { get; private set; }

  public DateTime CreatedAt { get; private set; }

  public DateTime UpdatedAt { get; private set; }

  public IReadOnlyCollection<RoomType> RoomTypes => _roomTypes.AsReadOnly();

  public IReadOnlyCollection<Room> Rooms => _rooms.AsReadOnly();

  private Hotel()
  {
    // Required by EF Core
  }

  private Hotel(
      HotelId id,
      string name,
      string address,
      string phone,
      string email,
      string description,
      byte starRating,
      DateTime createdAt,
      DateTime updatedAt)
  {
    Id = id;
    Name = name;
    Address = address;
    Phone = phone;
    Email = email;
    Description = description;
    StarRating = starRating;
    CreatedAt = createdAt;
    UpdatedAt = updatedAt;
  }

  #region Factory
  public static ErrorOr<Hotel> Create(
      string name,
      string address,
      string phone,
      string email,
      string description,
      byte starRating)
  {
    if (string.IsNullOrWhiteSpace(name)) return HotelErrors.InvalidName;
    if (starRating < 1 || starRating > 5) return HotelErrors.InvalidRating;

    var now = DateTime.UtcNow;
    return new Hotel(
        HotelId.New(),
        name,
        address,
        phone,
        email,
        description,
        starRating,
        now,
        now);
  }
  #endregion

  #region RoomType Management
  public ErrorOr<Success> AddRoomType(string name, string description, decimal basePrice, int maxAdults, int maxChildren)
  {
    if (_roomTypes.Any(rt => rt.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
    {
      return HotelErrors.RoomTypeAlreadyExists;
    }

    var roomTypeResult = RoomType.Create(Id, name, description, basePrice, maxAdults, maxChildren);
    if (roomTypeResult.IsError) return roomTypeResult.Errors;

    _roomTypes.Add(roomTypeResult.Value);
    UpdateTimestamp();

    return Result.Success;
  }

  public ErrorOr<Success> UpdateRoomType(RoomTypeId roomTypeId, string name, string description, decimal basePrice, int maxAdults, int maxChildren)
  {
    var roomType = _roomTypes.FirstOrDefault(rt => rt.Id == roomTypeId);
    if (roomType is null) return HotelErrors.RoomTypeNotFound;

    if (_roomTypes.Any(rt => rt.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && rt.Id != roomTypeId))
    {
      return HotelErrors.RoomTypeAlreadyExists;
    }

    var updateResult = roomType.Update(name, description, basePrice, maxAdults, maxChildren);
    if (updateResult.IsError) return updateResult.Errors;

    UpdateTimestamp();
    return Result.Success;
  }
  #endregion

  #region Room Management
  public ErrorOr<Success> AddRoom(RoomTypeId roomTypeId, string roomNumber, int floor)
  {
    if (!_roomTypes.Any(rt => rt.Id == roomTypeId))
    {
      return HotelErrors.RoomTypeNotFound;
    }

    if (_rooms.Any(r => r.RoomNumber.Equals(roomNumber, StringComparison.OrdinalIgnoreCase)))
    {
      return HotelErrors.RoomNumberAlreadyExists;
    }

    var roomResult = Room.Create(Id, roomTypeId, roomNumber, floor);
    if (roomResult.IsError) return roomResult.Errors;

    _rooms.Add(roomResult.Value);
    UpdateTimestamp();

    return Result.Success;
  }

  public ErrorOr<Success> UpdateRoomStatus(RoomId roomId, RoomStatus status)
  {
    var room = _rooms.FirstOrDefault(r => r.Id == roomId);
    if (room is null) return HotelErrors.RoomNotFound;

    var updateResult = room.UpdateStatus(status);
    if (updateResult.IsError) return updateResult.Errors;

    UpdateTimestamp();
    return Result.Success;
  }
  #endregion

  private void UpdateTimestamp()
  {
    UpdatedAt = DateTime.UtcNow;
  }
}
