
namespace Concertify.Domain.Models;

public class Rating : EntityBase
{
    public string? UserId { get; set; }
    public int ConcertId { get; set; }
    public float Stars { get; set; }
}
