using System.Security.Claims;

using Concertify.Domain.Dtos.Account;
using Concertify.Domain.Models;


namespace Concertify.Domain.Interfaces;

public interface IAccountService
{
    public Task<List<Claim>> GetToken(UserLoginDto loginDto);
    public Task<UserInfoDto> RegisterUser(UserRegisterDto registerDto);
    public Task ConfirmEmail(string email, string confirmationToken);
}