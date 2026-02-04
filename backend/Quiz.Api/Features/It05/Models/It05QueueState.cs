namespace Quiz.Api.Models;

public sealed class It05QueueState
{
    public int Id { get; set; }
    public int LastIssuedIndex { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}