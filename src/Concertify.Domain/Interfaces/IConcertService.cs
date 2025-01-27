using Concertify.Domain.Dtos.Concert;

namespace Concertify.Domain.Interfaces;

public interface IConcertService
{
    public Task<ConcertListDto> GetConcertsAsync(ConcertFilterDto concertFilterDto);
    public Task<ConcertDetailsDto> GetConcertByIdAsync(int concertId, string? userId);
    public Task<List<ConcertSummaryDto>> SearchAsync(ConcertSearchDto concertSearch);
    public Task RateConcertAsync(ConcertRatingDto concertRating);
    public Task<float> GetAverageRatingAsync(int concertId);
    public Task ToggleBookmarkAsync(int concertId, string? userId);

}
