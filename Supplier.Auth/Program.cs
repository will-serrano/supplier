using FluentMigrator.Runner;
using Serilog;
using Supplier.Auth.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console()
);

// Configura as depend�ncias e servi�os usando os m�todos de extens�o
builder.Services
    .ConfigureSerilogLogging(builder.Configuration)
    .ConfigureJwtAuthentication(builder.Configuration)
    .ConfigureDependencies(builder.Configuration)
    .ConfigureFluentMigrator(builder.Configuration)
    .ConfigureControllers();

var app = builder.Build();

// Executa as migra��es do FluentMigrator
using (var scope = app.Services.CreateScope())
{
    var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    runner.MigrateUp();
}

// Configura os middlewares
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
