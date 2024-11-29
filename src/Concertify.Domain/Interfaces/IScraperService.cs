using Concertify.Domain.Dtos.Concert;

namespace Concertify.Domain.Interfaces;

public interface IScraperService
{
    public Task<List<ConcertSummaryDto>> Collect();
}
