namespace Quiz.Api.Models;

public sealed class It09Comment
{
    public int Id { get; set; }
    public string Commenter { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}