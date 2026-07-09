using BookingHotel.Application.Hotels.Commands.CreateHotel;
using BookingHotel.Contracts.Hotels;
using Mapster;

namespace BookingHotel.Api.Controller.Hotels;

public sealed class AuthorMappingConfig : IRegister
{
  public void Register(TypeAdapterConfig config)
  {
    config.NewConfig<CreateHotelHTTPRequest, CreateHotelCommand>();
  }
}
