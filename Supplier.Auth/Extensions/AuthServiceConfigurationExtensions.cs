using FluentMigrator.Runner;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Supplier.Auth.Helper;
using Supplier.Auth.Repositories.Interfaces;
using Supplier.Auth.Repositories;
using Supplier.Auth.Services.Interfaces;
using Supplier.Auth.Services;
using System.Text;
using Supplier.Auth.Configuration.Interfaces;
using Supplier.Auth.Configuration;
using Microsoft.Data.Sqlite;
using System.Data;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace Supplier.Auth.Extensions
{
    public static class AuthServiceConfigurationExtensions
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

        public static IServiceCollection ConfigureJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            // Carrega as configurações do JWT a partir do appsettings
            var jwtSettings = new JwtSettings();
            configuration.GetSection("JwtSettings").Bind(jwtSettings);
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
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

        public static IServiceCollection ConfigureDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Supplier.Auth API",
                    Version = "v1",
                    Description = "API para autenticação da Supplier."
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
            services.AddSingleton<IToken, JwtService>();
            services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
            services.AddScoped<IDbConnection>(sp =>
            {
                var connectionString = ConnectionStringHelper.GetSqliteConnectionString(configuration);
                return new SqliteConnection(connectionString);
            });
            services.AddSingleton<JwtService>();
            services.AddScoped<IPasswordHasher<IdentityUser>, PasswordHasher<IdentityUser>>();
            services.AddScoped<IDapperWrapper, DapperWrapper>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IAuthService, AuthService>();

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
    }
}
