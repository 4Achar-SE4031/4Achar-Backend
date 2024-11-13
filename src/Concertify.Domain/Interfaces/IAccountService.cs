using System.Security.Claims;

using Concertify.Domain.Dtos.Account;
using Concertify.Domain.Models;


namespace Concertify.Domain.Interfaces;

public interface IAccountService
{
    public Task<List<Claim>> GetTokenAsync(UserLoginDto loginDto);
    public Task<UserInfoDto> RegisterUserAsync(UserRegisterDto registerDto);
    public Task ConfirmEmailAsync(string email, string confirmationToken);
    public Task SendConfirmationEmailAsync(ApplicationUser user);
    public Task<UserInfoDto> GetUserInfoAsync(string userId);
    public Task<UserInfoDto> UpdateUserInfoAsync(UserUpdateDto userUpdate, string userId);
    public Task ChangeUserPasswordAsync(ChangePasswordDto changePasswordDto, string userId);

}