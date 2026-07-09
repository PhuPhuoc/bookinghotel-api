using BookingHotel.Domain.Hotels;
using BookingHotel.Domain.Repositories;
using ErrorOr;

namespace BookingHotel.Application.Hotels.Commands.CreateHotel;

public sealed class CreateHotelHandler
{
  public static async Task<ErrorOr<Guid>> Handle(
      CreateHotelCommand command,
      IHotelRepository repository,
      CancellationToken ct)
  {
    var result = Hotel.Create(
        command.Name,
        command.Address,
        command.Phone,
        command.Email,
        command.Description,
        command.StarRating
        );

    if (result.IsError)
    {
      return result.Errors;
    }

    repository.Add(result.Value, ct);

    return result.Value.Id.Value;
  }
}
