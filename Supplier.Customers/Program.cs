using Serilog;
using Supplier.Customers.Extensions;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console()
);

builder.Services.ConfigureSerilogLogging(builder.Configuration);
builder.Services.AddControllers();
builder.Services.ConfigureFluentValidation();
builder.Services.ConfigureJwtAuthentication(builder.Configuration);
builder.Services.ConfigureDependencies();
builder.Services.ConfigureRebusMessaging();

var app = builder.Build();

// Configura os middlewares
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();