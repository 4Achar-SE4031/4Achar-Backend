using System.Security.Claims;

using Concertify.Domain.Dtos.Account;
using Concertify.Domain.Dtos.Concert;
using Concertify.Domain.Models;


namespace Concertify.Domain.Interfaces;

public interface IAccountService
{
    public Task<List<Claim>> GetTokenAsync(UserLoginDto loginDto);
    public Task<UserInfoDto> RegisterUserAsync(UserRegisterDto registerDto);
    public Task ConfirmEmailAsync(EmailConfirmationDto confirmationDto);
    public Task SendConfirmationEmailAsync(ConfirmationEmailRequestDto confirmationEmailRequestDto);
    public Task<UserInfoDto> GetUserInfoAsync(string userId);
    public Task<UserInfoDto> UpdateUserInfoAsync(UserUpdateDto userUpdate, string userId);
    public Task ChangeUserPasswordAsync(ChangePasswordDto changePasswordDto, string userId);
    public Task<string> SendPasswordResetEmailAsync(PasswordResetEmailRequestDto passwordResetEmailRequestDto);
    public Task ResetPasswordAsync(UserPasswordResetDto passwordResetDto);
    public Task<List<ConcertSummaryDto>> GetBookmarkedConcertsAsync(string userId);
}