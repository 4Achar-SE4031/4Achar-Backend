using System.Security.Claims;

using AutoMapper;

using Concertify.Application.Services;
using Concertify.Domain.Dtos.Account;
using Concertify.Domain.Exceptions;
using Concertify.Domain.Models;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

using Moq;

namespace Concertify.Application.Tests;
public class AccountServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IEmailSender> _emailSenderMock;
    private readonly AccountService _accountService;

    public AccountServiceTests()
    {
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(), null!, null!, null!, null!, null!, null!, null!, null!);

        _mapperMock = new Mock<IMapper>();
        _emailSenderMock = new Mock<IEmailSender>();

        _accountService = new AccountService(
            _userManagerMock.Object,
            _mapperMock.Object,
            _emailSenderMock.Object);
    }

    [Fact]
    public async Task GetTokenAsync_ShouldReturnClaims_WhenLoginIsSuccessful()
    {
        var loginDto = new UserLoginDto { UserName = "testUser", Password = "password123" };
        var user = new ApplicationUser { Id = "1", UserName = "testUser" };
        var roles = new List<string> { "Admin", "User" };

        _userManagerMock.Setup(um => um.FindByNameAsync(loginDto.UserName)).ReturnsAsync(user);
        _userManagerMock.Setup(um => um.CheckPasswordAsync(user, loginDto.Password)).ReturnsAsync(true);
        _userManagerMock.Setup(um => um.GetRolesAsync(user)).ReturnsAsync(roles);

        var claims = await _accountService.GetTokenAsync(loginDto);

        Assert.NotNull(claims);
        Assert.Contains(claims, c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id);
        Assert.Contains(claims, c => c.Type == ClaimTypes.Name && c.Value == user.UserName);
        Assert.Contains(claims, c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        Assert.Contains(claims, c => c.Type == ClaimTypes.Role && c.Value == "User");
    }

    [Fact]
    public async Task GetTokenAsync_ShouldThrowException_WhenUserNotFound()
    {
        var loginDto = new UserLoginDto { UserName = "nonexistentUser", Password = "password123" };

        _userManagerMock.Setup(um => um.FindByNameAsync(loginDto.UserName)).ReturnsAsync((ApplicationUser)null!);

        await Assert.ThrowsAsync<UserNotFoundException>(() => _accountService.GetTokenAsync(loginDto));
    }

    [Fact]
    public async Task RegisterUserAsync_ShouldReturnUserInfo_WhenRegistrationIsSuccessful()
    {
        var registerDto = new UserRegisterDto
        {
            UserName = "newUser",
            Password = "password123",
            Email = "newuser@example.com"
        };

        var newUser = new ApplicationUser { UserName = "newUser", Email = "newuser@example.com" };
        var createdUser = new ApplicationUser { Id = "1", UserName = "newUser", Email = "newuser@example.com" };
        var userInfo = new UserInfoDto {UserName = "newUser" };

        _mapperMock.Setup(m => m.Map<ApplicationUser>(registerDto)).Returns(newUser);
        _userManagerMock.Setup(um => um.CreateAsync(newUser, registerDto.Password)).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(um => um.FindByNameAsync(newUser.UserName)).ReturnsAsync(createdUser);
        _userManagerMock.Setup(um => um.FindByEmailAsync(newUser.Email)).ReturnsAsync(createdUser);
        _mapperMock.Setup(m => m.Map<UserInfoDto>(createdUser)).Returns(userInfo);

        var result = await _accountService.RegisterUserAsync(registerDto);

        Assert.NotNull(result);
        Assert.Equal(userInfo.UserName, result.UserName);
        _emailSenderMock.Verify(es => es.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_ShouldThrowException_WhenUserCreationFails()
    {
        var registerDto = new UserRegisterDto { UserName = "newUser", Password = "password123" };

        var newUser = new ApplicationUser { UserName = "newUser" };

        _mapperMock.Setup(m => m.Map<ApplicationUser>(registerDto)).Returns(newUser);
        _userManagerMock.Setup(um => um.CreateAsync(newUser, registerDto.Password)).ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "User creation failed." }));

        var exception = await Assert.ThrowsAsync<Exception>(() => _accountService.RegisterUserAsync(registerDto));
        Assert.Equal("User creation failed.", exception.Message);
    }

    [Fact]
    public async Task ConfirmEmailAsync_ShouldConfirmEmail_WhenTokenIsValid()
    {
        var emailConfirmationDto = new EmailConfirmationDto { Email = "user@example.com", ConfirmationToken = "valid-token" };
        var user = new ApplicationUser { Email = "user@example.com", EmailConfirmed = false };

        _userManagerMock.Setup(um => um.FindByEmailAsync(emailConfirmationDto.Email)).ReturnsAsync(user);
        _userManagerMock.Setup(um => um.VerifyTwoFactorTokenAsync(user, "CustomTotpProvider", emailConfirmationDto.ConfirmationToken)).ReturnsAsync(true);

        await _accountService.ConfirmEmailAsync(emailConfirmationDto);

        Assert.True(user.EmailConfirmed);
        _userManagerMock.Verify(um => um.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task ConfirmEmailAsync_ShouldThrowException_WhenEmailAlreadyConfirmed()
    {
        var emailConfirmationDto = new EmailConfirmationDto { Email = "user@example.com", ConfirmationToken = "valid-token" };
        var user = new ApplicationUser { Email = "user@example.com", EmailConfirmed = true };

        _userManagerMock.Setup(um => um.FindByEmailAsync(emailConfirmationDto.Email)).ReturnsAsync(user);

        var exception = await Assert.ThrowsAsync<Exception>(() => _accountService.ConfirmEmailAsync(emailConfirmationDto));
        Assert.Equal("Your email has already been confirmed.", exception.Message);
    }
}
