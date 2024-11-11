using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using AutoMapper;

using Concertify.Domain.Dtos.Account;
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

    public async Task<List<Claim>> GetToken(UserLoginDto loginDto)
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


    public async Task<UserInfoDto> RegisterUser(UserRegisterDto registerDto)
    {
        ApplicationUser newUser = _mapper.Map<ApplicationUser>(registerDto);

        var result = await _userManager.CreateAsync(newUser, registerDto.Password);

        if (!result.Succeeded)
            throw new Exception(result.Errors.First().Description);

        ApplicationUser createdUser = await _userManager.FindByNameAsync(newUser.UserName!)
            ?? throw new Exception("Internal server error");

        await SendConfirmationEmailAsync(createdUser);

        UserInfoDto userInfo = new()
        {
            FirstName = createdUser.FirstName!,
            LastName = createdUser.LastName!,
            Email = createdUser.Email!,
            UserName = createdUser.UserName!,
        };

        return userInfo;
    }

    public async Task ConfirmEmail(string email, string confirmationToken)
    {
        ApplicationUser user = await _userManager.FindByEmailAsync(email)
            ?? throw new Exception("Unexpected error occured!");

        var result = await _userManager.ConfirmEmailAsync(user, confirmationToken);

        if (!result.Succeeded)
            throw new Exception("Unexpected error occured!");
    }

    private async Task SendConfirmationEmailAsync(ApplicationUser user)
    {
        var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink = "http://localhost:5000/Account/confirm_email/";
        var emailContent = $"Thanks for subscribing to Concertify!" +
        $"<br/><br/>" +
        $"To activate your email, please click on the following link: " +
        $"<br/><br/>" +
        $"<a href=\"{confirmationLink}\">Activation Link</a>" +
        $"<br/><br/>" +
        $"<a href=\"{confirmationLink}\">{confirmationLink}</a>" +
        $"<br/><br/>" +
        $"Concertify Team";

        await _emailSender.SendEmailAsync(user.Email!, "Concertify team. Confirm your email", emailContent);
    }

}
