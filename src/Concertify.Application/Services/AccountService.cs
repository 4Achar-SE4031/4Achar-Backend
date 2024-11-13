using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using AutoMapper;

using Concertify.Domain.Dtos.Account;
using Concertify.Domain.Interfaces;
using Concertify.Domain.Models;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

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
            ?? throw new Exception("User not found!");

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

        await SendConfirmationEmailAsync(createdUser);

        UserInfoDto userInfo = _mapper.Map<UserInfoDto>(createdUser);

        return userInfo;
    }

    public async Task ConfirmEmailAsync(string email, string confirmationToken)
    {
        ApplicationUser user = await _userManager.FindByEmailAsync(email)
            ?? throw new Exception("Unexpected error occured!");

        bool result = await _userManager.VerifyTwoFactorTokenAsync(user, "CustomTotpProvider", confirmationToken);

        if (!result)
        {
            throw new Exception("Email confirmation failed!");
        }

        user.EmailConfirmed = true;
        await _userManager.UpdateAsync(user);

    }

    public async Task SendConfirmationEmailAsync(ApplicationUser user)
    {
        var totpCode = await _userManager.GenerateTwoFactorTokenAsync(user, "CustomTotpProvider");

        var emailContent = $"Thanks for subscribing to Concertify!\n" +
        $"Your verification code is: {totpCode}\n" +
        $"Concertify Team\n";

        await _emailSender.SendEmailAsync(user.Email!, "Concertify team. Confirm your email", emailContent);
    }

    public async Task<UserInfoDto> GetUserInfoAsync(string userId)
    {
        ApplicationUser user = await _userManager.FindByIdAsync(userId)
            ?? throw new Exception("User not found!");
        return _mapper.Map<UserInfoDto>(user);
    }

    public async Task<UserInfoDto> UpdateUserInfoAsync(UserUpdateDto userUpdate, string userId)
    {
        ApplicationUser user = await _userManager.FindByIdAsync(userId)
            ?? throw new Exception("User not found!");

        _mapper.Map(userUpdate, user);

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
            throw new Exception(result.Errors.First().Description);

        return _mapper.Map<UserInfoDto>(user);
    }

    public async Task ChangeUserPasswordAsync(ChangePasswordDto changePasswordDto, string userId)
    {
        ApplicationUser user = await _userManager.FindByIdAsync(userId)
            ?? throw new Exception("User not found!");

        var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.OldPassword, changePasswordDto.NewPassword);

        if (!result.Succeeded)
            throw new Exception("Unexpected error occured!");

    }

}
