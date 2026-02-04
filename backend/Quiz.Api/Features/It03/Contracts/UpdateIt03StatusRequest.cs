using System.ComponentModel.DataAnnotations;

namespace Quiz.Api.Contracts;

public sealed class UpdateIt03StatusRequest
{
    [Required]
    [MinLength(1)]
    public List<int> Ids { get; set; } = [];

    [Required]
    [MaxLength(20)]
    public string Action { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}