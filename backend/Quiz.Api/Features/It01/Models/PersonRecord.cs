namespace Quiz.Api.Models;

public sealed class PersonRecord
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
    public int Age { get; set; }
    public string? Remark { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}