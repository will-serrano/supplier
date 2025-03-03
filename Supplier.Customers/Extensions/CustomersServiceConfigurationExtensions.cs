using FluentMigrator.Runner;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.Sqlite;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Rebus.Config;
using Rebus.Serialization;
using Serilog;
using Supplier.Customers.Configuration;
using Supplier.Customers.Configuration.Interfaces;
using Supplier.Customers.Helper;
using Supplier.Customers.Mappers;
using Supplier.Customers.Mappers.Interfaces;
using Supplier.Customers.Messaging;
using Supplier.Customers.Repositories;
using Supplier.Customers.Repositories.Interfaces;
using Supplier.Customers.Services;
using Supplier.Customers.Services.Interfaces;
using Supplier.Customers.Validators;
using System.Data;
using System.Reflection;
using System.Text;

namespace Supplier.Customers.Extensions
{
    public static class CustomersServiceConfigurationExtensions
    {
        public static IServiceCollection ConfigureSerilogLogging(this IServiceCollection services, IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .WriteTo.Seq("http://localhost:5341")
                .CreateLogger();

            // Configuração adicional, se necessário, pode ser incluída aqui.
            return services;
        }

        public static IServiceCollection ConfigureFluentValidation(this IServiceCollection services)
        {
            services.AddValidatorsFromAssemblyContaining<CustomerRequestDtoValidator>();
            return services;
        }

        public static IServiceCollection ConfigureJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
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

        public static IServiceCollection ConfigureDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = new JwtSettings();
            configuration.GetSection("JwtSettings").Bind(jwtSettings);
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

            services.AddValidatorsFromAssemblyContaining<CustomerRequestDtoValidator>();
            services.AddMemoryCache();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Supplier.Customers API",
                    Version = "v1",
                    Description = "API para clientes da Supplier."
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = jwtSettings.Secret,
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] {}
                        }
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

            });
            services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
            services.AddSingleton<ISerializer, RebusMessageSerializer>();
            services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
            services.AddScoped<IDbConnection>(sp =>
            {
                var connectionString = ConnectionStringHelper.GetSqliteConnectionString(configuration);
                return new SqliteConnection(connectionString);
            });
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<ICustomerMapper, CustomerMapper>();
            services.AddScoped<IDapperWrapper, DapperWrapper>();
            services.AddScoped<CustomerMessageHandler>();
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
