using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Policy;

using AutoMapper;

using Concertify.Domain.Dtos.Account;
using Concertify.Domain.Exceptions;
using Concertify.Domain.Interfaces;
using Concertify.Domain.Models;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Concertify.Application.Services;

public class AccountService : IAccountService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;
    private readonly IEmailSender _emailSender;
    
    public AccountService(UserManager<ApplicationUser> userManager, IMapper mapper, IEmailSender emailSender)
    {
        _userManager = userManager;
        _mapper = mapper;
        _emailSender = emailSender;
    }

    public async Task<List<Claim>> GetTokenAsync(UserLoginDto loginDto)
    {
        var user = await _userManager.FindByNameAsync(loginDto.UserName)
            ?? throw new UserNotFoundException($"No user with username {loginDto.UserName} exists.");

        if (!await _userManager.CheckPasswordAsync(user, loginDto.Password))
            throw new UnauthorizedAccessException("Incorrect password!");

        var userRoles = await _userManager.GetRolesAsync(user);

        var userClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        userClaims.AddRange(
            userRoles.Select(r => new Claim(ClaimTypes.Role, r)));

        return userClaims;
    }


    public async Task<UserInfoDto> RegisterUserAsync(UserRegisterDto registerDto)
    {
        ApplicationUser newUser = _mapper.Map<ApplicationUser>(registerDto);

        var result = await _userManager.CreateAsync(newUser, registerDto.Password);

        if (!result.Succeeded)
            throw new Exception(result.Errors.First().Description);

        ApplicationUser createdUser = await _userManager.FindByNameAsync(newUser.UserName!)
            ?? throw new Exception("Internal server error");

        var confirmationEmailRequest = new ConfirmationEmailRequestDto 
        {
            Email = registerDto.Email 
        };

        await SendConfirmationEmailAsync(confirmationEmailRequest);

        UserInfoDto userInfo = _mapper.Map<UserInfoDto>(createdUser);

        return userInfo;
    }

    public async Task ConfirmEmailAsync(EmailConfirmationDto confirmationDto)
    {
        ApplicationUser user = await _userManager.FindByEmailAsync(confirmationDto.Email)
            ?? throw new UserNotFoundException($"no user with email {confirmationDto.Email} exists.");

        if (user.EmailConfirmed)
            throw new Exception("Your email has already been confirmed.");

        bool result = await _userManager.VerifyTwoFactorTokenAsync(user, "CustomTotpProvider", confirmationDto.ConfirmationToken);

        if (!result)
        {
            throw new Exception("Email confirmation failed!");
        }

        user.EmailConfirmed = true;
        await _userManager.UpdateAsync(user);

    }

    public async Task SendConfirmationEmailAsync(ConfirmationEmailRequestDto confirmationEmailRequestDto)
    {
        ApplicationUser user = await _userManager.FindByEmailAsync(confirmationEmailRequestDto.Email)
            ?? throw new UserNotFoundException($"No user with email {confirmationEmailRequestDto.Email} exists");

        var totpCode = await _userManager.GenerateTwoFactorTokenAsync(user, "CustomTotpProvider");

        var subject = $"{user.FirstName}, Please confirm your email.";

        var emailContent = $"Thanks for subscribing to Concertify!\n" +
        $"Your verification code is: {totpCode}\n" +
        $"Concertify Team\n";

        await _emailSender.SendEmailAsync(user.Email!, subject, emailContent);
    }

    public async Task<UserInfoDto> GetUserInfoAsync(string userId)
    {
        ApplicationUser user = await _userManager.FindByIdAsync(userId)
            ?? throw new UserNotFoundException($"no user with the id {userId} exists.");
        return _mapper.Map<UserInfoDto>(user);
    }

    public async Task<UserInfoDto> UpdateUserInfoAsync(UserUpdateDto userUpdate, string userId)
    {
        ApplicationUser user = await _userManager.FindByIdAsync(userId)
            ?? throw new UserNotFoundException($"no user with the id {userId} exists.");


        if (userUpdate.UserName is not null)
        {
            var userNameResult = await _userManager.SetUserNameAsync(user, userUpdate.UserName);
            if (!userNameResult.Succeeded)
                throw new Exception(userNameResult.Errors.First().Description);
        }

        if (userUpdate.Email is not null)
        {
            var emailResult = await _userManager.SetEmailAsync(user, userUpdate.Email);
            if (!emailResult.Succeeded)
                throw new Exception(emailResult.Errors.First().Description);
        }
        _mapper.Map(userUpdate, user);
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
            throw new Exception(result.Errors.First().Description);

        return _mapper.Map<UserInfoDto>(user);
    }

    public async Task ChangeUserPasswordAsync(ChangePasswordDto changePasswordDto, string userId)
    {
        ApplicationUser user = await _userManager.FindByIdAsync(userId)
            ?? throw new UserNotFoundException($"no user with the id {userId} exists.");

        var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.OldPassword, changePasswordDto.NewPassword);

        if (!result.Succeeded)
            throw new Exception(result.Errors.First().Description);

    }

    public async Task<string> SendPasswordResetEmailAsync(PasswordResetEmailRequestDto passwordResetEmailRequest)
    {
        ApplicationUser user = await _userManager.FindByEmailAsync(passwordResetEmailRequest.Email)
            ?? throw new UserNotFoundException($"No user with email {passwordResetEmailRequest.Email} exists");

        string passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

        var uriBuilder = new UriBuilder
        {
            Scheme = "http",
            Host = "localhost",
            Port = 3000,
            Path = "account/reset_password",
            Query = $"username={user.UserName}&PasswordResetToken={passwordResetToken}"

        };
        string resetUrl = uriBuilder.Uri.ToString();

        var emailContent = $"Dear {user.UserName},\n" +
            "click on the link below to reset your password:\n" +
            $"{resetUrl}\n" +
            "Concertify Team.\n";

        var subject = $"So, you want to reset your password?...";

        await _emailSender.SendEmailAsync(passwordResetEmailRequest.Email, subject, emailContent);
        return passwordResetToken;
    }

    public async Task ResetPasswordAsync(UserPasswordResetDto passwordResetDto)
    {
        ApplicationUser user = await _userManager.FindByNameAsync(passwordResetDto.UserName)
            ?? throw new UserNotFoundException($"No user with username {passwordResetDto.UserName} exists");

        var result = await _userManager.ResetPasswordAsync(user, passwordResetDto.PasswordResetToken, passwordResetDto.NewPassword);

        if (!result.Succeeded)
            throw new Exception(result.Errors.First().Description);
    }

}
