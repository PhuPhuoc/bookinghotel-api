using BookingHotel.Application.Abstractions;
using ErrorOr;

namespace BookingHotel.Application.Hotels.Commands.CreateHotel;

public sealed record CreateHotelCommand(
    string Name,
    string Address,
    string Phone,
    string Email,
    string Description,
    byte StarRating
) : ICommand<ErrorOr<Guid>>;
