using System.ComponentModel.DataAnnotations;

namespace Concertify.Domain.Dtos.Account;

public record PasswordResetEmailRequestDto
{
    [Required]
    public string Email { get; set; } = default!;
}
