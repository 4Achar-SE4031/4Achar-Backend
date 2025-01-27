namespace Concertify.Domain.Models;

public class Bookmark : EntityBase
{
    public string? UserId { get; set; }
    public int ConcertId { get; set; }
}
