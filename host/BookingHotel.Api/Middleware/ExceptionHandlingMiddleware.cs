using System.Net;
using System.Text.Json;
using Serilog;

namespace BookingHotel.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next)
{
  public async Task InvokeAsync(HttpContext context)
  {
    try
    {
      await next(context);
    }
    catch (Exception ex)
    {
      Log.Error(ex, "Unhandled exception occurred.");
      context.Response.ContentType = "application/json";
      context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

      var response = new
      {
        statusCode = 500,
        error = "An error occurred while processing your request."
      };

      await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
  }
}
