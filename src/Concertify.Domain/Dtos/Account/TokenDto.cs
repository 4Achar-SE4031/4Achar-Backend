namespace Concertify.Domain.Dtos.Account;

public record TokenDto
{
    public string Token { get; set; } = default!;
    public DateTime Expires { get; set; } = default!;
}
