namespace Concertify.Domain.Dtos.Concert;

public record ConcertCreateDto
{
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public DateTime StartDate { get; set; }
    public string? Province { get; set; }
    public string? City { get; set; }
    public string Address { get; set; } = default!;
    public string Category { get; set; } = default!;
    public string TicketPrice { get; set; } = default!;

}
