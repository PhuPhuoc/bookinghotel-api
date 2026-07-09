using ErrorOr;

namespace BookingHotel.Domain.Hotels;

public static class HotelErrors
{
  // Hotel
  public static readonly Error InvalidName = Error.Validation("Hotel.InvalidName", "Hotel name cannot be empty.");
  public static readonly Error InvalidRating = Error.Validation("Hotel.InvalidRating", "Star rating must be between 1 and 5.");
  public static readonly Error NotFound = Error.NotFound("Hotel.NotFound", "Hotel not found.");

  // RoomType
  public static readonly Error RoomTypeNotFound = Error.NotFound("Hotel.RoomTypeNotFound", "Room type not found in this hotel.");
  public static readonly Error RoomTypeAlreadyExists = Error.Conflict("Hotel.RoomTypeAlreadyExists", "Room type name already exists in this hotel.");
  public static readonly Error InvalidRoomTypeName = Error.Validation("Hotel.InvalidRoomTypeName", "Room type name cannot be empty.");
  public static readonly Error InvalidPrice = Error.Validation("Hotel.InvalidPrice", "Base price cannot be negative.");
  public static readonly Error InvalidCapacity = Error.Validation("Hotel.InvalidCapacity", "Max adults must be at least 1.");

  // Room
  public static readonly Error RoomNotFound = Error.NotFound("Hotel.RoomNotFound", "Room not found in this hotel.");
  public static readonly Error RoomNumberAlreadyExists = Error.Conflict("Hotel.RoomNumberAlreadyExists", "Room number already exists in this hotel.");
  public static readonly Error InvalidRoomNumber = Error.Validation("Hotel.InvalidRoomNumber", "Room number cannot be empty.");
  public static readonly Error InvalidRoomStatus = Error.Validation("Hotel.InvalidRoomStatus", "Room status is invalid.");

  // Amenity
  public static readonly Error InvalidAmenity = Error.Validation("Hotel.InvalidAmenity", "Amenity identity is invalid.");
  public static readonly Error AmenityAlreadyAssigned = Error.Conflict("Hotel.AmenityAlreadyAssigned", "This amenity has already been assigned to the hotel.");
  public static readonly Error AmenityNotAssigned = Error.NotFound("Hotel.AmenityNotAssigned", "This amenity is not assigned to the hotel.");
}
