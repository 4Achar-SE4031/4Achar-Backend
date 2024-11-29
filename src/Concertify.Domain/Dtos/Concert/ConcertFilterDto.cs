namespace Concertify.Domain.Dtos.Concert;

public record ConcertFilterDto
{
    public string? Title { get; set; }
    public string? StartRange { get; set; }
    public string? EndRange { get; set; }
    public string? Province { get; set; }
    public string? City { get; set; }
    public string? Category { get; set; }
    public string? TicketPriceRangeStart { get; set; }
    public string? TicketPriceRangeEnd { get; set; }

    public int? Skip { get; set; }
    public int? Take { get; set; }
}
