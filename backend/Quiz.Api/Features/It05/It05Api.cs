using Microsoft.EntityFrameworkCore;
using Quiz.Api.Data;
using Quiz.Api.Models;

namespace Quiz.Api.Features.It05;

public static class It05Api
{
    private const int MaxQueueIndex = 259;

    public static void EnsureDatabase(AppDbContext db)
    {
        db.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS it05_queue_state (
                Id INTEGER PRIMARY KEY,
                LastIssuedIndex INTEGER NOT NULL,
                UpdatedAtUtc TEXT NOT NULL
            );");

        var exists = db.It05QueueStates.Any(x => x.Id == 1);
        if (!exists)
        {
            db.It05QueueStates.Add(new It05QueueState
            {
                Id = 1,
                LastIssuedIndex = -1,
                UpdatedAtUtc = DateTime.UtcNow
            });
            db.SaveChanges();
        }
    }

    public static void MapEndpoints(RouteGroupBuilder api)
    {
        api.MapGet("/it05/current", async (AppDbContext db) =>
        {
            var state = await db.It05QueueStates.Where(x => x.Id == 1).SingleAsync();
            return Results.Ok(new
            {
                queueNumber = FormatQueueNumber(state.LastIssuedIndex),
                updatedAtUtc = state.UpdatedAtUtc
            });
        });

        api.MapPost("/it05/issue", async (AppDbContext db) =>
        {
            var now = DateTime.UtcNow;

            await using var tx = await db.Database.BeginTransactionAsync();

            await db.Database.ExecuteSqlRawAsync(
                "UPDATE it05_queue_state SET LastIssuedIndex = CASE WHEN LastIssuedIndex >= {0} THEN 0 ELSE LastIssuedIndex + 1 END, UpdatedAtUtc = {1} WHERE Id = 1",
                MaxQueueIndex,
                now);

            var currentIndex = await db.It05QueueStates
                .Where(x => x.Id == 1)
                .Select(x => x.LastIssuedIndex)
                .SingleAsync();

            await tx.CommitAsync();

            return Results.Ok(new
            {
                queueNumber = FormatQueueNumber(currentIndex),
                issuedAtUtc = now
            });
        }).RequireRateLimiting("ticket");

        api.MapPost("/it05/clear", async (AppDbContext db) =>
        {
            var now = DateTime.UtcNow;

            await db.Database.ExecuteSqlRawAsync(
                "UPDATE it05_queue_state SET LastIssuedIndex = -1, UpdatedAtUtc = {0} WHERE Id = 1",
                now);

            return Results.Ok(new
            {
                queueNumber = "00",
                clearedAtUtc = now
            });
        });
    }

    private static string FormatQueueNumber(int index)
    {
        if (index < 0)
        {
            return "00";
        }

        var letter = (char)('A' + (index / 10));
        var number = index % 10;
        return $"{letter}{number}";
    }
}