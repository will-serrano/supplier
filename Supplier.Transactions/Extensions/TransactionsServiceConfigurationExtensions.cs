using FluentMigrator.Runner;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;
using Rebus.Config;
using Rebus.Serialization;
using Serilog;
using Supplier.Transactions.Configuration;
using Supplier.Transactions.Configuration.Interfaces;
using Supplier.Transactions.Dto.Requests;
using Supplier.Transactions.Helper;
using Supplier.Transactions.HttpClients;
using Supplier.Transactions.HttpClients.Interfaces;
using Supplier.Transactions.Mappers;
using Supplier.Transactions.Mappers.Interfaces;
using Supplier.Transactions.Messaging;
using Supplier.Transactions.Messaging.Interfaces;
using Supplier.Transactions.Repositories;
using Supplier.Transactions.Repositories.Interfaces;
using Supplier.Transactions.Services;
using Supplier.Transactions.Services.Interfaces;
using Supplier.Transactions.Validators;
using System.Data;
using System.Reflection;
using System.Text;

namespace Supplier.Transactions.Extensions
{
    /// <summary>
    /// Extension methods for configuring services in the Supplier.Transactions project.
    /// </summary>
    public static class TransactionsServiceConfigurationExtensions
    {
        /// <summary>
        /// Configures Serilog logging for the application.
        /// </summary>
        /// <param name="services">The IServiceCollection to add the logging to.</param>
        /// <param name="configuration">The IConfiguration to read the logging settings from.</param>
        /// <returns>The IServiceCollection with Serilog logging configured.</returns>
        public static IServiceCollection ConfigureSerilogLogging(this IServiceCollection services, IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .WriteTo.Seq("http://localhost:5341")
                .CreateLogger();

            // Additional configuration, if necessary, can be included here.
            return services;
        }

        /// <summary>
        /// Configures FluentValidation for the application.
        /// </summary>
        /// <param name="services">The IServiceCollection to add FluentValidation to.</param>
        /// <returns>The IServiceCollection with FluentValidation configured.</returns>
        public static IServiceCollection ConfigureFluentValidation(this IServiceCollection services)
        {
            services.AddFluentValidationAutoValidation();
            services.AddValidatorsFromAssemblyContaining<TransactionRequestDtoValidator>();
            return services;
        }

