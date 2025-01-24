namespace Concertify.Domain.Dtos.Concert;

public record ConcertSummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public DateTime StartDateTime { get; set; } = default!;
    public List<int> TicketPrice { get; set; } = default!;
    public string? City { get; set; }
    public string Location { get; set; } = default!;
    public string Category { get; set; } = default!;
    public string CardImage { get; set; } = default!;
    public float AverageRating { get; set; } = default!;

}
