using CustomerOrders.Application.Helpers.Validators.Primitives;
using CustomerOrders.Application.Helpers.Validators.Requests;
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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Serilog structuring
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File(
        path: "Logs/info-.log",
        restrictedToMinimumLevel: LogEventLevel.Information,
        rollingInterval: RollingInterval.Day,
        fileSizeLimitBytes: 5_000_000,
        rollOnFileSizeLimit: true,
        retainedFileCountLimit: 10
    )
    .WriteTo.File(
        path: "Logs/error-.log",
        restrictedToMinimumLevel: LogEventLevel.Error,
        rollingInterval: RollingInterval.Day,
        fileSizeLimitBytes: 5_000_000,
        rollOnFileSizeLimit: true,
        retainedFileCountLimit: 10
    )
    .CreateLogger();

builder.Host.UseSerilog();

// Retrieve secrets
var connectionString = builder.Configuration["ConnectionStrings:DefaultConnection"];
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = Convert.FromBase64String(jwtSettings["Key"]!);

// Configure PostgreSQL
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

// Dependency Injections
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<CustomerOrderService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<RedisCacheService>();

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(builder.Configuration.GetValue<string>("Redis:ConnectionString"))
);

builder.Services.AddSingleton<RabbitMqService>();
builder.Services.AddSingleton<ConsumerService>(sp =>
{
    var rabbitMqService = sp.GetRequiredService<RabbitMqService>();
    var logger = sp.GetRequiredService<ILogger<ConsumerService>>();
    return new ConsumerService(rabbitMqService, logger);
});

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<AddOrderProductItemRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ProductItemsValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<IdValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<QuantityValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<EmailValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UsernameValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

// Swagger configuration
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter JWT Bearer token. Example: Bearer {token}",
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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Start RabbitMQ Consumer
var consumerService = app.Services.GetRequiredService<ConsumerService>();
consumerService.StartConsuming();

// Middleware pipeline
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseAuthorization();
app.MapControllers();

// Run DB seed
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
    await DataSeeder.SeedAsync(context);
}

app.Run();