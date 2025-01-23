namespace Concertify.Domain.Dtos
{
    /// <summary>
    /// DTO for toggling likes/upvotes on a comment
    /// </summary>
    public class ToggleLikeDto
    {
        // e.g. you might pass something like { "like": true } to explicitly set state,
        // but typically a "toggle" doesn't need a body. 
        public bool Like { get; set; }
    }
}
