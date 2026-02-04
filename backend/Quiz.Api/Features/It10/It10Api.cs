using Microsoft.EntityFrameworkCore;
using Quiz.Api.Contracts;
using Quiz.Api.Data;
using Quiz.Api.Models;

namespace Quiz.Api.Features.It10;

public static class It10Api
{
    private sealed record It10Question(int Id, string Text, string[] Choices, int CorrectIndex);

    private static readonly It10Question[] Questions =
    [
        new(1, "ข้อใดคือจำนวนคี่ต่อไปนี้", ["3", "5", "9", "11"], 0),
        new(2, "2 x ? = 2 + 4 จงหาค่า x", ["1", "2", "3", "4"], 1)
    ];

    public static void EnsureDatabase(AppDbContext db)
    {
        db.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS it10_exam_results (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                FullName TEXT NOT NULL,
                Score INTEGER NOT NULL,
                TotalQuestions INTEGER NOT NULL,
                CreatedAtUtc TEXT NOT NULL
            );");
    }

    public static void MapEndpoints(RouteGroupBuilder api)
    {
        api.MapGet("/it10/questions", () =>
        {
            var payload = Questions.Select(q => new
            {
                id = q.Id,
                text = q.Text,
                choices = q.Choices
            });

            return Results.Ok(new { questions = payload });
        });

        api.MapPost("/it10/submit", async (CreateIt10ResultRequest request, AppDbContext db) =>
        {
            var fullName = request.FullName?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return Results.BadRequest(new { message = "กรุณากรอกชื่อ-นามสกุล" });
            }

            if (request.Answers.Count != Questions.Length)
            {
                return Results.BadRequest(new { message = "จำนวนคำตอบไม่ถูกต้อง" });
            }

            if (request.Answers.Any(x => x < 0 || x > 3))
            {
                return Results.BadRequest(new { message = "รูปแบบคำตอบไม่ถูกต้อง" });
            }

            var score = 0;
            for (var i = 0; i < Questions.Length; i++)
            {
                if (request.Answers[i] == Questions[i].CorrectIndex)
                {
                    score++;
                }
            }

            var result = new It10ExamResult
            {
                FullName = fullName,
                Score = score,
                TotalQuestions = Questions.Length,
                CreatedAtUtc = DateTime.UtcNow
            };

            db.It10ExamResults.Add(result);
            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                result.Id,
                result.FullName,
                result.Score,
                result.TotalQuestions,
                result.CreatedAtUtc
            });
        }).RequireRateLimiting("exam");
    }
}
