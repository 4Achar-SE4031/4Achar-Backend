namespace Concertify.Domain.Dtos.Concert;

public record ConcertDetailsDto
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string StartDate { get; set; } = default!;
    public string? Province { get; set; }
    public string? City { get; set; }
    public string Address { get; set; } = default!;
    public string Category { get; set; } = default!;
    public string TicketPrice { get; set; } = default!;
    public float Latitude {  get; set; } = default!;
    public float Longitude { get; set; } = default!;
}
