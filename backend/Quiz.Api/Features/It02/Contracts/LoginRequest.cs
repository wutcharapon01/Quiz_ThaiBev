using System.ComponentModel.DataAnnotations;

namespace Quiz.Api.Contracts;

public sealed class LoginRequest
{
    [Required]
    [MaxLength(60)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(120)]
    public string Password { get; set; } = string.Empty;
}