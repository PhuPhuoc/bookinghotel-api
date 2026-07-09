namespace BookingHotel.Contracts.Hotels;

public record CreateHotelHTTPRequest(
  string Name,
  string Address,
  string Phone,
  string Email,
  string Description,
  byte StarRating);

