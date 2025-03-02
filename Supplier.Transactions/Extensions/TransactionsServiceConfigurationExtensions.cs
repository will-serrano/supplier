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
    public static class TransactionsServiceConfigurationExtensions
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
            services.AddFluentValidationAutoValidation();
            services.AddValidatorsFromAssemblyContaining<TransactionRequestDtoValidator>();
            return services;
        }

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

        public static IServiceCollection ConfigureDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Supplier.Transactions API",
                    Version = "v1",
                    Description = "API para transações da Supplier."
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Insira o token JWT no formato: Bearer {seu_token}",
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
            services.AddTransient<ITransactionRequestService, TransactionRequestService>();
            services.AddTransient<ITransactionRequestRepository, TransactionRequestRepository>();
            services.AddTransient<ITransactionRequestMapper, TransactionRequestMapper>();
            services.AddTransient<IValidator<TransactionRequestDto>, TransactionRequestDtoValidator>();
            services.AddScoped<CustomerMessagePublisher>();
            return services;
        }

        public static IServiceCollection ConfigureRebusMessaging(this IServiceCollection services)
        {
            services.AddRebus(configure => configure
                .Transport(t => t.UseRabbitMq("amqp://guest:guest@localhost", RoutingKeys.CustomersToTransactions))
                .Options(o =>
                {
                    o.SetNumberOfWorkers(1);
                    // Outras opções podem ser configuradas aqui
                })
                .Logging(l => l.Serilog())
            );

            services.AutoRegisterHandlersFromAssemblyOf<Program>();

            return services;
        }
    }
}
