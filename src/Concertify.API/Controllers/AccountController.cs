using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Concertify.Domain.Dtos.Account;
using Concertify.Domain.Interfaces;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.IdentityModel.Tokens;
    
namespace Concertify.API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class AccountController : ControllerBase
{

    private readonly IAccountService _accountService;
    private readonly IConfiguration _configuration;

    public AccountController(IAccountService accountService, IConfiguration configuration)
    {
        _accountService = accountService;
        _configuration = configuration;
    }

    [HttpPost]
    [Route("jwt/create")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTokenAsync(UserLoginDto loginDto)
    {
        List<Claim> userClaims = await _accountService.GetTokenAsync(loginDto);
        SymmetricSecurityKey authSigningKey = new(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

        JwtSecurityToken token = new(
            issuer: _configuration["JWT:ValidIssuer"],
            audience: _configuration["JWT:ValidAudience"],
            expires: DateTime.Now.AddDays(1),
            claims: userClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

        string jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new
        {
            token = jwtToken,
            expires = token.ValidTo
        });
    }

    [HttpPost]
    [Route("signup")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterAsync(UserRegisterDto registerDto)
    {
        UserInfoDto userInfo = await _accountService.RegisterUserAsync(registerDto);

        return Ok(userInfo);
    }

    [HttpPost]
    [Route("confirm_email")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmailAsync(EmailConfirmationDto confirmationDto)
    {
        if (string.IsNullOrEmpty(confirmationDto.ConfirmationToken) || string.IsNullOrEmpty(confirmationDto.Email))
        {
            return BadRequest(new
            {
                detail = "Either email address or confirmation code is not valid."
            });
        }
        await _accountService.ConfirmEmailAsync(confirmationDto);

        return Ok(new
        {
            detail = "Your email was confirmed successfully!"
        });
    }

    [HttpGet]
    [Route("me")]
    public async Task<IActionResult> GetUserInfoAsync()
    {
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new Exception("User Id cannot be null.");

        var info = await _accountService.GetUserInfoAsync(userId);

        return Ok(info);
    }

    [HttpPut]
    [Route("me/update")]
    public async Task<IActionResult> UpdateUserInfoAsync(UserUpdateDto updateDto)
    {
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new Exception("User Id cannot be null.");

        var updatedInfo = await _accountService.UpdateUserInfoAsync(updateDto, userId);

        return Ok(updatedInfo);
    }

    [HttpPut]
    [Route("me/change_password")]
    public async Task<IActionResult> ChangePasswordAsync(ChangePasswordDto changePassword)
    {
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? throw new Exception("User Id was null.");

        await _accountService.ChangeUserPasswordAsync(changePassword, userId);

        return Ok(new
        {
            detail = "Your password was changed successfully!"
        });
    }

    [HttpPost]
    [Route("send_confirmation_email")]
    [AllowAnonymous]
    public async Task<IActionResult> SendConfirmationEmailAsync(ConfirmationEmailRequestDto confirmationEmailRequestDto)
    {
        await _accountService.SendConfirmationEmailAsync(confirmationEmailRequestDto);

        return Ok(new
        {
            detail = "Confirmation code was sent. please check your email."
        });
    }

    [HttpPost]
    [Route("send_reset_password_email")]
    [AllowAnonymous]
    public async Task<IActionResult> SendPasswordResetEmailAsync(PasswordResetEmailRequestDto passwordResetEmailRequestDto)
    {
        string passwordResetToken = await _accountService.SendPasswordResetEmailAsync(passwordResetEmailRequestDto);

        return Ok(new
        {
            detail = "Password reset link was sent. please check your email.",
            token = passwordResetToken
        });
    }

    [HttpPost]
    [Route("reset_password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPasswordAsync(UserPasswordResetDto passwordResetDto)
    {
        await _accountService.ResetPasswordAsync(passwordResetDto);

        return Ok(new
        {
            detail = "Your password was changed successfully."
        });
    }
}
