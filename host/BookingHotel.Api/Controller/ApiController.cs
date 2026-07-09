using ErrorOr;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace BookingHotel.Api.Controller;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiController(
   IMessageBus bus,
   IMapper mapper
    ) : ControllerBase
{
  protected readonly IMessageBus _bus = bus;

  protected readonly IMapper _mapper = mapper;

  protected IActionResult Problem(List<Error> errors)
  {
    var first = errors.First();

    var statusCode = first.Type switch
    {
      ErrorType.NotFound => StatusCodes.Status404NotFound,
      ErrorType.Conflict => StatusCodes.Status409Conflict,
      ErrorType.Validation => StatusCodes.Status422UnprocessableEntity,
      _ => StatusCodes.Status500InternalServerError,
    };

    return Problem(statusCode: statusCode, detail: first.Description);
  }
}
