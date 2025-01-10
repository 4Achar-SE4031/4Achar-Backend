using Concertify.Domain.Models;

namespace Concertify.Domain.Interfaces;

public interface IScraperManager
{
    public IAsyncEnumerable<Concert> StartScraping(string url);
}
