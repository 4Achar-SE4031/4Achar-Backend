namespace Concertify.Infrastructure.Dtos;

public record ScraperContext
{
    public string Url { get; init; } = default!;
    public string City { get; init; } = default!;
    public string CardImage { get; init; } = default!;
}
