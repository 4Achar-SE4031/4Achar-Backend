using Concertify.Application.Services;
using Concertify.Domain.Dtos;
using Concertify.Domain.Models;
using Concertify.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Concertify.API.Tests
{
    public class CommentsServiceTests
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly CommentService _commentService;

        public CommentsServiceTests()
        {
            // Ensure you have Microsoft.EntityFrameworkCore.InMemory installed
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Requires using Microsoft.EntityFrameworkCore
                .Options;

            _dbContext = new ApplicationDbContext(options);
            _dbContext.Database.EnsureCreated();

            _commentService = new CommentService(_dbContext);

            SeedData();
        }

        private void SeedData()
        {
            // Add some initial users
            _dbContext.Users.AddRange(
                new ApplicationUser
                {
                    Id = "user-1",
                    UserName = "User1"
                },
                new ApplicationUser
                {
                    Id = "user-2",
                    UserName = "User2"
                }
            );

            // Add some initial comments
            _dbContext.Comments.AddRange(
                new Comment
                {
                    Id = 1,
                    Text = "Parent Comment",
                    UserId = "user-1",
                    EventId = 100,
                    CreatedAt = DateTime.UtcNow
                },
                new Comment
                {
                    Id = 2,
                    Text = "Child Comment",
                    UserId = "user-2",
                    EventId = 100,
                    ParentId = 1,
                    CreatedAt = DateTime.UtcNow
                }
            );

            _dbContext.SaveChanges();
        }

        [Fact]
        public async Task GetCommentsForEventAsync_ShouldReturnAllComments()
        {
            // Arrange
            int eventId = 100;
            string currentUserId = "user-1";

            // Act
            var result = await _commentService.GetCommentsForEventAsync(eventId, currentUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);  // we have 2 comments in seed
        }

        [Fact]
        public async Task CreateCommentAsync_ShouldCreateAndReturnNewComment()
        {
            // Arrange
            var dto = new CreateCommentDto
            {
                EventId = 200,
                Text = "Newly Created",
                ParentId = null
            };
            string userId = "user-1";

            // Act
            var createdComment = await _commentService.CreateCommentAsync(dto, userId);

            // Assert
            Assert.NotNull(createdComment);
            Assert.Equal("Newly Created", createdComment.Text);

            // Ensure it's in database
            var commentInDb = _dbContext.Comments.FirstOrDefault(c => c.Id == createdComment.Id);
            Assert.NotNull(commentInDb);
            Assert.Equal("user-1", commentInDb!.UserId);
        }

        [Fact]
        public async Task CreateCommentAsync_ShouldThrow_WhenTextIsEmpty()
        {
            // Arrange
            var dto = new CreateCommentDto
            {
                EventId = 100,
                Text = ""
            };
            string userId = "user-1";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _commentService.CreateCommentAsync(dto, userId));
        }

        [Fact]
        public async Task DeleteCommentAsync_ShouldDelete_WhenAuthorized()
        {
            // Arrange
            int commentId = 1; // belongs to user-1
            string userId = "user-1";

            // Act
            await _commentService.DeleteCommentAsync(commentId, userId);

            // Assert
            var commentInDb = await _dbContext.Comments.FindAsync(commentId);
            Assert.Null(commentInDb); // should be deleted
        }

        [Fact]
        public async Task DeleteCommentAsync_ShouldThrowKeyNotFound_WhenCommentNotExists()
        {
            // Arrange
            int commentId = 999;
            string userId = "user-1";

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _commentService.DeleteCommentAsync(commentId, userId));
        }

        [Fact]
        public async Task DeleteCommentAsync_ShouldThrowUnauthorized_WhenWrongUser()
        {
            // Arrange
            int commentId = 1;      // belongs to user-1
            string userId = "user-2"; // not the owner

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _commentService.DeleteCommentAsync(commentId, userId));
        }

        [Fact]
        public async Task UpdateCommentAsync_ShouldUpdate_WhenAuthorized()
        {
            // Arrange
            int commentId = 1;        // belongs to user-1
            string userId = "user-1";
            var updateDto = new UpdateCommentDto { NewText = "Updated Text" };

            // Act
            var updatedComment = await _commentService.UpdateCommentAsync(commentId, updateDto, userId);

            // Assert
            Assert.Equal("Updated Text", updatedComment.Text);

            // check DB
            var commentInDb = await _dbContext.Comments.FindAsync(commentId);
            Assert.NotNull(commentInDb);
            Assert.Equal("Updated Text", commentInDb!.Text);
        }

        [Fact]
        public async Task UpdateCommentAsync_ShouldThrowKeyNotFound_WhenCommentNotExists()
        {
            // Arrange
            int commentId = 999;
            var updateDto = new UpdateCommentDto { NewText = "New text" };
            string userId = "user-1";

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _commentService.UpdateCommentAsync(commentId, updateDto, userId));
        }

        [Fact]
        public async Task UpdateCommentAsync_ShouldThrowUnauthorized_WhenWrongUser()
        {
            // Arrange
            int commentId = 1; // belongs to user-1
            var updateDto = new UpdateCommentDto { NewText = "New text" };
            string userId = "user-2";

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _commentService.UpdateCommentAsync(commentId, updateDto, userId));
        }

        [Fact]
        public async Task ToggleLikeAsync_ShouldAddLike_WhenUserHasNotLikedBefore()
        {
            // Arrange
            int commentId = 1;            // belongs to user-1
            string currentUserId = "user-2"; 
            var user2 = await _dbContext.Users.FindAsync("user-2");
            var comment1 = await _dbContext.Comments.FindAsync(commentId);

            // Make sure they're not null
            Assert.NotNull(user2);
            Assert.NotNull(comment1);

            // user-2 has not yet liked commentId=1
            // Act
            var updatedComment = await _commentService.ToggleLikeAsync(commentId, currentUserId);

            // Assert
            Assert.True(updatedComment.HasLiked);
            Assert.Equal(1, updatedComment.Score);

            // Check DB
            var updatedCommentInDb = await _dbContext.Comments.FindAsync(commentId);
            Assert.NotNull(updatedCommentInDb);
            Assert.Equal(1, updatedCommentInDb!.Score);
            Assert.Contains(updatedCommentInDb.LikedBy, u => u.Id == "user-2");
        }

        [Fact]
        public async Task ToggleLikeAsync_ShouldRemoveLike_WhenUserAlreadyLiked()
        {
            // Arrange
            int commentId = 1;
            // Manually simulate that user-2 has already liked the comment
            var user2 = await _dbContext.Users.FindAsync("user-2");
            var comment1 = await _dbContext.Comments.FindAsync(commentId);

            Assert.NotNull(user2);
            Assert.NotNull(comment1);

            comment1!.LikedBy = new List<ApplicationUser> { user2! };
            comment1.Score = 1;
            await _dbContext.SaveChangesAsync();

            // Act
            var updatedComment = await _commentService.ToggleLikeAsync(commentId, "user-2");

            // Assert
            Assert.False(updatedComment.HasLiked);
            Assert.Equal(0, updatedComment.Score);

            // Check DB
            var commentInDb = await _dbContext.Comments.FindAsync(commentId);
            Assert.NotNull(commentInDb);
            Assert.Equal(0, commentInDb!.Score);
            Assert.DoesNotContain(commentInDb.LikedBy, u => u.Id == "user-2");
        }

        [Fact]
        public async Task ToggleLikeAsync_ShouldThrowKeyNotFound_WhenCommentNotExist()
        {
            // Arrange
            int commentId = 999;
            string currentUserId = "user-1";

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _commentService.ToggleLikeAsync(commentId, currentUserId));
        }
    }
}
