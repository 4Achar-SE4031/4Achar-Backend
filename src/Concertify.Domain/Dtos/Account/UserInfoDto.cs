﻿namespace Concertify.Domain.Dtos.Account;

public record UserInfoDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string UserName { get; set; } = default!;
    public string Email { get; set; } = default!;
}
