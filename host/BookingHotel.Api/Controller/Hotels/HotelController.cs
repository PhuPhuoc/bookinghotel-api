using BookingHotel.Application.Hotels.Commands.CreateHotel;
using BookingHotel.Contracts.Hotels;
using ErrorOr;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace BookingHotel.Api.Controller.Hotels;

[ApiController]
[Route("api/hotels")]
public class HotelsController(
    IMessageBus bus,
    IMapper mapper
    // ILogger<AuthorsController> logger
    ) : ApiController(bus, mapper)
{
  [HttpPost]
  public async Task<IActionResult> Create(CreateHotelHTTPRequest request, CancellationToken ct)
  {
    var command = _mapper.Map<CreateHotelCommand>(request);

    var result = await _bus.InvokeAsync<ErrorOr<Guid>>(command, ct);

    return result.Match(
        hotelId => CreatedAtAction(
            nameof(GetById),
            new { id = hotelId },
            new CreateHotelHTTPResponse(hotelId)),
        Problem);
  }

  [HttpGet("{id:guid}")]
  public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
  {
    await Task.CompletedTask;

    return Ok(new
    {
      Id = id,
      Name = "Grand Hotel",
      Address = "123 Nguyen Hue, Ho Chi Minh City",
      Phone = "0123456789",
      Email = "contact@grandhotel.com",
      Description = "Dummy hotel data",
      StarRating = 5
    });
  }
}
