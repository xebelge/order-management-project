using System.Reflection;
using CustomerOrders.Application.Helpers.Validators.AuthValidators;
using CustomerOrders.Application.Helpers.Validators.ProductValidators;
using CustomerOrders.Application.Interfaces;
using CustomerOrders.Application.Middleware;
using CustomerOrders.Application.Services;
using CustomerOrders.Application.Services.ConsumerService;
using CustomerOrders.Application.Services.RabbitMQ;
using CustomerOrders.Core.Interfaces;
using CustomerOrders.Infrastructure.Data;
using CustomerOrders.Infrastructure.Repositories;
using CustomerOrders.Infrastructure.Seed;
using FluentValidation;
using FluentValidation.AspNetCore;
using HealthChecks.UI.Client;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Serilog Configuration
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File("Logs/info-.log", LogEventLevel.Information, rollingInterval: RollingInterval.Day,
                  fileSizeLimitBytes: 5_000_000, rollOnFileSizeLimit: true, retainedFileCountLimit: 10)
    .WriteTo.File("Logs/error-.log", LogEventLevel.Error, rollingInterval: RollingInterval.Day,
                  fileSizeLimitBytes: 5_000_000, rollOnFileSizeLimit: true, retainedFileCountLimit: 10)
    .CreateLogger();

builder.Host.UseSerilog();

// Configuration values
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = Convert.FromBase64String(jwtSettings["Key"]!);

// PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// JWT Authentication
builder.Services.AddAuthentication(options =>
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
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(jwtKey),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// CQRS - MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    cfg.RegisterServicesFromAssemblyContaining<CustomerOrders.Application.Queries.CustomerQueries.GetCustomerByIdQuery>();
});

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateProductRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<AddCustomerOrdersCommand>();

// AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Dependency Injection
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<RedisCacheService>();
builder.Services.AddScoped<TokenService>();

// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"])
);

// RabbitMQ
builder.Services.AddSingleton<RabbitMqService>();
builder.Services.AddSingleton<ConsumerService>(sp =>
{
    var rabbitMqService = sp.GetRequiredService<RabbitMqService>();
    var logger = sp.GetRequiredService<ILogger<ConsumerService>>();
    return new ConsumerService(rabbitMqService, logger);
});

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Customer Order API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter JWT token as: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// HealthChecks 
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "PostgreSQL")
    .AddRedis(builder.Configuration["Redis:ConnectionString"], name: "Redis");

builder.Services.AddHealthChecksUI(options =>
{
    options.SetEvaluationTimeInSeconds(20);
    options.MaximumHistoryEntriesPerEndpoint(60);
    options.SetApiMaxActiveRequests(1);
    options.AddHealthCheckEndpoint("CustomerOrder API", "/health");
}).AddInMemoryStorage();

// Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Seed DB
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
    await DataSeeder.SeedAsync(dbContext);
}

// Start RabbitMQ Consumer
var consumerService = app.Services.GetRequiredService<ConsumerService>();
consumerService.StartConsuming();

// Middleware
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseAuthorization();
app.MapControllers();

// Health Check Endpoints
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.MapHealthChecksUI(options =>
{
    options.UIPath = "/health-ui";
});

app.Run();
