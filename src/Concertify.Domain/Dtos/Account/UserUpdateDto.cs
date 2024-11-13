namespace Concertify.Domain.Dtos.Account;
public record UserUpdateDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }

}
