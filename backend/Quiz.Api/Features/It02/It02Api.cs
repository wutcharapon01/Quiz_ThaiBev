using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Quiz.Api.Contracts;
using Quiz.Api.Data;
using Quiz.Api.Models;
using Quiz.Api.Security;

namespace Quiz.Api.Features.It02;

public sealed record It02JwtOptions(string Issuer, string Audience, int ExpireMinutes, SymmetricSecurityKey SigningKey);

public static class It02Api
{
    public static void EnsureDatabase(AppDbContext db)
    {
        db.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS user_accounts (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL,
                NormalizedUsername TEXT NOT NULL,
                PasswordHash TEXT NOT NULL,
                PasswordSalt TEXT NOT NULL,
                FailedLoginCount INTEGER NOT NULL DEFAULT 0,
                LockoutEndUtc TEXT NULL,
                CreatedAtUtc TEXT NOT NULL,
                LastLoginAtUtc TEXT NULL
            );");

        db.Database.ExecuteSqlRaw("CREATE UNIQUE INDEX IF NOT EXISTS IX_user_accounts_NormalizedUsername ON user_accounts (NormalizedUsername);");
    }

    public static void MapEndpoints(RouteGroupBuilder api, It02JwtOptions jwtOptions)
    {
        api.MapPost("/auth/register", async (RegisterUserRequest request, AppDbContext db) =>
        {
            var username = request.Username?.Trim() ?? string.Empty;
            var normalizedUsername = username.ToLowerInvariant();

            if (!IsValidUsername(username))
            {
                return Results.BadRequest(new { message = "ชื่อผู้ใช้ต้องยาว 4-30 ตัวอักษร และใช้ได้เฉพาะตัวอักษรภาษาอังกฤษ ตัวเลข จุด (.) ขีดล่าง (_) หรือขีดกลาง (-)" });
            }

            if (request.Password != request.ConfirmPassword)
            {
                return Results.BadRequest(new { message = "รหัสผ่านและยืนยันรหัสผ่านต้องตรงกัน" });
            }

            if (!AuthSecurity.IsStrongPassword(request.Password))
            {
                return Results.BadRequest(new { message = "รหัสผ่านต้องมีตัวพิมพ์ใหญ่ ตัวพิมพ์เล็ก ตัวเลข และอักขระพิเศษ" });
            }

            var exists = await db.Users.AnyAsync(x => x.NormalizedUsername == normalizedUsername);
            if (exists)
            {
                return Results.BadRequest(new { message = "ชื่อผู้ใช้นี้ถูกใช้งานแล้ว" });
            }

            var (hash, salt) = AuthSecurity.HashPassword(request.Password);

            db.Users.Add(new UserAccount
            {
                Username = username,
                NormalizedUsername = normalizedUsername,
                PasswordHash = hash,
                PasswordSalt = salt,
                CreatedAtUtc = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
            return Results.Ok(new { message = "สมัครสมาชิกสำเร็จ" });
        });

        api.MapPost("/auth/login", async (LoginRequest request, AppDbContext db) =>
        {
            var username = request.Username?.Trim() ?? string.Empty;
            var normalizedUsername = username.ToLowerInvariant();

            var user = await db.Users.FirstOrDefaultAsync(x => x.NormalizedUsername == normalizedUsername);
            if (user is null)
            {
                return Results.Json(new { message = "ชื่อผู้ใช้หรือรหัสผ่านไม่ถูกต้อง" }, statusCode: StatusCodes.Status401Unauthorized);
            }

            if (user.LockoutEndUtc.HasValue && user.LockoutEndUtc.Value > DateTime.UtcNow)
            {
                return Results.BadRequest(new { message = "บัญชีถูกล็อกชั่วคราว กรุณาลองใหม่ภายหลัง" });
            }

            var validPassword = AuthSecurity.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt);
            if (!validPassword)
            {
                user.FailedLoginCount += 1;
                if (user.FailedLoginCount >= 5)
                {
                    user.FailedLoginCount = 0;
                    user.LockoutEndUtc = DateTime.UtcNow.AddMinutes(10);
                }

                await db.SaveChangesAsync();
                return Results.Json(new { message = "ชื่อผู้ใช้หรือรหัสผ่านไม่ถูกต้อง" }, statusCode: StatusCodes.Status401Unauthorized);
            }

            user.FailedLoginCount = 0;
            user.LockoutEndUtc = null;
            user.LastLoginAtUtc = DateTime.UtcNow;
            await db.SaveChangesAsync();

            var expiresAtUtc = DateTime.UtcNow.AddMinutes(jwtOptions.ExpireMinutes);
            var token = CreateJwtToken(user, expiresAtUtc, jwtOptions);

            return Results.Ok(new
            {
                userId = user.Id,
                username = user.Username,
                token,
                expiresAtUtc
            });
        });

        api.MapGet("/auth/me", async (ClaimsPrincipal principal, AppDbContext db) =>
        {
            var userIdClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Results.Unauthorized();
            }

            var user = await db.Users
                .Where(x => x.Id == userId)
                .Select(x => new
                {
                    x.Id,
                    x.Username,
                    x.CreatedAtUtc,
                    x.LastLoginAtUtc
                })
                .FirstOrDefaultAsync();

            return user is null ? Results.NotFound(new { message = "ไม่พบผู้ใช้งาน" }) : Results.Ok(user);
        }).RequireAuthorization();
    }

    private static string CreateJwtToken(UserAccount user, DateTime expiresAtUtc, It02JwtOptions jwtOptions)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };

        var creds = new SigningCredentials(jwtOptions.SigningKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static bool IsValidUsername(string username)
    {
        if (username.Length is < 4 or > 30)
        {
            return false;
        }

        return username.All(ch => char.IsLetterOrDigit(ch) || ch is '.' or '_' or '-');
    }
}