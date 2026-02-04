using Microsoft.EntityFrameworkCore;
using Quiz.Api.Contracts;
using Quiz.Api.Data;
using Quiz.Api.Models;

namespace Quiz.Api.Features.It01;

public static class It01Api
{
    public static void MapEndpoints(RouteGroupBuilder api)
    {
        api.MapGet("/people", async (AppDbContext db) =>
        {
            var data = await db.People
                .OrderByDescending(x => x.Id)
                .Select(x => new
                {
                    x.Id,
                    x.FirstName,
                    x.LastName,
                    FullName = $"{x.FirstName} {x.LastName}",
                    x.BirthDate,
                    x.Age,
                    x.Remark,
                    x.CreatedAtUtc
                })
                .ToListAsync();

            return Results.Ok(data);
        });

        api.MapGet("/people/{id:int}", async (int id, AppDbContext db) =>
        {
            var person = await db.People
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.Id,
                    x.FirstName,
                    x.LastName,
                    FullName = $"{x.FirstName} {x.LastName}",
                    x.BirthDate,
                    x.Age,
                    x.Remark,
                    x.CreatedAtUtc
                })
                .FirstOrDefaultAsync();

            return person is null ? Results.NotFound(new { message = "Data not found" }) : Results.Ok(person);
        });

        api.MapPost("/people", async (CreatePersonRequest request, AppDbContext db) =>
        {
            var firstName = request.FirstName?.Trim() ?? string.Empty;
            var lastName = request.LastName?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                return Results.BadRequest(new { message = "First name and last name are required" });
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (request.BirthDate > today || request.BirthDate.Year < 1900)
            {
                return Results.BadRequest(new { message = "Birth date is invalid" });
            }

            var age = CalculateAge(request.BirthDate, today);
            var person = new PersonRecord
            {
                FirstName = firstName,
                LastName = lastName,
                BirthDate = request.BirthDate,
                Age = age,
                Remark = request.Remark?.Trim(),
                CreatedAtUtc = DateTime.UtcNow
            };

            db.People.Add(person);
            await db.SaveChangesAsync();

            return Results.Created($"/api/people/{person.Id}", new
            {
                person.Id,
                person.FirstName,
                person.LastName,
                FullName = $"{person.FirstName} {person.LastName}",
                person.BirthDate,
                person.Age,
                person.Remark,
                person.CreatedAtUtc
            });
        });
    }

    private static int CalculateAge(DateOnly birthDate, DateOnly today)
    {
        var age = today.Year - birthDate.Year;
        if (birthDate > today.AddYears(-age))
        {
            age--;
        }

        return Math.Max(age, 0);
    }
}