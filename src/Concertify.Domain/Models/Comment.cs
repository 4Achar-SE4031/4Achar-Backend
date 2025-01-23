using System;
using System.Collections.Generic;

namespace Concertify.Domain.Models
{
    public class Comment : EntityBase
    {
        public string Text { get; set; } = default!;
        public DateTime CreatedAt { get; set; } 
        public int Score { get; set; }
        public string UserId { get; set; } = default!;
        public ApplicationUser User { get; set; } = default!; 
        public int EventId { get; set; }
        public Concert Event { get; set; } = default!;
        public int? ParentId { get; set; }
        public Comment Parent { get; set; } = default!;
        public ICollection<Comment> Replies { get; set; }
        public ICollection<ApplicationUser> LikedBy { get; set; }

        public Comment()
        {
            Replies = new List<Comment>();
            LikedBy = new List<ApplicationUser>();
        }
    }
}
