using Microsoft.EntityFrameworkCore;
using Quiz.Api.Contracts;
using Quiz.Api.Data;
using Quiz.Api.Models;

namespace Quiz.Api.Features.It03;

public static class It03Api
{
    private const string Pending = "pending";
    private const string Approved = "approved";
    private const string Rejected = "rejected";

    public static void EnsureDatabase(AppDbContext db)
    {
        db.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS it03_documents (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT NOT NULL,
                Reason TEXT NOT NULL,
                Status TEXT NOT NULL,
                CreatedAtUtc TEXT NOT NULL,
                UpdatedAtUtc TEXT NOT NULL
            );");

        if (!db.It03Documents.Any())
        {
            var seed = Enumerable.Range(1, 12).Select(i => new It03Document
            {
                Title = $"รายการที่ {i}",
                Reason = string.Empty,
                Status = i is 2 or 5 or 11 ? Approved : i == 7 ? Rejected : Pending,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            });

            db.It03Documents.AddRange(seed);
            db.SaveChanges();
        }
    }

    public static void MapEndpoints(RouteGroupBuilder api)
    {
        api.MapGet("/it03/documents", async (AppDbContext db) =>
        {
            var docs = await db.It03Documents
                .OrderBy(x => x.Id)
                .Select(x => new
                {
                    x.Id,
                    x.Title,
                    x.Reason,
                    x.Status
                })
                .ToListAsync();

            return Results.Ok(docs);
        });

        api.MapPost("/it03/documents/reset", async (AppDbContext db) =>
        {
            var docs = await db.It03Documents.OrderBy(x => x.Id).ToListAsync();
            if (docs.Count == 0)
            {
                return Results.Ok(new { message = "ไม่มีข้อมูลให้รีเซ็ต" });
            }

            foreach (var doc in docs)
            {
                doc.Status = GetDefaultStatus(doc.Id);
                doc.Reason = string.Empty;
                doc.UpdatedAtUtc = DateTime.UtcNow;
            }

            await db.SaveChangesAsync();
            return Results.Ok(new { message = "รีเซ็ตข้อมูลกลับค่าเริ่มต้นเรียบร้อย" });
        });

        api.MapPost("/it03/documents/decision", async (UpdateIt03StatusRequest request, AppDbContext db) =>
        {
            var action = request.Action?.Trim().ToLowerInvariant() ?? string.Empty;
            var reason = request.Reason?.Trim() ?? string.Empty;

            if (action is not (Approved or Rejected))
            {
                return Results.BadRequest(new { message = "ประเภทการทำรายการไม่ถูกต้อง" });
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                return Results.BadRequest(new { message = "กรุณากรอกเหตุผล" });
            }

            var ids = request.Ids.Distinct().ToList();
            if (ids.Count == 0)
            {
                return Results.BadRequest(new { message = "กรุณาเลือกรายการอย่างน้อย 1 รายการ" });
            }

            var docs = await db.It03Documents.Where(x => ids.Contains(x.Id)).ToListAsync();
            if (docs.Count != ids.Count)
            {
                return Results.BadRequest(new { message = "มีรายการที่ไม่พบในระบบ" });
            }

            if (docs.Any(x => x.Status != Pending))
            {
                return Results.BadRequest(new { message = "อนุมัติได้เฉพาะรายการที่รออนุมัติเท่านั้น" });
            }

            foreach (var doc in docs)
            {
                doc.Status = action;
                doc.Reason = reason;
                doc.UpdatedAtUtc = DateTime.UtcNow;
            }

            await db.SaveChangesAsync();
            return Results.Ok(new { message = "บันทึกสถานะเรียบร้อย" });
        });
    }

    private static string GetDefaultStatus(int id)
    {
        return id switch
        {
            2 or 5 or 11 => Approved,
            7 => Rejected,
            _ => Pending
        };
    }
}
