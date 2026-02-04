using System.ComponentModel.DataAnnotations;

namespace Quiz.Api.Contracts;

public sealed class CreateIt10ResultRequest
{
    [Required]
    [MaxLength(160)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MinLength(1)]
    public List<int> Answers { get; set; } = [];
}