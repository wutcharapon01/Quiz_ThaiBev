using Microsoft.EntityFrameworkCore;
using Quiz.Api.Contracts;
using Quiz.Api.Data;
using Quiz.Api.Models;

namespace Quiz.Api.Features.It09;

public static class It09Api
{
    private const string DefaultCommenter = "Blend 285";

    public static void EnsureDatabase(AppDbContext db)
    {
        db.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS it09_comments (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Commenter TEXT NOT NULL,
                Message TEXT NOT NULL,
                CreatedAtUtc TEXT NOT NULL
            );");

        if (!db.It09Comments.Any())
        {
            db.It09Comments.Add(new It09Comment
            {
                Commenter = DefaultCommenter,
                Message = "have a good day",
                CreatedAtUtc = DateTime.UtcNow
            });
            db.SaveChanges();
        }
    }

    public static void MapEndpoints(RouteGroupBuilder api)
    {
        api.MapGet("/it09/thread", async (AppDbContext db) =>
        {
            var comments = await db.It09Comments
                .OrderBy(x => x.CreatedAtUtc)
                .Select(x => new
                {
                    x.Id,
                    x.Commenter,
                    x.Message,
                    x.CreatedAtUtc
                })
                .ToListAsync();

            return Results.Ok(new
            {
                post = new
                {
                    author = "Change can",
                    createdAtUtc = new DateTime(2021, 10, 16, 9, 0, 0, DateTimeKind.Utc),
                    imageUrl = "https://images.unsplash.com/photo-1548199973-03cce0bbc87b?auto=format&fit=crop&w=1200&q=80"
                },
                comments
            });
        });

        api.MapPost("/it09/comments", async (CreateIt09CommentRequest request, AppDbContext db) =>
        {
            var message = request.Message?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(message))
            {
                return Results.BadRequest(new { message = "กรุณาพิมพ์ข้อความก่อนส่ง" });
            }

            var item = new It09Comment
            {
                Commenter = DefaultCommenter,
                Message = message,
                CreatedAtUtc = DateTime.UtcNow
            };

            db.It09Comments.Add(item);
            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                item.Id,
                item.Commenter,
                item.Message,
                item.CreatedAtUtc
            });
        }).RequireRateLimiting("comment");
    }
}