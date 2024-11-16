using System.ComponentModel.DataAnnotations;

namespace Concertify.Domain.Dtos.Account;

public record UserPasswordResetDto
{
    [Required]
    public string UserName { get; set; } = default!;
    [Required]
    public string PasswordResetToken { get; set; } = default!;
    [Required]
    public string NewPassword { get; set; } = default!;
    [Required]
    public string ConfirmNewPassword {  get; set; } = default!;
}
