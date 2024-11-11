using Concertify.Domain.Dtos.Account;
using Concertify.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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
    public async Task<IActionResult> GetToken(UserLoginDto loginDto)
    {
        List<Claim> userClaims = await _accountService.GetToken(loginDto);
        SymmetricSecurityKey authSigningKey = new(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

        JwtSecurityToken token = new(
            issuer: null,
            audience: null,
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
    public async Task<IActionResult> Register(UserRegisterDto registerDto)
    {
        UserInfoDto userInfo = await _accountService.RegisterUser(registerDto);

        return Ok(userInfo);
    }

    [HttpPost]
    [Route("confirm_email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] string confirmationToken)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(confirmationToken))
        {
            return BadRequest(new
            {
                detail = "Either email address or activation token is not valid."
            });
        }

        await _accountService.ConfirmEmail(email, confirmationToken);

        return Ok(new
        {
            detail = "Your email was confirmed successfully!"
        });
    }
}
