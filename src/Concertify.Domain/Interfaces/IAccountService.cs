using System.Security.Claims;

using Concertify.Domain.Dtos.Account;


namespace Concertify.Domain.Interfaces;

public interface IAccountService
{
    public Task<List<Claim>> GetToken(UserLoginDto loginDto);
    public Task<string> RegisterUser(UserRegisterDto registerDto);
}