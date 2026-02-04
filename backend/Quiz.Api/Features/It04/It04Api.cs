using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using Quiz.Api.Contracts;
using Quiz.Api.Data;
using Quiz.Api.Models;

namespace Quiz.Api.Features.It04;

public static class It04Api
{
    private static readonly string[] Occupations =
    [
        "นักพัฒนาโปรแกรม",
        "นักวิเคราะห์ข้อมูล",
        "นักบัญชี",
        "เจ้าหน้าที่บุคคล",
        "นักการตลาด",
        "นักออกแบบ"
    ];

    public static void EnsureDatabase(AppDbContext db)
    {
        db.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS it04_profiles (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                FirstName TEXT NOT NULL,
                LastName TEXT NOT NULL,
                Email TEXT NOT NULL,
                Phone TEXT NOT NULL,
                ProfileBase64 TEXT NOT NULL,
                Occupation TEXT NOT NULL,
                Sex TEXT NOT NULL,
                BirthDay TEXT NOT NULL,
                CreatedAtUtc TEXT NOT NULL
            );");
    }

    public static void MapEndpoints(RouteGroupBuilder api)
    {
        api.MapGet("/it04/occupations", () => Results.Ok(Occupations));

        api.MapPost("/it04/profiles", async (CreateIt04ProfileRequest request, AppDbContext db) =>
        {
            var firstName = request.FirstName?.Trim() ?? string.Empty;
            var lastName = request.LastName?.Trim() ?? string.Empty;
            var email = request.Email?.Trim() ?? string.Empty;
            var phone = request.Phone?.Trim() ?? string.Empty;
            var profileBase64 = request.ProfileBase64?.Trim() ?? string.Empty;
            var occupation = request.Occupation?.Trim() ?? string.Empty;
            var sex = request.Sex?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(phone) ||
                string.IsNullOrWhiteSpace(profileBase64) ||
                string.IsNullOrWhiteSpace(occupation) ||
                string.IsNullOrWhiteSpace(sex))
            {
                return Results.BadRequest(new { message = "กรุณากรอกข้อมูลให้ครบทุกช่อง" });
            }

            if (!Occupations.Contains(occupation))
            {
                return Results.BadRequest(new { message = "อาชีพไม่ถูกต้อง" });
            }

            if (!IsValidEmail(email))
            {
                return Results.BadRequest(new { message = "รูปแบบอีเมลไม่ถูกต้อง" });
            }

            if (!IsValidPhone(phone))
            {
                return Results.BadRequest(new { message = "รูปแบบเบอร์โทรไม่ถูกต้อง" });
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (request.BirthDay > today || request.BirthDay.Year < 1900)
            {
                return Results.BadRequest(new { message = "วันเกิดไม่ถูกต้อง" });
            }

            if (!IsValidImageBase64(profileBase64))
            {
                return Results.BadRequest(new { message = "ไฟล์โปรไฟล์ต้องเป็นรูปภาพที่ถูกต้อง (Base64 Image)" });
            }

            if (sex is not ("Male" or "Female"))
            {
                return Results.BadRequest(new { message = "ค่าเพศไม่ถูกต้อง" });
            }

            var profile = new It04Profile
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Phone = phone,
                ProfileBase64 = profileBase64,
                Occupation = occupation,
                Sex = sex,
                BirthDay = request.BirthDay,
                CreatedAtUtc = DateTime.UtcNow
            };

            db.It04Profiles.Add(profile);
            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                id = profile.Id,
                message = $"save data success id : {profile.Id:00000}"
            });
        });
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var parsed = new MailAddress(email);
            return parsed.Address.Equals(email, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static bool IsValidPhone(string phone)
    {
        if (phone.Length is < 9 or > 15)
        {
            return false;
        }

        return phone.All(char.IsDigit);
    }

    private static bool IsValidImageBase64(string value)
    {
        if (!value.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var commaIndex = value.IndexOf(',');
        if (commaIndex < 0)
        {
            return false;
        }

        var meta = value[..commaIndex];
        if (!meta.Contains(";base64", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var payload = value[(commaIndex + 1)..];
        if (payload.Length < 16 || payload.Length % 4 != 0)
        {
            return false;
        }

        try
        {
            var bytes = Convert.FromBase64String(payload);
            return HasImageSignature(bytes);
        }
        catch
        {
            return false;
        }
    }

    private static bool HasImageSignature(byte[] bytes)
    {
        if (bytes.Length < 8)
        {
            return false;
        }

        if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
        {
            return true;
        }

        if (bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
        {
            return true;
        }

        if (bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x38)
        {
            return true;
        }

        if (bytes.Length >= 12 &&
            bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46 &&
            bytes[8] == 0x57 && bytes[9] == 0x45 && bytes[10] == 0x42 && bytes[11] == 0x50)
        {
            return true;
        }

        return bytes[0] == 0x42 && bytes[1] == 0x4D;
    }
}