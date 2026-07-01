using System.Reflection;
using Mapster;
using MapsterMapper;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
  Log.Information("Starting Booking Hotel API...");

  var builder = WebApplication.CreateBuilder(args);
  {
    // ── read config from appsettings.json then replace bootstrap logger
    builder.Host.UseSerilog((context, services, config) =>
        config.ReadFrom.Configuration(context.Configuration)
              .ReadFrom.Services(services));

    // ── Mapster config
    var mappingConfig = TypeAdapterConfig.GlobalSettings;
    mappingConfig.Scan(Assembly.GetExecutingAssembly());
    builder.Services.AddSingleton(mappingConfig);
    builder.Services.AddScoped<IMapper, ServiceMapper>();

    // ── project config
    builder.Services.AddSwaggerGen();
    builder.Services.AddControllers();
  }

  var app = builder.Build();
  {
    if (app.Environment.IsDevelopment())
    {
      app.UseSwagger().UseSwaggerUI();
    }

    // app.UseMiddleware<ExceptionHandlingMiddleware>();
    // app.UseSerilogRequestLogging();

    app.UseHttpsRedirection();
    app.MapControllers();
    app.MapGet("/", () => Results.Redirect("/swagger"));

    app.Run();
  }

}
catch (HostAbortedException)
{
  // EF Core design-time uses this internally.
  // Ignore to avoid noisy fatal logs during migrations.
}
catch (Exception ex)
{
  Log.Fatal(ex, "Application terminated unexpectedly.");
}
finally
{
  Log.CloseAndFlush();
}
