using System.ComponentModel.DataAnnotations;

namespace Quiz.Api.Contracts;

public sealed class CreateIt08QuestionRequest
{
    [Required]
    [MaxLength(300)]
    public string QuestionText { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Choice1 { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Choice2 { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Choice3 { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Choice4 { get; set; } = string.Empty;
}