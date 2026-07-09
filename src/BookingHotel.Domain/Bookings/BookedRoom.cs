using BookingHotel.Domain.Common;
using ErrorOr;

namespace BookingHotel.Domain.Bookings;

public sealed class BookedRoom : Entity<BookedRoomId>
{
  private readonly List<GuestStay> _guestStays = [];

  public string RoomId { get; private set; } = "";

  public string RoomNumberSnapshot { get; private set; } = "";

  public string RoomTypeSnapshot { get; private set; } = "";

  public decimal PricePerNightSnapshot { get; private set; } = 0;

  public int MaxCapacitySnapshot { get; private set; } = 0;

  public DateTime CheckInDate { get; private set; }

  public DateTime CheckOutDate { get; private set; }

  public DateTime CreatedAt { get; private set; }

  public DateTime UpdatedAt { get; private set; }

  public IReadOnlyCollection<GuestStay> GuestStays => _guestStays.AsReadOnly();

  private BookedRoom()
  {
    // Required by EF Core.
  }

  private BookedRoom(
      BookedRoomId id,
      string roomId,
      string roomNumberSnapshot,
      string roomTypeSnapshot,
      decimal pricePerNightSnapshot,
      int maxCapacitySnapshot,
      DateTime checkInDate,
      DateTime checkOutDate,
      DateTime createdAt,
      DateTime updatedAt)
  {
    Id = id;
    RoomId = roomId;
    RoomNumberSnapshot = roomNumberSnapshot;
    RoomTypeSnapshot = roomTypeSnapshot;
    PricePerNightSnapshot = pricePerNightSnapshot;
    MaxCapacitySnapshot = maxCapacitySnapshot;
    CheckInDate = checkInDate;
    CheckOutDate = checkOutDate;
    CreatedAt = createdAt;
    UpdatedAt = updatedAt;
  }

  internal static ErrorOr<BookedRoom> Create(
      string roomId,
      string roomNumberSnapshot,
      string roomTypeSnapshot,
      decimal pricePerNightSnapshot,
      int maxCapacitySnapshot,
      DateTime checkInDate,
      DateTime checkOutDate)
  {
    if (string.IsNullOrWhiteSpace(roomId)) return BookingErrors.RoomNotFound;

    if (maxCapacitySnapshot <= 0) return BookingErrors.RoomCapacityExceeded;

    var now = DateTime.UtcNow;
    return new BookedRoom(
        BookedRoomId.New(),
        roomId,
        roomNumberSnapshot,
        roomTypeSnapshot,
        pricePerNightSnapshot,
        maxCapacitySnapshot,
        checkInDate,
        checkOutDate,
        now,
        now);
  }

  internal ErrorOr<Success> UpdateSnapshot(
      string newRoomId,
      string roomNumberSnapshot,
      string roomTypeSnapshot,
      decimal pricePerNightSnapshot,
      int maxCapacitySnapshot)
  {
    if (_guestStays.Count > maxCapacitySnapshot)
    {
      return BookingErrors.RoomCapacityExceeded;
    }

    RoomId = newRoomId;
    RoomNumberSnapshot = roomNumberSnapshot;
    RoomTypeSnapshot = roomTypeSnapshot;
    PricePerNightSnapshot = pricePerNightSnapshot;
    MaxCapacitySnapshot = maxCapacitySnapshot;
    UpdatedAt = DateTime.UtcNow;

    return Result.Success;
  }

  internal void UpdateDates(DateTime checkInDate, DateTime checkOutDate)
  {
    CheckInDate = checkInDate;
    CheckOutDate = checkOutDate;
    UpdatedAt = DateTime.UtcNow;
  }

  internal ErrorOr<Success> AddGuest(string fullName, string idNumber, bool isAdult, DateTime dob, string nationality)
  {
    if (_guestStays.Count >= MaxCapacitySnapshot)
    {
      return BookingErrors.RoomCapacityExceeded;
    }

    if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(idNumber))
    {
      return BookingErrors.InvalidGuestInformation;
    }

    if (_guestStays.Any(g => g.IdNumber == idNumber))
    {
      return BookingErrors.GuestAlreadyExists;
    }

    var guest = GuestStay.Create(fullName, idNumber, isAdult, dob, nationality);
    _guestStays.Add(guest);
    UpdatedAt = DateTime.UtcNow;

    return Result.Success;
  }

  internal ErrorOr<Success> RemoveGuest(GuestStayId guestId)
  {
    var guest = _guestStays.FirstOrDefault(g => g.Id == guestId);
    if (guest is null) return BookingErrors.GuestNotFound;

    _guestStays.Remove(guest);
    UpdatedAt = DateTime.UtcNow;

    return Result.Success;
  }

  internal ErrorOr<Success> UpdateGuest(GuestStayId guestId, string fullName, string idNumber, bool isAdult, DateTime dob, string nationality)
  {
    var guest = _guestStays.FirstOrDefault(g => g.Id == guestId);
    if (guest is null) return BookingErrors.GuestNotFound;

    if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(idNumber))
    {
      return BookingErrors.InvalidGuestInformation;
    }

    if (_guestStays.Any(g => g.IdNumber == idNumber && g.Id != guestId))
    {
      return BookingErrors.GuestAlreadyExists;
    }

    guest.Update(fullName, idNumber, isAdult, dob, nationality);
    UpdatedAt = DateTime.UtcNow;

    return Result.Success;
  }
}
