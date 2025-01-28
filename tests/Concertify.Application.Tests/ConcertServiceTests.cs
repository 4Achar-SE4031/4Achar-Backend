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
    private readonly Mock<IGenericRepository<Rating>> _ratingRepositoryMock;
    private readonly Mock<IGenericRepository<Bookmark>> _bookmarkRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly ConcertService _concertService;

    public ConcertServiceTests()
    {
        _concertRepositoryMock = new Mock<IGenericRepository<Concert>>();
        _ratingRepositoryMock = new Mock<IGenericRepository<Rating>>();
        _bookmarkRepositoryMock = new Mock<IGenericRepository<Bookmark>>();
        _mapperMock = new Mock<IMapper>();

        _concertService = new ConcertService(
            _concertRepositoryMock.Object,
            _ratingRepositoryMock.Object,
            _bookmarkRepositoryMock.Object,
            _mapperMock.Object
        );
    }

    [Fact]
    public async Task GetConcertByIdAsync_ShouldReturnConcertDetails_WhenConcertExists()
    {
        var concertId = 1;
        var userId = "user1";
        var newUser1 = new ApplicationUser { UserName = "newUser1", Email = "newuser1@example.com" };
        var newUser2 = new ApplicationUser { UserName = "newUser2", Email = "newuser2@example.com" };


        var concert = new Concert
        {
            Id = concertId,
            Title = "Concert 1",
            Ratings =
            [
                newUser1
            ],
            Bookmarks =
            [
                newUser2
            ]
        };

        var concertDetailsDto = new ConcertDetailsDto
        {
            Id = concertId,
            Title = "Concert 1",
            UserRating = 5,
            IsBookmarked = false
        };

        _concertRepositoryMock
            .Setup(repo => repo.GetByIdAsync(concertId, It.IsAny<Expression<Func<Concert, object>>[]>()))
            .ReturnsAsync(concert);

        _mapperMock.Setup(m => m.Map<ConcertDetailsDto>(concert)).Returns(concertDetailsDto);

        var result = await _concertService.GetConcertByIdAsync(concertId, userId);

        Assert.NotNull(result);
        Assert.Equal(concertDetailsDto.Id, result.Id);
        Assert.Equal(concertDetailsDto.Title, result.Title);
        Assert.Equal(concertDetailsDto.UserRating, result.UserRating);
        Assert.False(result.IsBookmarked);
    }

    [Fact]
    public async Task GetConcertByIdAsync_ShouldThrowException_WhenConcertNotFound()
    {
        var concertId = 999;

        _concertRepositoryMock
            .Setup(repo => repo.GetByIdAsync(concertId, It.IsAny<Expression<Func<Concert, object>>[]>()))
            .ReturnsAsync((Concert)null!);

        await Assert.ThrowsAsync<ItemNotFoundException>(() => _concertService.GetConcertByIdAsync(concertId, null));
    }

    [Fact]
    public async Task GetConcertsAsync_ShouldReturnFilteredConcerts()
    {
        var concertFilter = new ConcertFilterDto
        {
            Title = "Concert",
            Skip = 0,
            Take = 10
        };

        var concerts = new List<Concert>
        {
            new Concert { Id = 1, Title = "Concert A" },
            new Concert { Id = 2, Title = "Concert B" }
        };

        var concertSummaryDtos = new List<ConcertSummaryDto>
        {
            new ConcertSummaryDto { Id = 1, Title = "Concert A" },
            new ConcertSummaryDto { Id = 2, Title = "Concert B" }
        };


        _concertRepositoryMock
            .Setup(repo => repo.GetFilteredAsync(It.IsAny<Expression<Func<Concert, bool>>[]>(),
                                                 concertFilter.Skip,
                                                 concertFilter.Take,
                                                 It.IsAny<Expression<Func<Concert, object>>[]>()))
            .ReturnsAsync(concerts);

        
        _mapperMock.Setup(m => m.Map<List<ConcertSummaryDto>>(concerts)).Returns(concertSummaryDtos);

        var result = await _concertService.GetConcertsAsync(concertFilter);

        Assert.NotNull(result);
        Assert.Equal(2, result.Concerts.Count);
        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public async Task RateConcertAsync_ShouldAddRating_WhenNoExistingRatingFound()
    {
        var concertRating = new ConcertRatingDto
        {
            ConcertId = 1,
            UserId = "user1",
            Rating = 4
        };

        var concertRating2 = new ConcertRatingDto
        {
            ConcertId = 1,
            UserId = "user2",
            Rating = 3
        };

        var concert = new Concert { Id = concertRating.ConcertId };
        var rating1 = new Rating { ConcertId = concertRating.ConcertId, UserId = concertRating.UserId, Stars = concertRating.Rating };
        var rating2 = new Rating { ConcertId = concertRating2.ConcertId, UserId = concertRating2.UserId, Stars = concertRating2.Rating };

        List<Rating> ratings = [rating1, rating2];

        _concertRepositoryMock
            .Setup(repo => repo.GetByIdAsync(concertRating.ConcertId))
            .ReturnsAsync(concert);

        Expression<Func<Rating, bool>>[] filters = [
            r => r.ConcertId == concertRating.ConcertId 
            && r.UserId == concertRating.UserId
        ];


        _ratingRepositoryMock
       .Setup(repo => repo.GetFilteredAsync(It.Is<Expression<Func<Rating, bool>>[]>(f =>
           f.Length == 1 && f[0].Compile()(new Rating { ConcertId = concertRating.ConcertId, UserId = concertRating.UserId })
       ), null, null))
       .ReturnsAsync(new List<Rating>());


        Expression<Func<Rating, bool>>[] filters2 = [
            r => r.ConcertId == 1
        ];

        _ratingRepositoryMock
               .Setup(repo => repo.GetFilteredAsync(It.Is<Expression<Func<Rating, bool>>[]>(f =>
                   f.Length == 1 && f[0].Compile()(new Rating { ConcertId = concertRating.ConcertId })
               ), null, null))
               .ReturnsAsync(ratings);

        await _concertService.RateConcertAsync(concertRating);

        _ratingRepositoryMock.Verify(repo => repo.InsertAsync(It.Is<Rating>(r =>
            r.ConcertId == concertRating.ConcertId &&
            r.UserId == concertRating.UserId &&
            r.Stars == concertRating.Rating)), Times.Once);

        _ratingRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ToggleBookmarkAsync_ShouldAddBookmark_WhenNoBookmarkExists()
    {
        var concertId = 1;
        var userId = "user1";

        var concert = new Concert { Id = concertId };

        _concertRepositoryMock
            .Setup(repo => repo.GetByIdAsync(concertId))
            .ReturnsAsync(concert);

        _bookmarkRepositoryMock
            .Setup(repo => repo.GetFilteredAsync(It.IsAny<Expression<Func<Bookmark, bool>>[]>(), null, null))
            .ReturnsAsync(new List<Bookmark>());

        await _concertService.ToggleBookmarkAsync(concertId, userId);

        _bookmarkRepositoryMock.Verify(repo => repo.InsertAsync(It.Is<Bookmark>(b =>
            b.ConcertId == concertId &&
            b.UserId == userId)), Times.Once);

        _bookmarkRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }
}
