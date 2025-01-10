namespace Concertify.Domain.Dtos.Concert;

public record ConcertListDto
{
    public List<ConcertSummaryDto> Concerts { get; set; } = default!;
    public int TotalCount { get; set; } = default;
}
