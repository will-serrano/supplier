// Import necessary namespaces
using FluentMigrator.Runner;
using Serilog;
using Supplier.Customers.Configuration;
using Supplier.Customers.Extensions;

// Create a WebApplication builder with the provided arguments
var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for logging
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration) // Read configuration from appsettings
    .ReadFrom.Services(services) // Read services for dependency injection
    .Enrich.FromLogContext() // Enrich logs with context information
    .WriteTo.Console() // Write logs to the console
);

// Configure dependencies and services using extension methods
builder.Services
    .ConfigureSerilogLogging(builder.Configuration) // Configure Serilog logging
    .ConfigureFluentValidation() // Configure FluentValidation
    .ConfigureJwtAuthentication(builder.Configuration) // Configure JWT authentication
    .ConfigureDependencies(builder.Configuration) // Configure other dependencies
    .ConfigureFluentMigrator(builder.Configuration) // Configure FluentMigrator
    .ConfigureControllers() // Configure controllers
    .ConfigureRebusMessaging(); // Configure Rebus messaging

// Build the application
var app = builder.Build();

// Execute FluentMigrator migrations
using (var scope = app.Services.CreateScope())
{
    var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    runner.MigrateUp(); // Run the migrations
}

// Add a custom type handler for Dapper
Dapper.SqlMapper.AddTypeHandler(new GuidTypeHandler());

// Configure middlewares
app.UseSerilogRequestLogging(); // Log requests using Serilog
app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS
app.UseAuthentication(); // Enable authentication
app.UseAuthorization(); // Enable authorization
app.MapControllers(); // Map controller routes

// Configure Swagger for API documentation in development environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Enable Swagger
    app.UseSwaggerUI(); // Enable Swagger UI
}

// Run the application
app.Run();