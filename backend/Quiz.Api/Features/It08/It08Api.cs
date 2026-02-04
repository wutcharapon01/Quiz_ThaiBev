using Microsoft.EntityFrameworkCore;
using Quiz.Api.Contracts;
using Quiz.Api.Data;
using Quiz.Api.Models;

namespace Quiz.Api.Features.It08;

public static class It08Api
{
    public static void EnsureDatabase(AppDbContext db)
    {
        db.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS it08_questions (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                DisplayOrder INTEGER NOT NULL,
                QuestionText TEXT NOT NULL,
                Choice1 TEXT NOT NULL,
                Choice2 TEXT NOT NULL,
                Choice3 TEXT NOT NULL,
                Choice4 TEXT NOT NULL,
                CreatedAtUtc TEXT NOT NULL
            );");
    }

    public static void MapEndpoints(RouteGroupBuilder api)
    {
        api.MapGet("/it08/questions", async (AppDbContext db) =>
        {
            var data = await db.It08Questions
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new
                {
                    x.Id,
                    x.DisplayOrder,
                    x.QuestionText,
                    choices = new[] { x.Choice1, x.Choice2, x.Choice3, x.Choice4 }
                })
                .ToListAsync();

            return Results.Ok(data);
        });

        api.MapPost("/it08/questions", async (CreateIt08QuestionRequest request, AppDbContext db) =>
        {
            var questionText = request.QuestionText?.Trim() ?? string.Empty;
            var c1 = request.Choice1?.Trim() ?? string.Empty;
            var c2 = request.Choice2?.Trim() ?? string.Empty;
            var c3 = request.Choice3?.Trim() ?? string.Empty;
            var c4 = request.Choice4?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(questionText) ||
                string.IsNullOrWhiteSpace(c1) ||
                string.IsNullOrWhiteSpace(c2) ||
                string.IsNullOrWhiteSpace(c3) ||
                string.IsNullOrWhiteSpace(c4))
            {
                return Results.BadRequest(new { message = "กรุณากรอกข้อมูลคำถามและตัวเลือกให้ครบ" });
            }

            var nextOrder = (await db.It08Questions.MaxAsync(x => (int?)x.DisplayOrder) ?? 0) + 1;
            var item = new It08Question
            {
                DisplayOrder = nextOrder,
                QuestionText = questionText,
                Choice1 = c1,
                Choice2 = c2,
                Choice3 = c3,
                Choice4 = c4,
                CreatedAtUtc = DateTime.UtcNow
            };

            db.It08Questions.Add(item);
            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                item.Id,
                item.DisplayOrder,
                item.QuestionText,
                choices = new[] { item.Choice1, item.Choice2, item.Choice3, item.Choice4 }
            });
        });

        api.MapDelete("/it08/questions/{id:int}", async (int id, AppDbContext db) =>
        {
            await using var tx = await db.Database.BeginTransactionAsync();

            var target = await db.It08Questions.FirstOrDefaultAsync(x => x.Id == id);
            if (target is null)
            {
                return Results.NotFound(new { message = "ไม่พบคำถามที่ต้องการลบ" });
            }

            db.It08Questions.Remove(target);
            await db.SaveChangesAsync();

            var rows = await db.It08Questions
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();

            for (var i = 0; i < rows.Count; i++)
            {
                rows[i].DisplayOrder = i + 1;
            }

            await db.SaveChangesAsync();
            await tx.CommitAsync();

            return Results.Ok(new { message = "ลบคำถามสำเร็จ" });
        });
    }
}