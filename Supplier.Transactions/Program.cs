using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Rebus.Config;
using Serilog;
using Supplier.Transactions.Configuration;
using Supplier.Transactions.HttpClients;
using Supplier.Transactions.HttpClients.Interfaces;
using Supplier.Transactions.Messaging;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console()
);

// Add services to the container.

builder.Services.AddTransient<AuthenticatedHttpClientHandler>();

builder.Services.AddHttpClient<ICustomerValidationClient, CustomerValidationClient>(client =>
{
    // Lê a BaseAddress do appsettings
    client.BaseAddress = new Uri(builder.Configuration["CustomerApi:BaseAddress"]);
})
// Adiciona o handler para autenticação
.AddHttpMessageHandler<AuthenticatedHttpClientHandler>()
// Adiciona uma política de timeout
.AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(
    TimeSpan.FromSeconds(Convert.ToInt32(builder.Configuration["CustomerApi:TimeoutSeconds"]))))
// Adiciona uma política de retry para erros transitórios
.AddTransientHttpErrorPolicy(policyBuilder => policyBuilder.WaitAndRetryAsync(
    retryCount: Convert.ToInt32(builder.Configuration["CustomerApi:RetryCount"]),
    sleepDurationProvider: retryAttempt =>
        TimeSpan.FromSeconds(Convert.ToInt32(builder.Configuration["CustomerApi:RetryDelaySeconds"]))));

builder.Services.AddControllers();

var jwtSettings = new JwtSettings();
builder.Configuration.GetSection("JwtSettings").Bind(jwtSettings);
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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

builder.Services.AddAuthorization();

builder.Services.AddRebus(configure => configure
    .Transport(t => t.UseRabbitMq("amqp://guest:guest@localhost", RoutingKeys.CustomersToTransactions))
    .Options(o =>
    {
        o.SetNumberOfWorkers(1);
        // Outras opções (como política de retry) podem ser configuradas aqui
    })
    .Logging(l => l.Serilog())
);

builder.Services.AutoRegisterHandlersFromAssemblyOf<Program>();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
