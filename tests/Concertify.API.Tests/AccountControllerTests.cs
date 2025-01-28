using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using AutoMapper;

using Concertify.API.Controllers;
using Concertify.Domain.Dtos.Account;
using Concertify.Domain.Dtos.Concert;
using Concertify.Domain.Interfaces;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using Moq;

using Xunit;

namespace Concertify.API.Tests;
public class AccountControllerTests
{
    private readonly Mock<IAccountService> _accountServiceMock = new();
    private readonly Mock<IConfiguration> _configurationMock = new();
    private readonly Mock<IWebHostEnvironment> _webHostEnvironmentMock = new();
    private readonly AccountController _controller;

    public AccountControllerTests()
    {
        _controller = new AccountController(
            _accountServiceMock.Object,
            _configurationMock.Object,
            _webHostEnvironmentMock.Object
        );

        // Mocking HTTP Context for user claims
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user-id")
        }));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    [Fact]
    public async Task GetTokenAsync_ShouldReturnToken_WhenLoginIsSuccessful()
    {
        // Arrange
        var loginDto = new UserLoginDto { UserName = "testusername", Password = "password" };
        var claims = new List<Claim> { new Claim(ClaimTypes.Name, loginDto.UserName) };

        _accountServiceMock
            .Setup(s => s.GetTokenAsync(loginDto))
            .ReturnsAsync(claims);

        string secret; // 16 bytes for 128 bits

        byte[] randomBytes = new byte[16]; // 16 bytes for 128 bits

        using (var rng = RandomNumberGenerator.Create())

        {

            rng.GetBytes(randomBytes);

        }

        secret = Convert.ToBase64String(randomBytes);


        _configurationMock
            .SetupGet(c => c["JWT:Secret"])
            .Returns("AbCD123EfgHIJ6KLNM1OH7Xzyr7gHuw1qvUC5dcGt3SNM");

        _configurationMock
            .SetupGet(c => c["JWT:ValidIssuer"])
            .Returns("issuer");

        _configurationMock
            .SetupGet(c => c["JWT:ValidAudience"])
            .Returns("audience");

        // Act
        var result = await _controller.GetTokenAsync(loginDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value as TokenDto;

        Assert.NotNull(response.Token);
        Assert.NotNull(response.Expires);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnUserInfo_WhenRegistrationIsSuccessful()
    {
        // Arrange
        var registerDto = new UserRegisterDto
        {
            UserName = "testusername",
            Email = "test@example.com",
            Password = "password"
        };

        var userInfoDto = new UserInfoDto {Email = "test@example.com", UserName = registerDto.UserName };

        _accountServiceMock
            .Setup(s => s.RegisterUserAsync(registerDto))
            .ReturnsAsync(userInfoDto);

        // Act
        var result = await _controller.RegisterAsync(registerDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<UserInfoDto>(okResult.Value);

        Assert.Equal(registerDto.UserName, response.UserName);
        Assert.Equal(registerDto.Email, response.Email);
    }

    [Fact]
    public async Task ConfirmEmailAsync_ShouldReturnOk_WhenConfirmationIsSuccessful()
    {
        // Arrange
        var confirmationDto = new EmailConfirmationDto
        {
            Email = "test@example.com",
            ConfirmationToken = "token123"
        };

        // Act
        var result = await _controller.ConfirmEmailAsync(confirmationDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Contains("confirmed successfully", okResult.Value.ToString());
    }

    [Fact]
    public async Task GetUserInfoAsync_ShouldReturnUserInfo_WhenAuthenticated()
    {
        // Arrange
        var userInfoDto = new UserInfoDto { UserName = "testusername", Email = "test@example.com" };

        _accountServiceMock
            .Setup(s => s.GetUserInfoAsync("test-user-id"))
            .ReturnsAsync(userInfoDto);

        // Act
        var result = await _controller.GetUserInfoAsync();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<UserInfoDto>(okResult.Value);

        Assert.Equal("testusername", response.UserName);
        Assert.Equal("test@example.com", response.Email);
    }

    [Fact]
    public async Task ChangePasswordAsync_ShouldReturnOk_WhenPasswordIsChanged()
    {
        // Arrange
        var changePasswordDto = new ChangePasswordDto
        {
            OldPassword = "old-password",
            NewPassword = "new-password"
        };

        // Act
        var result = await _controller.ChangePasswordAsync(changePasswordDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Contains("password was changed successfully", okResult.Value.ToString());
    }

    [Fact]
    public async Task GetBookmarkedConcertsAsync_ShouldReturnBookmarkedConcerts()
    {
        // Arrange
        var bookmarkedConcerts = new List<ConcertSummaryDto>
        {
            new() { Id = 1, Title = "Concert 1", CardImage = "/images/concert1.jpg" },
            new() { Id = 2, Title = "Concert 2", CardImage = "/images/concert2.jpg" }
        };

        _accountServiceMock
            .Setup(s => s.GetBookmarkedConcertsAsync("test-user-id"))
            .ReturnsAsync(bookmarkedConcerts);

        _webHostEnvironmentMock
            .SetupGet(w => w.WebRootPath)
            .Returns("wwwroot");

        // Act
        IActionResult result = await _controller.GetBookmarkedConcertsAsync();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ConcertListDto>(okResult.Value);

        Assert.Equal(2, response.TotalCount);
        Assert.NotNull(response.Concerts);
    }
}
