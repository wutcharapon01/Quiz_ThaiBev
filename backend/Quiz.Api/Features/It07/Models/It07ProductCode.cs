namespace Quiz.Api.Models;

public sealed class It07ProductCode
{
    public int Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}