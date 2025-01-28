using AutoMapper;

using Concertify.Application.Services;
using Concertify.Domain.Dtos.Concert;
using Concertify.Domain.Interfaces;
using Concertify.Domain.Models;

using Moq;

namespace Concertify.Application.Tests;

public class ScraperServiceTests
{
    private readonly Mock<IScraperManager> _scraperManagerMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly ScraperService _scraperService;

    public ScraperServiceTests()
    {
        _scraperManagerMock = new Mock<IScraperManager>();
        _mapperMock = new Mock<IMapper>();
        _scraperService = new ScraperService(_scraperManagerMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Collect_ShouldCallStartScrapingWithCorrectUrl()
    {
        // Arrange
        string expectedUrl = "https://www.honarticket.com";
        _scraperManagerMock
            .Setup(sm => sm.StartScraping(expectedUrl))
            .Returns(GetTestConcertsAsync());

        _mapperMock
            .Setup(m => m.Map<ConcertSummaryDto>(It.IsAny<Concert>()))
            .Returns(new ConcertSummaryDto());

        // Act
        await foreach (var _ in _scraperService.Collect()) { }

        // Assert
        _scraperManagerMock.Verify(sm => sm.StartScraping(expectedUrl), Times.Once);
    }

    [Fact]
    public async Task Collect_ShouldMapAndYieldResultsCorrectly()
    {
        // Arrange
        var concerts = new List<Concert>
        {
            new Concert { Id = 1, Title = "Concert 1" },
            new Concert { Id = 2, Title = "Concert 2" }
        };

        _scraperManagerMock
            .Setup(sm => sm.StartScraping(It.IsAny<string>()))
            .Returns(GetTestConcertsAsync(concerts));

        _mapperMock
            .Setup(m => m.Map<ConcertSummaryDto>(It.IsAny<Concert>()))
            .Returns((Concert concert) => new ConcertSummaryDto { Id = concert.Id, Title = concert.Title });

        // Act
        var results = new List<ConcertSummaryDto>();
        await foreach (var result in _scraperService.Collect())
        {
            results.Add(result);
        }

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Equal("Concert 1", results[0].Title);
        Assert.Equal("Concert 2", results[1].Title);
    }

    [Fact]
    public async Task Collect_ShouldHandleNoResultsGracefully()
    {
        // Arrange
        _scraperManagerMock
            .Setup(sm => sm.StartScraping(It.IsAny<string>()))
            .Returns(GetTestConcertsAsync(new List<Concert>()));

        // Act
        var results = new List<ConcertSummaryDto>();
        await foreach (var result in _scraperService.Collect())
        {
            results.Add(result);
        }

        // Assert
        Assert.Empty(results);
    }

    //[Fact]
    //public async Task Collect_ShouldHandleExceptionsThrownByScraper()
    //{
    //    // Arrange
    //    _scraperManagerMock
    //        .Setup(sm => sm.StartScraping(It.IsAny<string>()))
    //        .ThrowsAsync(new Exception("Scraping failed"));

    //    // Act & Assert
    //    await Assert.ThrowsAsync<Exception>(async () =>
    //    {
    //        await foreach (var _ in _scraperService.Collect()) { }
    //    });
    //}

    // Helper method to simulate async enumerable
    private async IAsyncEnumerable<Concert> GetTestConcertsAsync(List<Concert>? concerts = null)
    {
        concerts ??= new List<Concert>
        {
            new Concert { Id = 1, Title = "Sample Concert1" },
            new Concert { Id = 2, Title = "Sample Concert2" },
            new Concert { Id = 3, Title = "Sample Concert3" },
            new Concert { Id = 4, Title = "Sample Concert4" }
        };

        foreach (var concert in concerts)
        {
            yield return concert;
        }
    }
}


