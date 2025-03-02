using FluentMigrator.Runner;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Rebus.Config;
using Serilog;
using Supplier.Customers.Configuration;
using Supplier.Customers.Configuration.Interfaces;
using Supplier.Customers.Helper;
using Supplier.Customers.Messaging;
using Supplier.Customers.Repositories;
using Supplier.Customers.Repositories.Interfaces;
using Supplier.Customers.Validators;
using System.Text;

namespace Supplier.Customers.Extensions
{
    public static class CustomersServiceConfigurationExtensions
    {
        public static IServiceCollection ConfigureSerilogLogging(this IServiceCollection services, IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            // Configuração adicional, se necessário, pode ser incluída aqui.
            return services;
        }

        public static IServiceCollection ConfigureFluentValidation(this IServiceCollection services)
        {
            services.AddFluentValidationAutoValidation();
            services.AddValidatorsFromAssemblyContaining<CustomerRequestDtoValidator>();
            return services;
        }

        public static IServiceCollection ConfigureJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = new JwtSettings();
            configuration.GetSection("JwtSettings").Bind(jwtSettings);
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
                    };
                });

            services.AddAuthorization();
            return services;
        }

        public static IServiceCollection ConfigureFluentMigrator(this IServiceCollection services, IConfiguration configuration)
        {
            // Recupera a connection string e configura o FluentMigrator para SQLite
            string connectionString = ConnectionStringHelper.GetSqliteConnectionString(configuration);

            services.AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddSQLite()  // Provider para SQLite
                    .WithGlobalConnectionString(connectionString)
                    // Escaneia a assembly atual para encontrar as migrações
                    .ScanIn(typeof(Program).Assembly).For.Migrations())
                .AddLogging(lb => lb.AddFluentMigratorConsole());

            return services;
        }

        public static IServiceCollection ConfigureControllers(this IServiceCollection services)
        {
            services.AddControllers();
            return services;
        }

        public static IServiceCollection ConfigureDependencies(this IServiceCollection services)
        {
            services.AddSingleton<Rebus.Serialization.ISerializer, RebusMessageSerializer>();
            services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            return services;
        }

        public static IServiceCollection ConfigureRebusMessaging(this IServiceCollection services)
        {
            services.AddRebus(configure => configure
                .Transport(t => t.UseRabbitMq("amqp://guest:guest@localhost", RoutingKeys.TransactionsToCustomers))
                .Options(o =>
                {
                    o.SetNumberOfWorkers(1);
                    // Outras opções (como política de retry) podem ser configuradas aqui
                })
                .Logging(l => l.Serilog())
            );

            services.AutoRegisterHandlersFromAssemblyOf<Program>();
            return services;
        }
    }
}
