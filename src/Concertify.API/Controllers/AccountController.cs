using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Concertify.Domain.Dtos.Account;
using Concertify.Domain.Interfaces;

using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
    
namespace Concertify.API.Controllers;

[ApiController]
[Route("[controller]")]
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
    public async Task<IActionResult> RegisterAsync(UserRegisterDto registerDto)
    {
        UserInfoDto userInfo = await _accountService.RegisterUserAsync(registerDto);

        return Ok(userInfo);
    }

    [HttpPost]
    [Route("confirm_email")]
    public async Task<IActionResult> ConfirmEmailAsync([FromQuery] string email, [FromQuery] string confirmationToken)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(confirmationToken))
        {
            return BadRequest(new
            {
                detail = "Either email address or activation token is not valid."
            });
        }

        await _accountService.ConfirmEmailAsync(email, confirmationToken);

        return Ok(new
        {
            detail = "Your email was confirmed successfully!"
        });
    }

    [HttpGet]
    [Route("me")]
    public async Task<IActionResult> GetUserInfoAsync()
    {
        if (!User.Identity.IsAuthenticated)
            return Unauthorized();

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
               ?? throw new Exception("User Id cannot be null.");

        await _accountService.ChangeUserPasswordAsync(changePassword, userId);

        return Ok(new
        {
            detail = "Your password was changed successfully!"
        });
    }
}
