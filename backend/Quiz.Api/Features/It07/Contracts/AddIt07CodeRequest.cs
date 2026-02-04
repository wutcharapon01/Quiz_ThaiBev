using System.ComponentModel.DataAnnotations;

namespace Quiz.Api.Contracts;

public sealed class AddIt07CodeRequest
{
    [Required]
    [MaxLength(35)]
    public string ProductCode { get; set; } = string.Empty;
}