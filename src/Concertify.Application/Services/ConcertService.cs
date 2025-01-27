using System.Linq.Expressions;

using AutoMapper;

using Concertify.Domain.Dtos.Concert;
using Concertify.Domain.Exceptions;
using Concertify.Domain.Interfaces;
using Concertify.Domain.Models;

using Microsoft.EntityFrameworkCore;

namespace Concertify.Application.Services;

public class ConcertService(
    IGenericRepository<Concert> concertRepository, 
    IGenericRepository<Rating> ratingRepository, 
    IGenericRepository<Bookmark> bookmarkRepository, 
    IMapper mapper
    ) : IConcertService
{
    private readonly IGenericRepository<Concert> _concertRepository = concertRepository;
    private readonly IGenericRepository<Rating> _ratingRepository = ratingRepository;
    private readonly IGenericRepository<Bookmark> _bookmarkRepository = bookmarkRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<ConcertDetailsDto> GetConcertByIdAsync(int concertId, string? userId)
    {
        
        Concert entity = await _concertRepository.GetByIdAsync(concertId, c => c.Ratings, c => c.Bookmarks)
            ?? throw new ItemNotFoundException(concertId);


        ConcertDetailsDto concert = _mapper.Map<ConcertDetailsDto>(entity);
        if (userId != null && entity.Ratings.Any(r => r.Id == userId))
        {
            concert.UserRating = (await _ratingRepository.GetFilteredAsync([r => r.UserId == userId, r => r.ConcertId == concertId], null, null)).First().Stars;

        }
        
        if (userId != null)
        {
            concert.IsBookmarked = entity.Bookmarks.Any(b => b.Id == userId);
        }
        return concert;

    }

    public async Task<ConcertListDto> GetConcertsAsync(ConcertFilterDto concertFilterDto)
    {
        Expression<Func<Concert, bool>>[] filters =
        [
            c => concertFilterDto.Title == null || c.Title.StartsWith(concertFilterDto.Title),
            c => concertFilterDto.StartRange == null || DateTime.Compare(c.StartDateTime, concertFilterDto.StartRange.Value) >= 0,
            c => concertFilterDto.EndRange == null || DateTime.Compare(c.StartDateTime, concertFilterDto.EndRange.Value) <= 0,
            c => concertFilterDto.City == null || (c.City != null && c.City.StartsWith(concertFilterDto.City)),
            c => concertFilterDto.Category == null || c.Category.StartsWith(concertFilterDto.Category),
            c => concertFilterDto.TicketPriceRangeStart == null || c.TicketPrice.Contains(concertFilterDto.TicketPriceRangeStart.Value),
            c => concertFilterDto.TicketPriceRangeEnd == null || c.TicketPrice.Contains(concertFilterDto.TicketPriceRangeEnd.Value)
        ];

        Expression<Func<Concert, object>>[] includes = [c => c.Ratings];

        List<Concert> concerts = await _concertRepository.GetFilteredAsync(filters, concertFilterDto.Skip, concertFilterDto.Take, includes);
        
        int totalCount = concerts.Count;

        if (concertFilterDto.Skip.HasValue)
            concerts = concerts.Skip(concertFilterDto.Skip.Value).ToList();
        if (concertFilterDto.Take.HasValue)
            concerts = concerts.Take(concertFilterDto.Take.Value).ToList();

        List<ConcertSummaryDto> concertDtos = _mapper.Map<List<ConcertSummaryDto>>(concerts);

        return new ConcertListDto()
        {
            Concerts = concertDtos,
            TotalCount = totalCount
        };
    }   

    public async Task<List<ConcertSummaryDto>> SearchAsync(ConcertSearchDto concertSearch)
    {
        Expression<Func<Concert, bool>>[] filters =
        [
            c => c.Title.Contains(concertSearch.SearchTerm),
        ];

        List<Concert> concerts = await _concertRepository.GetFilteredAsync(filters, null, null);
        List<ConcertSummaryDto> concertDtos = _mapper.Map<List<ConcertSummaryDto>>(concerts);

        return concertDtos;
    }

    public async Task RateConcertAsync(ConcertRatingDto concertRating)
    {
        Concert concert = await _concertRepository.GetByIdAsync(concertRating.ConcertId)
            ?? throw new ItemNotFoundException(concertRating.ConcertId);

        Rating? rating = (await _ratingRepository.GetFilteredAsync([
            r => r.ConcertId == concertRating.ConcertId
            && r.UserId == concertRating.UserId], null, null))
            .FirstOrDefault();
            

        if (rating == null)
        {

            Rating newRating = new()
            {
                ConcertId = concertRating.ConcertId,
                Stars = concertRating.Rating,
                UserId = concertRating.UserId
            };
            await _ratingRepository.InsertAsync(newRating);
        }
        else
        {
            rating.Stars = concertRating.Rating;
            _ratingRepository.Update(rating);
        }

        await _ratingRepository.SaveChangesAsync();
        
        concert.AverageRating = await GetAverageRatingAsync(concertRating.ConcertId);
        _concertRepository.Update(concert);
        await _concertRepository.SaveChangesAsync();
    }

    public async Task ToggleBookmarkAsync(int concertId, string? userId)
    {
        Concert concert = await _concertRepository.GetByIdAsync(concertId)
            ?? throw new ItemNotFoundException(concertId);

        Bookmark? bookmark = (await _bookmarkRepository.GetFilteredAsync([
            b => b.ConcertId == concertId
            && b.UserId == userId], null, null))
            .FirstOrDefault();

        if (bookmark == null)
        {
            Bookmark newBookmark = new()
            {
                ConcertId = concertId,
                UserId = userId
            };
            await _bookmarkRepository.InsertAsync(newBookmark);
        }
        else
        {
            _bookmarkRepository.Delete(bookmark);
        }

        await _bookmarkRepository.SaveChangesAsync();
    }

    public async Task<float> GetAverageRatingAsync(int concertId)
    {
        float averageRating = (await _ratingRepository.GetFilteredAsync([r => r.ConcertId == concertId], null, null)).Average(r => r.Stars);
        return averageRating;
    }
}
