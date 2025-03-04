// Import necessary namespaces
using FluentMigrator.Runner;
using Serilog;
using Supplier.Transactions.Configuration;
using Supplier.Transactions.Extensions;

// Create a WebApplication builder
var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for logging
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console()
);

// Configure dependencies and services using extension methods
builder.Services
    .ConfigureSerilogLogging(builder.Configuration)
    .ConfigureHttpClients(builder.Configuration)
    .ConfigureFluentValidation()
    .ConfigureJwtAuthentication(builder.Configuration)
    .ConfigureDependencies(builder.Configuration)
    .ConfigureFluentMigrator(builder.Configuration)
    .ConfigureControllers()
    .ConfigureRebusMessaging();

// Build the application
var app = builder.Build();

// Execute FluentMigrator migrations
using (var scope = app.Services.CreateScope())
{
    var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    runner.MigrateUp();
}

// Add a custom type handler for Dapper
Dapper.SqlMapper.AddTypeHandler(new GuidTypeHandler());

// Configure middlewares
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Configure Swagger for development environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Run the application
app.Run();
