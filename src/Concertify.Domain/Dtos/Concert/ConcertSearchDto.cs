namespace Concertify.Domain.Dtos.Concert;

public record ConcertSearchDto
{
    public string SearchTerm { get; set; } = default!;
}
