using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;
using AutoMapper;

using Concertify.Domain.Dtos.Account;
using Concertify.Domain.Interfaces;
using Concertify.Domain.Models;
using System.Data;


namespace Concertify.Application.Services;

public class AccountService : IAccountService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;
    
    public AccountService(UserManager<ApplicationUser> userManager, IMapper mapper)
    {
        _userManager = userManager;
        _mapper = mapper;
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


    public async Task<string> RegisterUser(UserRegisterDto registerDto)
    {
        var existingUser = await _userManager.FindByNameAsync(registerDto.UserName);
        if (existingUser is not null)
            throw new DuplicateNameException($"A user with the username {registerDto.UserName} already exists");

        ApplicationUser newUser = _mapper.Map<ApplicationUser>(registerDto);

        var result = await _userManager.CreateAsync(newUser, registerDto.Password);

        if (!result.Succeeded)
            throw new Exception(result.Errors.First().Description);

        return newUser.Id;
    }
}
