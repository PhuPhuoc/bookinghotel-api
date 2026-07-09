namespace BookingHotel.Domain.Hotels;

public record HotelId(Guid Value) { public static HotelId New() => new(Guid.NewGuid()); }

public record RoomTypeId(Guid Value) { public static RoomTypeId New() => new(Guid.NewGuid()); }

public record RoomId(Guid Value) { public static RoomId New() => new(Guid.NewGuid()); }

public record AmenityId(Guid Value) { public static AmenityId New() => new(Guid.NewGuid()); }

public record HotelAmenityId(Guid Value) { public static HotelAmenityId New() => new(Guid.NewGuid()); }
