using BookingHotel.Domain.Common;
using ErrorOr;

namespace BookingHotel.Domain.Bookings;

public sealed class Booking : AggregateRoot<BookingId>
{
  private readonly List<BookedRoom> _bookedRooms = [];

  public string CustomerId { get; private set; } = "";

  public DateTime CheckInDate { get; private set; }

  public DateTime CheckOutDate { get; private set; }

  public BookingStatus Status { get; private set; }

  public int TotalNight { get; private set; } = 0;

  public string Note { get; private set; } = "";

  public DateTime CreatedAt { get; private set; }

  public DateTime UpdatedAt { get; private set; }

  public IReadOnlyCollection<BookedRoom> BookedRooms => _bookedRooms.AsReadOnly();

  private Booking()
  {
    // Required by EF Core.
  }

  private Booking(
      BookingId id,
      string customerId,
      DateTime checkInDate,
      DateTime checkOutDate,
      int totalNight,
      string note,
      BookingStatus status,
      DateTime createdAt,
      DateTime updatedAt)
  {
    Id = id;
    CustomerId = customerId;
    CheckInDate = checkInDate;
    CheckOutDate = checkOutDate;
    TotalNight = totalNight;
    Note = note;
    Status = status;
    CreatedAt = createdAt;
    UpdatedAt = updatedAt;
  }

  #region Factory
  public static ErrorOr<Booking> Create(
      string customerId,
      DateTime checkInDate,
      DateTime checkOutDate,
      string? note = null)
  {
    if (string.IsNullOrWhiteSpace(customerId))
    {
      return BookingErrors.InvalidCustomer;
    }

    if (checkOutDate <= checkInDate)
    {
      return BookingErrors.InvalidStayPeriod;
    }

    int totalNights = (int)(checkOutDate - checkInDate).TotalDays;
    var now = DateTime.UtcNow;

    var booking = new Booking(
        BookingId.New(),
        customerId,
        checkInDate,
        checkOutDate,
        totalNights,
        note ?? "",
        BookingStatus.Pending,
        now,
        now);

    // raise Domain Event
    return booking;
  }
  #endregion

  #region Customer
  public ErrorOr<Success> ChangeCustomer(string customerId)
  {
    var editableResult = EnsureBookingEditable();
    if (editableResult.IsError) return editableResult.Errors;

    if (string.IsNullOrWhiteSpace(customerId))
    {
      return BookingErrors.InvalidCustomer;
    }

    CustomerId = customerId;
    UpdateTimestamp();

    return Result.Success;
  }
  #endregion

  #region Room
  public ErrorOr<Success> AddRoom(
      string roomId,
      string roomNumberSnapshot,
      string roomTypeSnapshot,
      decimal pricePerNightSnapshot,
      int maxCapacitySnapshot)
  {
    var editableResult = EnsureBookingEditable();
    if (editableResult.IsError) return editableResult.Errors;

    if (_bookedRooms.Any(r => r.RoomId == roomId))
    {
      return BookingErrors.RoomAlreadyAdded;
    }

    var bookedRoomResult = BookedRoom.Create(
        roomId,
        roomNumberSnapshot,
        roomTypeSnapshot,
        pricePerNightSnapshot,
        maxCapacitySnapshot,
        CheckInDate,
        CheckOutDate);

    if (bookedRoomResult.IsError) return bookedRoomResult.Errors;

    _bookedRooms.Add(bookedRoomResult.Value);

    Recalculate();
    UpdateTimestamp();

    return Result.Success;
  }

  public ErrorOr<Success> RemoveRoom(BookedRoomId bookedRoomId)
  {
    var editableResult = EnsureBookingEditable();
    if (editableResult.IsError) return editableResult.Errors;

    var roomResult = FindBookedRoom(bookedRoomId);
    if (roomResult.IsError) return roomResult.Errors;

    if (_bookedRooms.Count <= 1)
    {
      return BookingErrors.NoBookedRooms;
    }

    _bookedRooms.Remove(roomResult.Value);

    Recalculate();
    UpdateTimestamp();

    return Result.Success;
  }

  public ErrorOr<Success> ReplaceRoom(
      BookedRoomId bookedRoomId,
      string newRoomId,
      string roomNumberSnapshot,
      string roomTypeSnapshot,
      decimal pricePerNightSnapshot,
      int maxCapacitySnapshot)
  {
    var editableResult = EnsureBookingEditable();
    if (editableResult.IsError) return editableResult.Errors;

    var oldRoomResult = FindBookedRoom(bookedRoomId);
    if (oldRoomResult.IsError) return oldRoomResult.Errors;

    if (_bookedRooms.Any(r => r.RoomId == newRoomId && r.Id != bookedRoomId))
    {
      return BookingErrors.RoomAlreadyAdded;
    }

    var updateResult = oldRoomResult.Value.UpdateSnapshot(
        newRoomId,
        roomNumberSnapshot,
        roomTypeSnapshot,
        pricePerNightSnapshot,
        maxCapacitySnapshot);

    if (updateResult.IsError) return updateResult.Errors;

    Recalculate();
    UpdateTimestamp();

    return Result.Success;
  }
  #endregion

