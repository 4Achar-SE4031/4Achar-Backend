using System.Linq.Expressions;

using AutoMapper;

using Concertify.Application.Services;
using Concertify.Domain.Dtos.Concert;
using Concertify.Domain.Exceptions;
using Concertify.Domain.Interfaces;
using Concertify.Domain.Models;

using Moq;

namespace Concertify.Application.Tests;

public class ConcertServiceTests
{
    private readonly Mock<IGenericRepository<Concert>> _concertRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly ConcertService _concertService;

    public ConcertServiceTests()
    {
        _concertRepositoryMock = new Mock<IGenericRepository<Concert>>();
        _mapperMock = new Mock<IMapper>();
        _concertService = new ConcertService(_concertRepositoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task GetConcertByIdAsync_ShouldReturnConcertDetails_WhenConcertExists()
    {
        // Arrange
        int concertId = 1;
        var concertEntity = new Concert { Id = concertId, Title = "Concert A" };
        var concertDto = new ConcertDetailsDto { Id = concertId, Title = "Concert A" };
        _concertRepositoryMock.Setup(r => r.GetByIdAsync(concertId)).ReturnsAsync(concertEntity);
        _mapperMock.Setup(m => m.Map<ConcertDetailsDto>(concertEntity)).Returns(concertDto);

        // Act
        var result = await _concertService.GetConcertByIdAsync(concertId);

        // Assert
        Assert.Equal(concertId, result.Id);
        Assert.Equal("Concert A", result.Title);
    }

    [Fact]
    public async Task GetConcertByIdAsync_ShouldThrowItemNotFoundException_WhenConcertDoesNotExist()
    {
        // Arrange
        int concertId = 1;
        _concertRepositoryMock.Setup(r => r.GetByIdAsync(concertId)).ReturnsAsync((Concert)null);

        // Act & Assert
        await Assert.ThrowsAsync<ItemNotFoundException>(() => _concertService.GetConcertByIdAsync(concertId));
    }

    [Fact]
    public async Task GetConcertsAsync_ShouldReturnFilteredConcerts()
    {
        // Arrange
        var concertFilterDto = new ConcertFilterDto { Title = "Rock", Take = 2 };
        var concerts = new List<Concert>
        {
            new() { Title = "Rock Festival" },
            new() { Title = "Rock Night" }
        };
        var concertSummaryDtos = new List<ConcertSummaryDto>
        {
            new() { Title = "Rock Festival" },
            new() { Title = "Rock Night" }
        };

        _concertRepositoryMock.Setup(r => r.GetFilteredAsync(It.IsAny<Expression<Func<Concert, bool>>[]>(), concertFilterDto.Skip, concertFilterDto.Take))
            .ReturnsAsync(concerts);
        _mapperMock.Setup(m => m.Map<List<ConcertSummaryDto>>(concerts)).Returns(concertSummaryDtos);

        // Act
        var result = await _concertService.GetConcertsAsync(concertFilterDto);

        // Assert
        Assert.Equal(2, result.Concerts.Count);
        Assert.Equal("Rock Festival", result.Concerts[0].Title);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnMatchingConcerts()
    {
        // Arrange
        var searchDto = new ConcertSearchDto { SearchTerm = "Jazz" };
        var concerts = new List<Concert> { new() { Title = "Jazz Night" } };
        var concertSummaryDtos = new List<ConcertSummaryDto> { new() { Title = "Jazz Night" } };

        _concertRepositoryMock.Setup(r => r.GetFilteredAsync(It.IsAny<Expression<Func<Concert, bool>>[]>(), null, null))
            .ReturnsAsync(concerts);
        _mapperMock.Setup(m => m.Map<List<ConcertSummaryDto>>(concerts)).Returns(concertSummaryDtos);

        // Act
        var result = await _concertService.SearchAsync(searchDto);

        // Assert
        Assert.Single(result);
        Assert.Equal("Jazz Night", result[0].Title);
    }
}
