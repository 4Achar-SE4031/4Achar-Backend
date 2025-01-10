namespace Concertify.Domain.Dtos.Concert;

public record ConcertFilterDto
{
    public string? Title { get; set; }
    public DateTime? StartRange { get; set; }
    public DateTime? EndRange { get; set; }
    public string? Province { get; set; }
    public string? City { get; set; }
    public string? Category { get; set; }
    public int? TicketPriceRangeStart { get; set; }
    public int? TicketPriceRangeEnd { get; set; }

    public int? Skip { get; set; }
    public int? Take { get; set; }
}
