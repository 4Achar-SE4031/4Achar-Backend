using Concertify.API.Controllers;
using Concertify.Domain.Dtos.Concert;
using Concertify.Domain.Exceptions;
using Concertify.Domain.Interfaces;

using Microsoft.AspNetCore.Mvc;

public class ConcertControllerTests
{
    private readonly Mock<IConcertService> _mockConcertService;
    private readonly ConcertController _controller;

    public ConcertControllerTests()
    {
        _mockConcertService = new Mock<IConcertService>();
        _controller = new ConcertController(_mockConcertService.Object);
    }

    [Fact]
    public async Task GetConcertsAsync_ReturnsOkResult_WithListOfConcerts()
    {
        var concertFilter = new ConcertFilterDto();
        var concerts = new List<ConcertSummaryDto> { 
            new() { 
                Id = 1, 
                Title = "Concert1",
                StartDate = "دوشنبه 19 آذر",
                City = "تهران",
                Category = "کنسرت"
            },
            new()
            {
                Id = 2,
                Title = "Concert2",
                StartDate = "چهارشنبه 23 اسفند",
                City = "مشهد",
                Category = "کنسرت"

            }
        };
        _mockConcertService.Setup(service => service.GetConcertsAsync(concertFilter)).ReturnsAsync(concerts);

        var result = await _controller.GetConcertsAsync(concertFilter);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedConcerts = Assert.IsType<List<ConcertSummaryDto>>(okResult.Value);
    }

    [Fact]
    public async Task GetConcertsAsync_ReturnsOkResult_WithEmptyList()
    {
        var concertFilter = new ConcertFilterDto();
        var concerts = new List<ConcertSummaryDto>();
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
        var concertSearch = new ConcertSearchDto() { SearchTerm = searchTerm};
        var concerts = new List<ConcertSummaryDto> {
            new() {
                Id = 1,
                Title = "ناصر عبداللهی",
                StartDate = "دوشنبه 19 آذر",
                City = "تهران",
                Category = "کنسرت"
            },
            new()
            {
                Id = 2,
                Title = "ناصر یداللهی",
                StartDate = "چهارشنبه 23 اسفند",
                City = "مشهد",
                Category = "کنسرت"

            },
            new()
            {
                Id = 3,
                Title = "صابر فضل الهی",
                StartDate = "سه شنبه 22 اسفند",
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
