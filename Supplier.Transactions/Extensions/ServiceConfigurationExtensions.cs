using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Polly.Extensions.Http;
using Rebus.Config;
using Serilog;
using Supplier.Transactions.HttpClients.Interfaces;
using Supplier.Transactions.HttpClients;
using System.Text;
using Supplier.Transactions.Configuration;
using Supplier.Transactions.Messaging;
using Microsoft.Extensions.Options;
using FluentValidation;
using Supplier.Transactions.Dto.Requests;
using Supplier.Transactions.Mappers.Interfaces;
using Supplier.Transactions.Mappers;
using Supplier.Transactions.Repositories.Interfaces;
using Supplier.Transactions.Repositories;
using Supplier.Transactions.Services.Interfaces;
using Supplier.Transactions.Services;
using Supplier.Transactions.Validators;

namespace Supplier.Transactions.Extensions
{
    public static class ServiceConfigurationExtensions
    {
        public static IServiceCollection ConfigureSerilogLogging(this IServiceCollection services, IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            // Configuração adicional, se necessário, pode ser incluída aqui.
            return services;
        }

        public static IServiceCollection ConfigureHttpClients(this IServiceCollection services)
        {
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

        public static IServiceCollection ConfigureDependencies(this IServiceCollection services)
        {
            services.AddTransient<ITransactionRequestService, TransactionRequestService>();
            services.AddTransient<ITransactionRequestRepository, TransactionRequestRepository>();
            services.AddTransient<ITransactionRequestMapper, TransactionRequestMapper>();
            services.AddTransient<IValidator<TransactionRequestDto>, TransactionRequestDtoValidator>();
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
