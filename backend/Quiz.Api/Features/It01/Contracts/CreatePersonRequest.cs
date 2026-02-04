using System.ComponentModel.DataAnnotations;

namespace Quiz.Api.Contracts;

public sealed class CreatePersonRequest
{
    [Required]
    [MaxLength(80)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(80)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public DateOnly BirthDate { get; set; }

    [MaxLength(500)]
    public string? Remark { get; set; }
}