using ErrorOr;

namespace BookingHotel.Domain.Bookings;

public static class BookingErrors
{
  // Booking
  public static readonly Error NotFound = Error.NotFound(
      code: "Booking.NotFound",
      description: "Booking not found.");

  public static readonly Error CannotModify = Error.Conflict(
      code: "Booking.CannotModify",
      description: "Booking cannot be modified in its current status.");

  public static readonly Error AlreadyConfirmed = Error.Conflict(
      code: "Booking.AlreadyConfirmed",
      description: "Booking has already been confirmed.");

  public static readonly Error AlreadyCancelled = Error.Conflict(
      code: "Booking.AlreadyCancelled",
      description: "Booking has already been cancelled.");

  public static readonly Error AlreadyCheckedIn = Error.Conflict(
      code: "Booking.AlreadyCheckedIn",
      description: "Booking has already been checked in.");

  public static readonly Error AlreadyCompleted = Error.Conflict(
      code: "Booking.AlreadyCompleted",
      description: "Booking has already been completed.");

  public static readonly Error InvalidStatusTransition = Error.Conflict(
      code: "Booking.InvalidStatusTransition",
      description: "Booking status transition is invalid.");

  // Date
  public static readonly Error InvalidStayPeriod = Error.Validation(
      code: "Booking.InvalidStayPeriod",
      description: "Check-out date must be after check-in date.");

  public static readonly Error InvalidCheckInDate = Error.Validation(
      code: "Booking.InvalidCheckInDate",
      description: "Check-in date is invalid.");

  public static readonly Error InvalidCheckOutDate = Error.Validation(
      code: "Booking.InvalidCheckOutDate",
      description: "Check-out date is invalid.");

  // Room
  public static readonly Error RoomAlreadyAdded = Error.Conflict(
      code: "Booking.RoomAlreadyAdded",
      description: "Room has already been added to the booking.");

  public static readonly Error RoomNotFound = Error.NotFound(
      code: "Booking.RoomNotFound",
      description: "Booked room not found.");

  public static readonly Error NoBookedRooms = Error.Validation(
      code: "Booking.NoBookedRooms",
      description: "Booking must contain at least one room.");

  // Guest
  public static readonly Error GuestNotFound = Error.NotFound(
      code: "Booking.GuestNotFound",
      description: "Guest not found.");

  public static readonly Error GuestAlreadyExists = Error.Conflict(
      code: "Booking.GuestAlreadyExists",
      description: "Guest already exists in the room.");

  public static readonly Error RoomCapacityExceeded = Error.Validation(
      code: "Booking.RoomCapacityExceeded",
      description: "Number of guests exceeds room capacity.");

  public static readonly Error InvalidGuestInformation = Error.Validation(
      code: "Booking.InvalidGuestInformation",
      description: "Guest information is invalid.");

  // Customer
  public static readonly Error InvalidCustomer = Error.Validation(
      code: "Booking.InvalidCustomer",
      description: "Customer information is invalid.");

  // General
  public static readonly Error NoteTooLong = Error.Validation(
      code: "Booking.NoteTooLong",
      description: "Booking note exceeds the maximum allowed length.");
}