  #region Guest
  public ErrorOr<Success> AddGuest(
      BookedRoomId bookedRoomId,
      string fullName,
      string idNumber,
      bool isAdult,
      DateTime dob,
      string nationality)
  {
    // Khách hàng có thể thêm danh sách khách cho tới trước khi check-out (Pending/Confirmed)
    if (Status != BookingStatus.Pending && Status != BookingStatus.Confirmed)
    {
      return BookingErrors.CannotModify;
    }

    var roomResult = FindBookedRoom(bookedRoomId);
    if (roomResult.IsError) return roomResult.Errors;

    var addGuestResult = roomResult.Value.AddGuest(fullName, idNumber, isAdult, dob, nationality);
    if (addGuestResult.IsError) return addGuestResult.Errors;

    UpdateTimestamp();
    return Result.Success;
  }

  public ErrorOr<Success> RemoveGuest(BookedRoomId bookedRoomId, GuestStayId guestId)
  {
    if (Status != BookingStatus.Pending && Status != BookingStatus.Confirmed)
    {
      return BookingErrors.CannotModify;
    }

    var roomResult = FindBookedRoom(bookedRoomId);
    if (roomResult.IsError) return roomResult.Errors;

    var removeGuestResult = roomResult.Value.RemoveGuest(guestId);
    if (removeGuestResult.IsError) return removeGuestResult.Errors;

    UpdateTimestamp();
    return Result.Success;
  }

  public ErrorOr<Success> UpdateGuest(
      BookedRoomId bookedRoomId,
      GuestStayId guestId,
      string fullName,
      string idNumber,
      bool isAdult,
      DateTime dob,
      string nationality)
  {
    if (Status != BookingStatus.Pending && Status != BookingStatus.Confirmed)
    {
      return BookingErrors.CannotModify;
    }

    var roomResult = FindBookedRoom(bookedRoomId);
    if (roomResult.IsError) return roomResult.Errors;

    var updateGuestResult = roomResult.Value.UpdateGuest(guestId, fullName, idNumber, isAdult, dob, nationality);
    if (updateGuestResult.IsError) return updateGuestResult.Errors;

    UpdateTimestamp();
    return Result.Success;
  }
  #endregion

  #region Booking Trạng Thái (State Machine)
  public ErrorOr<Success> UpdateNote(string note)
  {
    if (note.Length > 1000)
    {
      return BookingErrors.NoteTooLong;
    }

    Note = note;
    UpdateTimestamp();
    return Result.Success;
  }

  public ErrorOr<Success> ChangeStayPeriod(DateTime checkInDate, DateTime checkOutDate)
  {
    var editableResult = EnsureBookingEditable();
    if (editableResult.IsError) return editableResult.Errors;

    if (checkOutDate <= checkInDate)
    {
      return BookingErrors.InvalidStayPeriod;
    }

    CheckInDate = checkInDate;
    CheckOutDate = checkOutDate;

    foreach (var room in _bookedRooms)
    {
      room.UpdateDates(checkInDate, checkOutDate);
    }

    Recalculate();
    UpdateTimestamp();

    return Result.Success;
  }

  public ErrorOr<Success> Confirm()
  {
    if (Status != BookingStatus.Pending) return BookingErrors.InvalidStatusTransition;
    if (_bookedRooms.Count == 0) return BookingErrors.NoBookedRooms;

    Status = BookingStatus.Confirmed;
    UpdateTimestamp();
    return Result.Success;
  }

  public ErrorOr<Success> Cancel()
  {
    if (Status != BookingStatus.Pending && Status != BookingStatus.Confirmed)
      return BookingErrors.InvalidStatusTransition;

    Status = BookingStatus.Cancelled;
    UpdateTimestamp();
    return Result.Success;
  }

  public ErrorOr<Success> CheckIn()
  {
    if (Status != BookingStatus.Confirmed) return BookingErrors.InvalidStatusTransition;

    Status = BookingStatus.CheckedIn;
    UpdateTimestamp();
    return Result.Success;
  }

  public ErrorOr<Success> CheckOut()
  {
    if (Status != BookingStatus.CheckedIn) return BookingErrors.InvalidStatusTransition;

    Status = BookingStatus.Completed;
    UpdateTimestamp();
    return Result.Success;
  }

  public ErrorOr<Success> MarkAsNoShow()
  {
    if (Status != BookingStatus.Confirmed) return BookingErrors.InvalidStatusTransition;

    Status = BookingStatus.NoShow;
    UpdateTimestamp();
    return Result.Success;
  }

  public ErrorOr<Success> Expire()
  {
    if (Status != BookingStatus.Pending) return BookingErrors.InvalidStatusTransition;

    Status = BookingStatus.Expired;
    UpdateTimestamp();
    return Result.Success;
  }
  #endregion

  #region Private Helpers
  private void Recalculate()
  {
    TotalNight = (int)(CheckOutDate - CheckInDate).TotalDays;
  }

  private ErrorOr<Success> EnsureBookingEditable()
  {
    if (Status != BookingStatus.Pending)
    {
      return BookingErrors.CannotModify;
    }
    return Result.Success;
  }

  private ErrorOr<BookedRoom> FindBookedRoom(BookedRoomId bookedRoomId)
  {
    var room = _bookedRooms.FirstOrDefault(r => r.Id == bookedRoomId);
    if (room is null)
    {
      return BookingErrors.RoomNotFound;
    }
    return room;
  }

  private void UpdateTimestamp()
  {
    UpdatedAt = DateTime.UtcNow;
  }
  #endregion
}
