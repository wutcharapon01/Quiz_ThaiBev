using System.ComponentModel.DataAnnotations;

namespace Quiz.Api.Contracts;

public sealed class CreateIt04ProfileRequest
{
    [Required]
    [MaxLength(80)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(80)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [MaxLength(6000000)]
    public string ProfileBase64 { get; set; } = string.Empty;

    [Required]
    [MaxLength(80)]
    public string Occupation { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Sex { get; set; } = string.Empty;

    [Required]
    public DateOnly BirthDay { get; set; }
}