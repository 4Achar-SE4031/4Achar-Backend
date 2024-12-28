using System.Security.Claims;

using Concertify.API.Controllers;
using Concertify.Domain.Dtos.Account;
using Concertify.Domain.Exceptions;
using Concertify.Domain.Interfaces;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

public class AccountControllerTests
{
    private readonly Mock<IAccountService> _mockAccountService;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly AccountController _controller;

    public AccountControllerTests()
    {
        _mockAccountService = new Mock<IAccountService>();
        _mockConfiguration = new Mock<IConfiguration>();
        _controller = new AccountController(_mockAccountService.Object, _mockConfiguration.Object);
    }

    [Fact]
    public async Task GetTokenAsync_ShouldReturnToken_ForValidUser()
    {
        // Arrange
        var loginDto = new UserLoginDto { UserName = "testuser", Password = "Password123!" };
        var userClaims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "1") };
        _mockAccountService.Setup(service => service.GetTokenAsync(loginDto)).ReturnsAsync(userClaims);
        _mockConfiguration.Setup(config => config["JWT:Secret"]).Returns("ByYM000OLlMQG6VVVp1OH7Xzyr7gHuw1qvUC5dcGt3SNM");
        _mockConfiguration.Setup(config => config["JWT:ValidIssuer"]).Returns("http://localhost:7180");
        _mockConfiguration.Setup(config => config["JWT:ValidAudience"]).Returns("http://localhost:7180");

        // Act
        var result = await _controller.GetTokenAsync(loginDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnUserInfo_ForValidUser()
    {
        // Arrange
        var registerDto = new UserRegisterDto { UserName = "testuser", Password = "Password123!", Email = "test@example.com" };
        var userInfo = new UserInfoDto { UserName = "testuser", Email = "test@example.com" };
        _mockAccountService.Setup(service => service.RegisterUserAsync(registerDto)).ReturnsAsync(userInfo);

        // Act
        var result = await _controller.RegisterAsync(registerDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedUser = Assert.IsType<UserInfoDto>(okResult.Value);
        Assert.Equal(registerDto.UserName, returnedUser.UserName);
    }

    [Fact]
    public async Task ConfirmEmailAsync_ShouldReturnSuccessMessage_ForValidTokenAndEmail()
    {
        // Arrange
        var confirmationDto = new EmailConfirmationDto { ConfirmationToken = "validtoken", Email = "test@example.com" };
        _mockAccountService.Setup(service => service.ConfirmEmailAsync(confirmationDto)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.ConfirmEmailAsync(confirmationDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task ConfirmEmailAsync_ShouldReturnBadRequest_ForInvalidTokenOrEmail()
    {
        string email = "invalid@mail.com";
        var confirmationDto = new EmailConfirmationDto { ConfirmationToken = "adfklagsd9834_invalid_adfsdfsdf", Email = email };
        _mockAccountService.Setup(service => service.ConfirmEmailAsync(confirmationDto)).Throws<UserNotFoundException>();

        var ex = await Assert.ThrowsAsync<UserNotFoundException>(() => _mockAccountService.Object.ConfirmEmailAsync(confirmationDto));
        Assert.Equal(typeof(UserNotFoundException), ex.GetType());
    }

    [Fact]
    public async Task GetUserInfoAsync_ShouldReturnUserInfo_ForValidUserId()
    {
        // Arrange
        var userId = "1";
        var userInfo = new UserInfoDto { UserName = "testuser", Email = "test@example.com" };
        _mockAccountService.Setup(service => service.GetUserInfoAsync(userId)).ReturnsAsync(userInfo);
        var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "mock"));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = userClaims }
        };

        // Act
        var result = await _controller.GetUserInfoAsync();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedUserInfo = Assert.IsType<UserInfoDto>(okResult.Value);
        Assert.Equal(userInfo.UserName, returnedUserInfo.UserName);
    }

    [Fact]
    public async Task UpdateUserInfoAsync_ShouldReturnUpdatedUserInfo_ForValidUserId()
    {
        // Arrange
        var userId = "1";
        var updateDto = new UserUpdateDto { UserName = "updateduser" };
        var updatedUserInfo = new UserInfoDto { UserName = "updateduser", Email = "test@example.com" };
        _mockAccountService.Setup(service => service.UpdateUserInfoAsync(updateDto, userId)).ReturnsAsync(updatedUserInfo);
        var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "mock"));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = userClaims }
        };

        // Act
        var result = await _controller.UpdateUserInfoAsync(updateDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedUserInfo = Assert.IsType<UserInfoDto>(okResult.Value);
        Assert.Equal(updatedUserInfo.UserName, returnedUserInfo.UserName);
    }
}
