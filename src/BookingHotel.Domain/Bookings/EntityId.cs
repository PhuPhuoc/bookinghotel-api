namespace BookingHotel.Domain.Bookings;

public readonly record struct BookingId(Guid Value)
{
  public static BookingId New() => new(Guid.NewGuid());
}

public readonly record struct BookedRoomId(Guid Value)
{
  public static BookedRoomId New() => new(Guid.NewGuid());
}

public readonly record struct RoomReservationId(Guid Value)
{
  public static RoomReservationId New() => new(Guid.NewGuid());
}

public readonly record struct GuestStayId(Guid Value)
{
  public static GuestStayId New() => new(Guid.NewGuid());
}
