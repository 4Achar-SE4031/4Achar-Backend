using System.ComponentModel.DataAnnotations;

namespace Concertify.Domain.Dtos.Account;

public record ChangePasswordDto
{
    [Required]
    public string OldPassword { get; set; } = default!;
    [Required]
    public string NewPassword { get; set; } = default!;
}
