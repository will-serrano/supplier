using Serilog;
using Supplier.Transactions.Configuration;
using Supplier.Transactions.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console()
);

builder.Services.Configure<CustomerApiOptions>(builder.Configuration.GetSection("CustomerApi"));

builder.Services.ConfigureSerilogLogging(builder.Configuration);
builder.Services.ConfigureHttpClients();
builder.Services.ConfigureJwtAuthentication(builder.Configuration);
builder.Services.ConfigureDependencies();
builder.Services.ConfigureRebusMessaging();

builder.Services.AddControllers();

// Registering services


var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