        /// <summary>
        /// Configures HTTP clients for the application.
        /// </summary>
        /// <param name="services">The IServiceCollection to add the HTTP clients to.</param>
        /// <param name="configuration">The IConfiguration to read the HTTP client settings from.</param>
        /// <returns>The IServiceCollection with HTTP clients configured.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the base address for the Customer API is not configured.</exception>
        public static IServiceCollection ConfigureHttpClients(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<CustomerApiOptions>(configuration.GetSection("CustomerApi"));

            services.AddTransient<AuthenticatedHttpClientHandler>();

            services.AddHttpClient<ICustomerValidationClient, CustomerValidationClient>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<CustomerApiOptions>>().Value;
                if (string.IsNullOrEmpty(options.BaseAddress))
                {
                    throw new ArgumentNullException(nameof(sp), "Base address for Customer API is not configured.");
                }
                client.BaseAddress = new Uri(options.BaseAddress);
            })
            .AddHttpMessageHandler<AuthenticatedHttpClientHandler>()
            .AddPolicyHandler((sp, request) =>
            {
                var options = sp.GetRequiredService<IOptions<CustomerApiOptions>>().Value;
                return Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(options.TimeoutSeconds));
            })
            .AddPolicyHandler((sp, request) =>
            {
                var options = sp.GetRequiredService<IOptions<CustomerApiOptions>>().Value;
                return HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .WaitAndRetryAsync(
                        options.RetryCount,
                        retryAttempt => TimeSpan.FromSeconds(options.RetryDelaySeconds));
            });

            return services;
        }

        /// <summary>
        /// Configures JWT authentication for the application.
        /// </summary>
        /// <param name="services">The IServiceCollection to add JWT authentication to.</param>
        /// <param name="configuration">The IConfiguration to read the JWT settings from.</param>
        /// <returns>The IServiceCollection with JWT authentication configured.</returns>
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
                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                            logger.LogError("Invalid token: {Message}", context.Exception.Message);
                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddAuthorization();

            return services;
        }

        /// <summary>
        /// Configures FluentMigrator for the application.
        /// </summary>
        /// <param name="services">The IServiceCollection to add FluentMigrator to.</param>
        /// <param name="configuration">The IConfiguration to read the database settings from.</param>
        /// <returns>The IServiceCollection with FluentMigrator configured.</returns>
        public static IServiceCollection ConfigureFluentMigrator(this IServiceCollection services, IConfiguration configuration)
        {
            // Retrieve the connection string and configure FluentMigrator for SQLite
            string connectionString = ConnectionStringHelper.GetSqliteConnectionString(configuration);

            services.AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddSQLite()  // Provider for SQLite
                    .WithGlobalConnectionString(connectionString)
                    // Scan the current assembly for migrations
                    .ScanIn(typeof(Program).Assembly).For.Migrations())
                .AddLogging(lb => lb.AddFluentMigratorConsole());

            return services;
        }

        /// <summary>
        /// Configures controllers for the application.
        /// </summary>
        /// <param name="services">The IServiceCollection to add controllers to.</param>
        /// <returns>The IServiceCollection with controllers configured.</returns>
        public static IServiceCollection ConfigureControllers(this IServiceCollection services)
        {
            services.AddControllers();
            return services;
        }

        /// <summary>
        /// Configures dependencies for the application.
        /// </summary>
        /// <param name="services">The IServiceCollection to add dependencies to.</param>
        /// <param name="configuration">The IConfiguration to read the dependency settings from.</param>
        /// <returns>The IServiceCollection with dependencies configured.</returns>
        public static IServiceCollection ConfigureDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = new JwtSettings();
            configuration.GetSection("JwtSettings").Bind(jwtSettings);
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Supplier.Transactions API",
                    Version = "v1",
                    Description = "API for Supplier transactions."
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
            services.AddScoped<IDbConnection>(sp =>
            {
                var connectionString = ConnectionStringHelper.GetSqliteConnectionString(configuration);
                return new SqliteConnection(connectionString);
            });
            services.AddTransient<IJwtSecurityTokenHandlerWrapper, JwtSecurityTokenHandlerWrapper>();
            services.AddTransient<ITransactionRequestService, TransactionRequestService>();
            services.AddTransient<ITransactionRequestRepository, TransactionRequestRepository>();
            services.AddTransient<ITransactionRequestMapper, TransactionRequestMapper>();
            services.AddTransient<IValidator<TransactionRequestDto>, TransactionRequestDtoValidator>();
            services.AddScoped<ICustomerMessagePublisher, CustomerMessagePublisher>();
            services.AddScoped<IDapperWrapper, DapperWrapper>();
            services.AddScoped<CustomerMessagePublisher>();
            services.AddTransient<AuthenticatedHttpClientHandler>();

            return services;
        }

        /// <summary>
        /// Configures Rebus messaging for the application.
        /// </summary>
        /// <param name="services">The IServiceCollection to add Rebus messaging to.</param>
        /// <returns>The IServiceCollection with Rebus messaging configured.</returns>
        public static IServiceCollection ConfigureRebusMessaging(this IServiceCollection services)
        {
            services.AddRebus(configure => configure
                .Transport(t => t.UseRabbitMq("amqp://guest:guest@localhost", RoutingKeys.CustomersToTransactions))
                .Options(o =>
                {
                    o.SetNumberOfWorkers(1);
                    // Other options can be configured here
                })
                .Logging(l => l.Serilog())
            );

            services.AutoRegisterHandlersFromAssemblyOf<Program>();

            return services;
        }
    }
}
