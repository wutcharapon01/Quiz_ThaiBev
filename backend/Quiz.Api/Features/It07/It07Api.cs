using Microsoft.EntityFrameworkCore;
using Quiz.Api.Contracts;
using Quiz.Api.Data;
using Quiz.Api.Models;
using System.Text.RegularExpressions;

namespace Quiz.Api.Features.It07;

public static class It07Api
{
    private static readonly Regex ProductCodeRegex = new("^[A-Z0-9]{5}-[A-Z0-9]{5}-[A-Z0-9]{5}-[A-Z0-9]{5}-[A-Z0-9]{5}-[A-Z0-9]{5}$", RegexOptions.Compiled);

    public static void EnsureDatabase(AppDbContext db)
    {
        db.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS it07_product_codes (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ProductCode TEXT NOT NULL,
                CreatedAtUtc TEXT NOT NULL
            );");

        db.Database.ExecuteSqlRaw("CREATE UNIQUE INDEX IF NOT EXISTS IX_it07_product_codes_ProductCode ON it07_product_codes (ProductCode);");
    }

    public static void MapEndpoints(RouteGroupBuilder api)
    {
        api.MapGet("/it07/codes", async (AppDbContext db) =>
        {
            var rows = await db.It07ProductCodes
                .OrderByDescending(x => x.Id)
                .Select(x => new { x.Id, x.ProductCode, x.CreatedAtUtc })
                .ToListAsync();

            return Results.Ok(rows);
        });

        api.MapPost("/it07/codes", async (AddIt07CodeRequest request, AppDbContext db) =>
        {
            var code = request.ProductCode?.Trim() ?? string.Empty;

            if (!ProductCodeRegex.IsMatch(code))
            {
                return Results.BadRequest(new { message = "รูปแบบรหัสต้องเป็น XXXXX-XXXXX-XXXXX-XXXXX-XXXXX-XXXXX และใช้ A-Z กับ 0-9 เท่านั้น" });
            }

            var exists = await db.It07ProductCodes.AnyAsync(x => x.ProductCode == code);
            if (exists)
            {
                return Results.BadRequest(new { message = "รหัสสินค้านี้มีอยู่แล้ว" });
            }

            var item = new It07ProductCode
            {
                ProductCode = code,
                CreatedAtUtc = DateTime.UtcNow
            };

            db.It07ProductCodes.Add(item);
            await db.SaveChangesAsync();

            return Results.Ok(new { item.Id, item.ProductCode, item.CreatedAtUtc });
        });

        api.MapDelete("/it07/codes/{id:int}", async (int id, AppDbContext db) =>
        {
            var row = await db.It07ProductCodes.FirstOrDefaultAsync(x => x.Id == id);
            if (row is null)
            {
                return Results.NotFound(new { message = "ไม่พบข้อมูลที่ต้องการลบ" });
            }

            db.It07ProductCodes.Remove(row);
            await db.SaveChangesAsync();

            return Results.Ok(new { message = "ลบข้อมูลสำเร็จ" });
        });
    }
}