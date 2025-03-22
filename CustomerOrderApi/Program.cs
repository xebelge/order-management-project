﻿using CustomerOrders.Application.Interfaces;
using CustomerOrders.Application.Services;
using CustomerOrders.Infrastructure.Data;
using CustomerOrders.Infrastructure.Repositories;
using CustomerOrders.Infrastructure.Seed;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

// Create the builder for the web application.
var builder = WebApplication.CreateBuilder(args);

// Retrieve secrets from User Secrets (DB connection and JWT settings)
var connectionString = builder.Configuration["ConnectionStrings:DefaultConnection"];
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = Convert.FromBase64String(jwtSettings["Key"]!);

// Configure PostgreSQL connection using User Secrets.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Configure JWT Authentication.
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

// Register application dependencies.
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<CustomerOrderService>();
builder.Services.AddScoped<IUserService, UserService>();

// Add FluentValidation support.
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CustomerOrders.Application.Validators.RegisterRequestValidator>();

// Configure Swagger with JWT support.
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

// Add controllers and API explorer.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Build the web application.
var app = builder.Build();

// Enable Swagger UI.
app.UseSwagger();
app.UseSwaggerUI();

// Configure middleware pipeline.
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


// Seed the database on startup.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
    await DataSeeder.SeedAsync(context);
}

app.Run();
