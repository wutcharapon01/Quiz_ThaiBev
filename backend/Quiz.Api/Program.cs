using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Quiz.Api.Data;
using Quiz.Api.Features.It01;
using Quiz.Api.Features.It02;
using Quiz.Api.Features.It03;
using Quiz.Api.Features.It04;
using Quiz.Api.Features.It05;
using Quiz.Api.Features.It06;
using Quiz.Api.Features.It07;
using Quiz.Api.Features.It08;
using Quiz.Api.Features.It09;
using Quiz.Api.Features.It10;
using Quiz.Api.Security;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// *********************************************************************************
// # Windows (CMD) set JWT_SECRET_KEY=your-secure-random-key-at-least-32-characters
// *********************************************************************************

var jwtKeyFromEnv = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
var jwtKeyFromConfig = builder.Configuration["Jwt:Key"] ?? string.Empty;

var jwtKey = !string.IsNullOrEmpty(jwtKeyFromEnv) 
    ? jwtKeyFromEnv 
    : (builder.Environment.IsDevelopment() ? jwtKeyFromConfig : string.Empty);

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "Quiz.Api";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "Quiz.Frontend";
var jwtExpireMinutes = int.TryParse(builder.Configuration["Jwt:ExpireMinutes"], out var minutes) ? minutes : 60;

if (jwtKey.Length < 32)
{
    throw new InvalidOperationException(
        "JWT_SECRET_KEY environment variable must be at least 32 characters. " +
        "Set via: export JWT_SECRET_KEY=\"your-secure-random-key-at-least-32-chars\"");
}

var signingKey = AuthSecurity.CreateSigningKey(jwtKey);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Quiz API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Input JWT token in this format: Bearer {your token}."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            []
        }
    });
});

builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.RequestMethod |
                            HttpLoggingFields.RequestPath |
                            HttpLoggingFields.ResponseStatusCode |
                            HttpLoggingFields.Duration;
});

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var dbPath = Path.Combine(builder.Environment.ContentRootPath, "quiz.db");
    options.UseSqlite($"Data Source={dbPath}");
});

builder.Services.AddCors(options =>
{
   
    var allowedOrigins = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS");
    var origins = !string.IsNullOrEmpty(allowedOrigins)
        ? allowedOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries)
        : new[] { "http://localhost:5173", "http://127.0.0.1:5173" };

    options.AddPolicy("frontend", policy =>
    {
        policy.WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("api", context =>
    {
        var key = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 60,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        });
    });

    options.AddPolicy("ticket", context =>
    {
        var key = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter($"ticket-{key}", _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromSeconds(10),
            QueueLimit = 0,
            AutoReplenishment = true
        });
    });

    options.AddPolicy("comment", context =>
    {
        var key = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter($"comment-{key}", _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 12,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        });
    });

    options.AddPolicy("exam", context =>
    {
        var key = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter($"exam-{key}", _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 20,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        });
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    It02Api.EnsureDatabase(db);
    It03Api.EnsureDatabase(db);
    It04Api.EnsureDatabase(db);
    It05Api.EnsureDatabase(db);
    It06Api.EnsureDatabase(db);
    It07Api.EnsureDatabase(db);
    It08Api.EnsureDatabase(db);
    It09Api.EnsureDatabase(db);
    It10Api.EnsureDatabase(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseHttpLogging();
app.UseCors("frontend");
app.UseRateLimiter();

// Security: CSRF Protection via Origin header validation
// For state-changing requests, validate that Origin matches allowed origins
app.Use(async (context, next) =>
{
    var method = context.Request.Method;
    var isStateChanging = method is "POST" or "PUT" or "DELETE" or "PATCH";
    
    if (isStateChanging)
    {
        var origin = context.Request.Headers.Origin.FirstOrDefault();
        var referer = context.Request.Headers.Referer.FirstOrDefault();
        
        // Get allowed origins (same as CORS config)
        var allowedOriginsEnv = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS");
        var allowedOrigins = !string.IsNullOrEmpty(allowedOriginsEnv)
            ? allowedOriginsEnv.Split(',', StringSplitOptions.RemoveEmptyEntries)
            : new[] { "http://localhost:5173", "http://127.0.0.1:5173" };
        
        // Check if Origin header is present and valid
        if (!string.IsNullOrEmpty(origin))
        {
            if (!allowedOrigins.Any(allowed => origin.Equals(allowed, StringComparison.OrdinalIgnoreCase)))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { message = "CSRF validation failed: Invalid origin" });
                return;
            }
        }
        // If no Origin but has Referer, check Referer
        else if (!string.IsNullOrEmpty(referer))
        {
            var refererHost = new Uri(referer).GetLeftPart(UriPartial.Authority);
            if (!allowedOrigins.Any(allowed => refererHost.Equals(allowed, StringComparison.OrdinalIgnoreCase)))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { message = "CSRF validation failed: Invalid referer" });
                return;
            }
        }
        // If neither Origin nor Referer, it could be a non-browser request (API client) - allow if has Authorization header
        else if (string.IsNullOrEmpty(context.Request.Headers.Authorization.FirstOrDefault()))
        {
            // No Origin, no Referer, no Auth header = likely CSRF attempt
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { message = "CSRF validation failed: Missing origin" });
            return;
        }
    }
    
    await next();
});

app.Use(async (context, next) =>
{
    context.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
    context.Response.Headers.TryAdd("X-Frame-Options", "DENY");
    context.Response.Headers.TryAdd("Referrer-Policy", "no-referrer");
    // Security: Content-Security-Policy to mitigate XSS attacks
    context.Response.Headers.TryAdd("Content-Security-Policy", 
        "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data: blob:; font-src 'self'; connect-src 'self'");
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

var api = app.MapGroup("/api").RequireRateLimiting("api");

It01Api.MapEndpoints(api);
It02Api.MapEndpoints(api, new It02JwtOptions(jwtIssuer, jwtAudience, jwtExpireMinutes, signingKey));
It03Api.MapEndpoints(api);
It04Api.MapEndpoints(api);
It05Api.MapEndpoints(api);
It06Api.MapEndpoints(api);
It07Api.MapEndpoints(api);
It08Api.MapEndpoints(api);
It09Api.MapEndpoints(api);
It10Api.MapEndpoints(api);

app.Run();
