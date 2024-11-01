using System.ComponentModel.DataAnnotations;


namespace Concertify.Domain.Dtos.Account;

public record UserLoginDto
{
    [Required]
    public string UserName { get; set; } = default!;
    [Required]
    public string Password { get; set; } = default!;
}
