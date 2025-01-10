using Concertify.Domain.Dtos.Concert;

namespace Concertify.Domain.Interfaces;

public interface IConcertService
{
    public Task<ConcertListDto> GetConcertsAsync(ConcertFilterDto concertFilterDto);
    public Task<ConcertDetailsDto> GetConcertByIdAsync(int concertId);
    public Task<List<ConcertSummaryDto>> SearchAsync(ConcertSearchDto concertSearch);

}
