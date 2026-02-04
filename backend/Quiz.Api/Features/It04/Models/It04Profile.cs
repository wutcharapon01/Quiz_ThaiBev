namespace Quiz.Api.Models;

public sealed class It04Profile
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string ProfileBase64 { get; set; } = string.Empty;
    public string Occupation { get; set; } = string.Empty;
    public string Sex { get; set; } = string.Empty;
    public DateOnly BirthDay { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}