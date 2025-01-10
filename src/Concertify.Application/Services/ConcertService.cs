using System.Linq.Expressions;

using AutoMapper;

using Concertify.Domain.Dtos.Concert;
using Concertify.Domain.Exceptions;
using Concertify.Domain.Interfaces;
using Concertify.Domain.Models;

using Microsoft.EntityFrameworkCore;

namespace Concertify.Application.Services;

public class ConcertService(IGenericRepository<Concert> concertRepository, IMapper mapper) : IConcertService
{
    private readonly IGenericRepository<Concert> _concertRepository = concertRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<ConcertDetailsDto> GetConcertByIdAsync(int concertId)
    {
        Concert entity = await _concertRepository.GetByIdAsync(concertId)
            ?? throw new ItemNotFoundException(concertId);

        ConcertDetailsDto concert = _mapper.Map<ConcertDetailsDto>(entity);

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

        List<Concert> concerts = await _concertRepository.GetFilteredAsync(filters, concertFilterDto.Skip, concertFilterDto.Take);
        
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
}
