using FluentMigrator.Runner;
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

// Configura as dependências e serviços usando os métodos de extensão
builder.Services
    .ConfigureSerilogLogging(builder.Configuration)
    .ConfigureHttpClients(builder.Configuration)
    .ConfigureFluentValidation()
    .ConfigureJwtAuthentication(builder.Configuration)
    .ConfigureDependencies(builder.Configuration)
    .ConfigureFluentMigrator(builder.Configuration)
    .ConfigureControllers()
    .ConfigureRebusMessaging();

var app = builder.Build();

// Executa as migrações do FluentMigrator
using (var scope = app.Services.CreateScope())
{
    var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    runner.MigrateUp();
}

Dapper.SqlMapper.AddTypeHandler(new GuidTypeHandler());

// Configura os middlewares
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
