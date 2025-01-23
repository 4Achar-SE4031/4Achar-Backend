using System;
using System.Collections.Generic;

namespace Concertify.Domain.Dtos
{
    /// <summary>
    /// Represents a single comment (or reply) for output.
    /// Nested "Replies" is used for child comments.
    /// </summary>
    public class CommentDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public int Score { get; set; }
        public string UserId { get; set; } = default!;
        public string Username { get; set; } = default!;
        public bool HasLiked { get; set; }
        public int? ParentId { get; set; }
        public int EventId { get; set; }
        public List<CommentDto> Replies { get; set; } = new();
        public int? ReplyingTo { get; set; }
        public string? ReplyingToName { get; set; }
    }
}
