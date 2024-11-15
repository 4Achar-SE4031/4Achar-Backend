using System.ComponentModel.DataAnnotations;

namespace Concertify.Domain.Dtos.Account;

public record ConfirmationEmailRequestDto
{
    [Required]
    public string Email { get; set; } = default!;
}
