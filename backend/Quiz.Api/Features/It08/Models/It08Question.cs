namespace Quiz.Api.Models;

public sealed class It08Question
{
    public int Id { get; set; }
    public int DisplayOrder { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string Choice1 { get; set; } = string.Empty;
    public string Choice2 { get; set; } = string.Empty;
    public string Choice3 { get; set; } = string.Empty;
    public string Choice4 { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}