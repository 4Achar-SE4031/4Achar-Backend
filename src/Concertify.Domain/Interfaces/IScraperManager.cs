using Concertify.Domain.Models;

namespace Concertify.Domain.Interfaces;

public interface IScraperManager
{
    public Task<List<Concert>> StartScraping(string url);
}
