// RealEstate.API/Middleware/ExceptionHandlingMiddleware.cs
using System.Net;
using System.Text.Json;
using RealEstate.Domain.Exceptions;

namespace RealEstate.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var response = context.Response;

        var errorResponse = new ErrorResponse
        {
            Success = false
        };

        switch (exception)
        {
            case EntityNotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.Message = exception.Message;
                break;

            case UnauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Forbidden;
                errorResponse.Message = exception.Message;
                break;

            case ValidationException validationEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = "Validation failed";
                errorResponse.Errors = validationEx.Errors;
                break;

            case InvalidOperationException:
            case BuyersLockedException:
            case InvitationExpiredException:
            case DuplicateEmailException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = exception.Message;
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.Message = "An error occurred processing your request";
                _logger.LogError(exception, "Unhandled exception");
                break;
        }

        var result = JsonSerializer.Serialize(errorResponse);
        await response.WriteAsync(result);
    }
}

public class ErrorResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, string[]>? Errors { get; set; }
}

// RealEstate.API/Middleware/RateLimitingMiddleware.cs
using System.Collections.Concurrent;

namespace RealEstate.API.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly int _requestsPerMinute;
    private static readonly ConcurrentDictionary<string, RequestCounter> _clients = new();

    public RateLimitingMiddleware(
        RequestDelegate next,
        ILogger<RateLimitingMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _requestsPerMinute = configuration.GetValue<int>("RateLimit:PerMinute", 100);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var counter = _clients.GetOrAdd(clientId, _ => new RequestCounter());

        if (!counter.AllowRequest())
        {
            context.Response.StatusCode = 429;
            await context.Response.WriteAsJsonAsync(new
            {
                message = "Rate limit exceeded. Please try again later."
            });
            return;
        }

        await _next(context);
    }

    private class RequestCounter
    {
        private int _requestCount;
        private DateTime _windowStart = DateTime.UtcNow;
        private readonly object _lock = new();

        public bool AllowRequest()
        {
            lock (_lock)
            {
                var now = DateTime.UtcNow;
                if ((now - _windowStart).TotalMinutes >= 1)
                {
                    _windowStart = now;
                    _requestCount = 0;
                }

                _requestCount++;
                return _requestCount <= 100;
            }
        }
    }
}

// RealEstate.API/Extensions/ServiceCollectionExtensions.cs
using System.Text;
using FluentValidation;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RealEstate.Application.Commands.Auth;
using RealEstate.Application.EventHandlers;
using RealEstate.Application.Interfaces;
using RealEstate.Application.Services;
using RealEstate.Application.Validators;
using RealEstate.Domain.Events;
using RealEstate.Infrastructure.EventBus;
using RealEstate.Infrastructure.Repositories;
using RealEstate.Infrastructure.Services;

namespace RealEstate.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(LoginCommand).Assembly));

        // FluentValidation
        services.AddValidatorsFromAssemblyContaining<LoginCommandValidator>();

        // Firestore
        var projectId = configuration["GoogleCloud:ProjectId"];
        services.AddSingleton(FirestoreDb.Create(projectId));

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPropertyRepository, PropertyRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IInvitationRepository, InvitationRepository>();
        services.AddScoped<IPropertyRoleRepository, PropertyRoleRepository>();

        // Services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IEncryptionService, EncryptionService>();
        services.AddScoped<IStorageService, StorageService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddSingleton<IEventBus, InMemoryEventBus>();

        // Event Handlers
        services.AddScoped<DocumentUploadedEventHandler>();
        services.AddScoped<DocumentApprovedEventHandler>();
        services.AddScoped<UserInvitedEventHandler>();
        services.AddScoped<CloserApprovalRequestedEventHandler>();
        services.AddScoped<PropertyUpdatedEventHandler>();

        // Background Services
        services.AddHostedService<CronJobService>();

        return services;
    }

    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSecret = configuration["Jwt:Secret"]!;
        var key = Encoding.UTF8.GetBytes(jwtSecret);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = true;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        })
        .AddGoogle(options =>
        {
            options.ClientId = configuration["Authentication:Google:ClientId"]!;
            options.ClientSecret = configuration["Authentication:Google:ClientSecret"]!;
        });

        return services;
    }

    public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Real Estate Transaction API",
                Version = "v1",
                Description = "API for managing real estate transactions with document management and closing costs"
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
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
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    public static IServiceCollection AddCorsServices(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

        services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigins", builder =>
            {
                builder.WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        return services;
    }

    public static void RegisterEventHandlers(this IServiceProvider serviceProvider)
    {
        var eventBus = serviceProvider.GetRequiredService<IEventBus>();
        var scope = serviceProvider.CreateScope();

        eventBus.Subscribe<DocumentUploadedEvent>(async @event =>
        {
            var handler = scope.ServiceProvider.GetRequiredService<DocumentUploadedEventHandler>();
            await handler.Handle(@event);
        });

        eventBus.Subscribe<DocumentApprovedEvent>(async @event =>
        {
            var handler = scope.ServiceProvider.GetRequiredService<DocumentApprovedEventHandler>();
            await handler.Handle(@event);
        });

        eventBus.Subscribe<UserInvitedEvent>(async @event =>
        {
            var handler = scope.ServiceProvider.GetRequiredService<UserInvitedEventHandler>();
            await handler.Handle(@event);
        });

        eventBus.Subscribe<CloserApprovalRequestedEvent>(async @event =>
        {
            var handler = scope.ServiceProvider.GetRequiredService<CloserApprovalRequestedEventHandler>();
            await handler.Handle(@event);
        });

        eventBus.Subscribe<PropertyUpdatedEvent>(async @event =>
        {
            var handler = scope.ServiceProvider.GetRequiredService<PropertyUpdatedEventHandler>();
            await handler.Handle(@event);
        });
    }
}

