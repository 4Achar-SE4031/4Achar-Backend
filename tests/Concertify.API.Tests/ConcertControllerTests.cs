using Concertify.API.Controllers;
using Concertify.Domain.Dtos.Concert;
using Concertify.Domain.Exceptions;
using Concertify.Domain.Interfaces;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

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

        _controller = new ConcertController(_mockConcertService.Object, _webHostEnvironment.Object);
    }

    [Fact]
    public async Task GetConcertsAsync_ReturnsOkResult_WithListOfConcerts()
    {
        var concertFilter = new ConcertFilterDto();
        var concerts = new List<ConcertSummaryDto> {
        new() {
            Id = 1,
            Title = "Concert1",
            StartDateTime = DateTime.Now,
            City = "تهران",
            Category = "کنسرت"
        },
        new()
        {
            Id = 2,
            Title = "Concert2",
            StartDateTime = DateTime.Now,
            City = "مشهد",
            Category = "کنسرت"

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
        var returnedConcerts = Assert.IsType<List<ConcertSummaryDto>>(okResult.Value);
    }

    [Fact]
    public async Task GetConcertsAsync_ReturnsOkResult_WithEmptyList()
    {
        var concertFilter = new ConcertFilterDto();
        var concerts = new ConcertListDto();
        _mockConcertService.Setup(service => service.GetConcertsAsync(concertFilter)).ReturnsAsync(concerts);

        var result = await _controller.GetConcertsAsync(concertFilter);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedConcerts = Assert.IsType<List<ConcertSummaryDto>>(okResult.Value);
        Assert.Empty(returnedConcerts);
    }

    [Fact]
    public async Task GetConcertByIdAsync_ReturnsOkResult_WithConcertDetails()
    {
        int concertId = 1;
        var concert = new ConcertDetailsDto { Id = concertId, Title = "Concert1" };
        _mockConcertService.Setup(service => service.GetConcertByIdAsync(concertId)).ReturnsAsync(concert);

        var result = await _controller.GetConcertByIdAsync(concertId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedConcert = Assert.IsType<ConcertDetailsDto>(okResult.Value);
        Assert.Equal(concertId, returnedConcert.Id);
    }

    [Fact]
    public async Task GetConcertByIdAsync_ReturnsNotFoundResult_WhenConcertDoesNotExist()
    {
        int concertId = 1;
        _mockConcertService.Setup(service => service.GetConcertByIdAsync(concertId)).Throws(new ItemNotFoundException(concertId));

        var ex = await Assert.ThrowsAsync<ItemNotFoundException>(() => _mockConcertService.Object.GetConcertByIdAsync(concertId));
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
            Category = "کنسرت"
        },
        new()
        {
            Id = 2,
            Title = "ناصر یداللهی",
            StartDateTime = DateTime.Now,
            City = "مشهد",
            Category = "کنسرت"

        },
        new()
        {
            Id = 3,
            Title = "صابر فضل الهی",
            StartDateTime = DateTime.Now,
            City = "اصفهان",
            Category = "کنسرت"

        },
    };
        _mockConcertService.Setup(service => service.SearchAsync(concertSearch)).ReturnsAsync(concerts[0..2]);

        var result = await _controller.SearchAsync(concertSearch);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedConcerts = Assert.IsType<List<ConcertSummaryDto>>(okResult.Value);
    }


}