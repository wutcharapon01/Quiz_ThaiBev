using Microsoft.EntityFrameworkCore;
using Quiz.Api.Contracts;
using Quiz.Api.Data;
using Quiz.Api.Models;
using System.Text.RegularExpressions;

namespace Quiz.Api.Features.It06;

public static class It06Api
{
    private static readonly Regex ProductCodeRegex = new("^\\d{4}-\\d{4}-\\d{4}-\\d{4}$", RegexOptions.Compiled);

    public static void EnsureDatabase(AppDbContext db)
    {
        db.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS it06_products (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ProductCode TEXT NOT NULL,
                CreatedAtUtc TEXT NOT NULL
            );");

        db.Database.ExecuteSqlRaw("CREATE UNIQUE INDEX IF NOT EXISTS IX_it06_products_ProductCode ON it06_products (ProductCode);");
    }

    public static void MapEndpoints(RouteGroupBuilder api)
    {
        api.MapGet("/it06/codes", async (AppDbContext db) =>
        {
            var data = await db.It06Products
                .OrderByDescending(x => x.Id)
                .Select(x => new
                {
                    x.Id,
                    x.ProductCode,
                    x.CreatedAtUtc
                })
                .ToListAsync();

            return Results.Ok(data);
        });

        api.MapPost("/it06/codes", async (AddIt06CodeRequest request, AppDbContext db) =>
        {
            var code = request.ProductCode?.Trim() ?? string.Empty;

            if (!ProductCodeRegex.IsMatch(code))
            {
                return Results.BadRequest(new { message = "รูปแบบรหัสต้องเป็น XXXX-XXXX-XXXX-XXXX และกรอกได้เฉพาะตัวเลขเท่านั้น" });
            }

            var exists = await db.It06Products.AnyAsync(x => x.ProductCode == code);
            if (exists)
            {
                return Results.BadRequest(new { message = "รหัสสินค้านี้มีอยู่แล้ว" });
            }

            var product = new It06Product
            {
                ProductCode = code,
                CreatedAtUtc = DateTime.UtcNow
            };

            db.It06Products.Add(product);
            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                product.Id,
                product.ProductCode,
                product.CreatedAtUtc
            });
        });

        api.MapDelete("/it06/codes/{id:int}", async (int id, AppDbContext db) =>
        {
            var item = await db.It06Products.FirstOrDefaultAsync(x => x.Id == id);
            if (item is null)
            {
                return Results.NotFound(new { message = "ไม่พบข้อมูลที่ต้องการลบ" });
            }

            db.It06Products.Remove(item);
            await db.SaveChangesAsync();
            return Results.Ok(new { message = "ลบข้อมูลสำเร็จ" });
        });
    }
}
