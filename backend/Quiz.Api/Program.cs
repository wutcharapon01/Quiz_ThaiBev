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

var jwtKey = builder.Configuration["Jwt:Key"] ?? string.Empty;
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "Quiz.Api";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "Quiz.Frontend";
var jwtExpireMinutes = int.TryParse(builder.Configuration["Jwt:ExpireMinutes"], out var minutes) ? minutes : 60;

if (jwtKey.Length < 32)
{
    throw new InvalidOperationException("Jwt:Key must be at least 32 characters. Please update appsettings or environment variable.");
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
    options.AddPolicy("frontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
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

app.Use(async (context, next) =>
{
    context.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
    context.Response.Headers.TryAdd("X-Frame-Options", "DENY");
    context.Response.Headers.TryAdd("Referrer-Policy", "no-referrer");
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
