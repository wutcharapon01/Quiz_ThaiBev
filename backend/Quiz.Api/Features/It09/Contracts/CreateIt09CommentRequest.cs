using System.ComponentModel.DataAnnotations;

namespace Quiz.Api.Contracts;

public sealed class CreateIt09CommentRequest
{
    [Required]
    [MaxLength(300)]
    public string Message { get; set; } = string.Empty;
}