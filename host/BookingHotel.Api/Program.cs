using System.Reflection;
using BookingHotel.Api.Middleware;
using BookingHotel.Application;
using BookingHotel.Infrastructure.Persistence;
using JasperFx.Resources;
using Mapster;
using MapsterMapper;
using Serilog;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;

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
    builder.Services.AddApplication();
    builder.Services.AddPersistence(builder.Configuration);

    builder.Host.UseResourceSetupOnStartup();
    builder.Host.UseWolverine(opts =>
    {
      var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
      opts.Discovery.IncludeAssembly(typeof(BookingHotel.Application.DependencyInjection).Assembly);

      // Outbox / Inbox
      opts.PersistMessagesWithPostgresql(connectionString);

      // EF Core integration
      opts.UseEntityFrameworkCoreTransactions();

      // Auto UnitOfWork
      opts.Policies.AutoApplyTransactions();
    });
  }

  var app = builder.Build();
  {
    if (app.Environment.IsDevelopment())
    {
      app.UseSwagger().UseSwaggerUI();
    }

    app.UseMiddleware<ExceptionHandlingMiddleware>();

    app.UseSerilogRequestLogging();

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
