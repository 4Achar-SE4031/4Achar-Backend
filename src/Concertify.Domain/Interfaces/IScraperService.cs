using Concertify.Domain.Dtos.Concert;

namespace Concertify.Domain.Interfaces;

public interface IScraperService
{
    public IAsyncEnumerable<ConcertSummaryDto> Collect();
}
