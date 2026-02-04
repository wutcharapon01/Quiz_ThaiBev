namespace Quiz.Api.Models;

public sealed class It10ExamResult
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Score { get; set; }
    public int TotalQuestions { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}