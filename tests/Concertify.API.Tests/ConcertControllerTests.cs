using System.Net.Http;
using System.Security.Claims;
using System.Text;

using Concertify.API.Controllers;
using Concertify.Domain.Dtos.Concert;
using Concertify.Domain.Exceptions;
using Concertify.Domain.Interfaces;
using Concertify.Domain.Models;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

namespace Concertify.API.Tests;

public class ConcertControllerTests
{
    private readonly Mock<IConcertService> _mockConcertService;
    private readonly ConcertController _controller;
    private readonly Mock<IWebHostEnvironment> _webHostEnvironment;
    public ConcertControllerTests()
    {
        _mockConcertService = new Mock<IConcertService>();
        _webHostEnvironment = new Mock<IWebHostEnvironment>();

        _webHostEnvironment.Setup(w => w.WebRootFileProvider).Returns(new Mock<IFileProvider>().Object);
        _webHostEnvironment.Setup(w => w.WebRootPath).Returns("Concertify.API\\wwwroot");

        _controller = new ConcertController(_mockConcertService.Object, _webHostEnvironment.Object);

        var userClaims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "123"),
            new Claim(ClaimTypes.Name, "TestUser"),
            new Claim(ClaimTypes.Email, "test@example.com")
        };
        var identity = new ClaimsIdentity(userClaims, "TestAuth");
        var user = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = user
        };

        httpContext.Request.Path = "/api/concert/test";
        httpContext.Request.Scheme = "http";
        httpContext.Request.Host = HostString.FromUriComponent("www.testconcertify.com");
        httpContext.Request.ContentType = "application/json";

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

    }

    [Fact]
    public async Task GetConcertsAsync_ReturnsOkResult_WithListOfConcerts()
    {
        var concertFilter = new ConcertFilterDto();
        var concerts = new List<ConcertSummaryDto> {
            new()
            {
                Id = 1,
                Title = "Concert1",
                StartDateTime = DateTime.Now,
                City = "تهران",
                Category = "کنسرت",
                CardImage = _webHostEnvironment.Object.WebRootPath + "images/concerts"
            },
            new()
            {
                Id = 2,
                Title = "Concert2",
                StartDateTime = DateTime.Now,
                City = "مشهد",
                Category = "کنسرت",
                CardImage = _webHostEnvironment.Object.WebRootPath + "images/concerts"
            }
        };

        var returnValue = new ConcertListDto()
        {
            Concerts = concerts,
            TotalCount = concerts.Count
        };
        _mockConcertService.Setup(service => service.GetConcertsAsync(concertFilter)).ReturnsAsync(returnValue);

        var result = await _controller.GetConcertsAsync(concertFilter);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedConcerts = Assert.IsType<ConcertListDto>(okResult.Value);
    }

    [Fact]
    public async Task GetConcertsAsync_ReturnsOkResult_WithEmptyList()
    {
        var concertFilter = new ConcertFilterDto();
        var concertDtos = new List<ConcertSummaryDto>();
        var concerts = new ConcertListDto()
        {
            TotalCount = 0,
            Concerts = concertDtos
        };
        _mockConcertService.Setup(service => service.GetConcertsAsync(concertFilter)).ReturnsAsync(concerts);

        var result = await _controller.GetConcertsAsync(concertFilter);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedConcerts = Assert.IsType<ConcertListDto>(okResult.Value);
        Assert.Empty(returnedConcerts.Concerts);
    }

    [Fact]
    public async Task GetConcertByIdAsync_ReturnsOkResult_WithConcertDetails()
    {
        int concertId = 1;
        var concert = new ConcertDetailsDto { Id = concertId, Title = "Concert1", CoverImage = _webHostEnvironment.Object.WebRootPath + "images/concerts" };
        string? userId = _controller.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? null;


        _mockConcertService.Setup(service => service.GetConcertByIdAsync(concertId, userId)).ReturnsAsync(concert);

        var result = await _controller.GetConcertByIdAsync(concertId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedConcert = Assert.IsType<ConcertDetailsDto>(okResult.Value);
        Assert.Equal(concertId, returnedConcert.Id);
    }

    [Fact]
    public async Task GetConcertByIdAsync_ReturnsNotFoundResult_WhenConcertDoesNotExist()
    {
        int concertId = 1;
        string? userId = _controller.User?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? null;
        _mockConcertService.Setup(service => service.GetConcertByIdAsync(concertId, userId)).Throws(new ItemNotFoundException(concertId));

        var ex = await Assert.ThrowsAsync<ItemNotFoundException>(() => _mockConcertService.Object.GetConcertByIdAsync(concertId, userId));
        Assert.Equal(typeof(ItemNotFoundException), ex.GetType());
    }

    [Fact]
    public async Task SearchAsync_ReturnsOkResult_WithListOfConcerts()
    {
        string searchTerm = "ناصر";
        var concertSearch = new ConcertSearchDto() { SearchTerm = searchTerm };
        var concerts = new List<ConcertSummaryDto> {
        new() {
            Id = 1,
            Title = "ناصر عبداللهی",
            StartDateTime = DateTime.Now,
            City = "تهران",
            Category = "کنسرت",
            CardImage = _webHostEnvironment.Object.WebRootPath + "images/concerts"
        },
        new()
        {
            Id = 2,
            Title = "ناصر یداللهی",
            StartDateTime = DateTime.Now,
            City = "مشهد",
            Category = "کنسرت",
            CardImage = _webHostEnvironment.Object.WebRootPath + "images/concerts"

        },
        new()
        {
            Id = 3,
            Title = "صابر فضل الهی",
            StartDateTime = DateTime.Now,
            City = "اصفهان",
            Category = "کنسرت",
            CardImage = _webHostEnvironment.Object.WebRootPath + "images/concerts"

        },
    };
        _mockConcertService.Setup(service => service.SearchAsync(concertSearch)).ReturnsAsync(concerts[0..2]);

        var result = await _controller.SearchAsync(concertSearch);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedConcerts = Assert.IsType<List<ConcertSummaryDto>>(okResult.Value);
    }

    [Fact]
    public async Task RateConcertAsync_ShouldRateConcert_WhenValidRequest()
    {
        // Arrange
        var concertId = 1;
        var stars = 5.0f;
        var userId = "user1";

        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }));

        var controller = new ConcertController(_mockConcertService.Object, _webHostEnvironment.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };

        _mockConcertService
            .Setup(service => service.RateConcertAsync(It.IsAny<ConcertRatingDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await controller.RateConcertAsync(stars, concertId);

        // Assert
        var okResult = Assert.IsType<OkResult>(result);
        _mockConcertService.Verify(service => service.RateConcertAsync(It.Is<ConcertRatingDto>(dto =>
            dto.ConcertId == concertId && dto.Rating == stars && dto.UserId == userId)), Times.Once);
    }

    [Fact]
    public async Task RateConcertAsync_ShouldThrowException_WhenUserNotAuthenticated()
    {
        // Arrange
        var concertId = 1;
        var stars = 4.0f;

        var controller = new ConcertController(_mockConcertService.Object, _webHostEnvironment.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => controller.RateConcertAsync(stars, concertId));
    }

    [Fact]
    public async Task ToggleBookmarkAsync_ShouldToggleBookmark_WhenValidRequest()
    {
        // Arrange
        var concertId = 1;
        var userId = "user1";

        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }));

        var controller = new ConcertController(_mockConcertService.Object, _webHostEnvironment.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };

        _mockConcertService
            .Setup(service => service.ToggleBookmarkAsync(concertId, userId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await controller.ToggleBookmarkAsync(concertId);

        // Assert
        var okResult = Assert.IsType<OkResult>(result);
        _mockConcertService.Verify(service => service.ToggleBookmarkAsync(concertId, userId), Times.Once);
    }

    [Fact]
    public async Task ToggleBookmarkAsync_ShouldThrowException_WhenUserNotAuthenticated()
    {
        // Arrange
        var concertId = 1;

        var controller = new ConcertController(_mockConcertService.Object, _webHostEnvironment.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => controller.ToggleBookmarkAsync(concertId));
    }



}