// RealEstate.API/Program.cs
using RealEstate.API.Extensions;
using RealEstate.API.Middleware;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;
using RealEstate.Domain.Enums;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.GoogleCloudLogging(
        builder.Configuration["GoogleCloud:ProjectId"],
        useJsonOutput: true)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerServices();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddAuthenticationServices(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddCorsServices(builder.Configuration);

var app = builder.Build();

// Bootstrap admin user
await BootstrapAdminUser(app.Services, builder.Configuration);

// Register event handlers
app.Services.RegisterEventHandlers();

// Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Real Estate API v1");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigins");
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

static async Task BootstrapAdminUser(IServiceProvider services, IConfiguration configuration)
{
    using var scope = services.CreateScope();
    var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
    var encryptionService = scope.ServiceProvider.GetRequiredService<IEncryptionService>();

    var adminEmail = configuration["BootstrapAdmin:Email"];
    var adminPassword = configuration["BootstrapAdmin:Password"];
    var adminName = configuration["BootstrapAdmin:Name"];

    if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
    {
        Log.Warning("Bootstrap admin credentials not configured");
        return;
    }

    var existingAdmin = await userRepository.GetByEmailAsync(adminEmail);
    if (existingAdmin != null)
    {
        Log.Information("Bootstrap admin user already exists");
        return;
    }

    var admin = new User
    {
        Email = adminEmail,
        Name = adminName ?? "System Administrator",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
        Status = UserStatus.Active,
        IsOAuthUser = false
    };

    await userRepository.AddAsync(admin);
    Log.Information("Bootstrap admin user created successfully");
}

// RealEstate.API/appsettings.json (for reference)
/*
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "BootstrapAdmin": {
    "Email": "admin@realestate.com",
    "Password": "Admin@123456",
    "Name": "System Administrator"
  },
  "GoogleCloud": {
    "ProjectId": "your-project-id",
    "FirestoreDatabase": "(default)",
    "StorageBucket": "your-bucket-name"
  },
  "Jwt": {
    "Secret": "your-super-secret-jwt-key-at-least-32-characters-long",
    "Issuer": "RealEstateAPI",
    "Audience": "RealEstateClients",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "Authentication": {
    "Google": {
      "ClientId": "your-google-client-id.apps.googleusercontent.com",
      "ClientSecret": "your-google-client-secret"
    }
  },
  "Gmail": {
    "ServiceAccountEmail": "your-service-account@your-project.iam.gserviceaccount.com",
    "ServiceAccountKeyPath": "/path/to/service-account-key.json",
    "SenderEmail": "noreply@yourdomain.com",
    "SenderName": "Real Estate Platform"
  },
  "Encryption": {
    "Key": "your-32-byte-encryption-key-here!!"
  },
  "RateLimit": {
    "EnableRateLimiting": true,
    "PerMinute": 100,
    "PerHour": 1000
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "https://yourdomain.com"
    ]
  }
}
*/