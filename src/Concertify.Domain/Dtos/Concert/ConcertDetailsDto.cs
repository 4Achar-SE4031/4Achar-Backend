namespace Concertify.Domain.Dtos.Concert;

public record ConcertDetailsDto
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public DateTime StartDateTime { get; set; } = default!;
    public string? City { get; set; }
    public string Address { get; set; } = default!;
    public string Location { get; set; } = default!;
    public string Category { get; set; } = default!;
    public List<int> TicketPrice { get; set; } = default!;
    public float Latitude {  get; set; } = default!;
    public float Longtitude { get; set; } = default!;
    public string CoverImage { get; set; } = default!;
    public string Url { get; set; } = default!;
}
