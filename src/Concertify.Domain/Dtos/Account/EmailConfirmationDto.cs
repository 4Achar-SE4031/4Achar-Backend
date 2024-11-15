using System.ComponentModel.DataAnnotations;

namespace Concertify.Domain.Dtos.Account;

public record EmailConfirmationDto
{
    [Required]
    public string ConfirmationToken{ get; set; } = default!;
    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;
}
