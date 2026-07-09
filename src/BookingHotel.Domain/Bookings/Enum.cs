namespace BookingHotel.Domain.Bookings;

public enum BookingStatus
{
  Pending = 0,
  Confirmed = 1,
  CheckedIn = 2,
  Completed = 3,
  Cancelled = 4,
  NoShow = 5,
  Expired = 6,
}

public enum RoomReservationStatus
{
  Reserved = 0,
  Completed = 1,
  Cancelled = 2,
}
