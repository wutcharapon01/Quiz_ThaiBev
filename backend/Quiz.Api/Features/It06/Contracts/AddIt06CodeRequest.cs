using System.ComponentModel.DataAnnotations;

namespace Quiz.Api.Contracts;

public sealed class AddIt06CodeRequest
{
    [Required]
    [MaxLength(19)]
    public string ProductCode { get; set; } = string.Empty;
}