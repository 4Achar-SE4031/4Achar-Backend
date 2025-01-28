using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Concertify.API.Controllers;
using Concertify.Domain.Dtos;
using Concertify.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Concertify.API.Tests
{
    public class CommentsControllerTests
    {
        private readonly Mock<ICommentService> _mockCommentService;
        private readonly CommentsController _controller;

        public CommentsControllerTests()
        {
            _mockCommentService = new Mock<ICommentService>();
            _controller = new CommentsController(_mockCommentService.Object);
             _controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity()) // Non-null, empty ClaimsPrincipal
                    }
                };
        }

        private void SetUserContext(string userId)
        {
            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaims }
            };
        }

        [Fact]
        public async Task GetCommentsForEvent_ShouldReturnOk_WithComments()
        {
            // Arrange
            int eventId = 123;
            string currentUserId = "user-1";
            SetUserContext(currentUserId);

            var mockComments = new List<CommentDto>
            {
                new CommentDto { Id = 1, Text = "Comment 1", UserId = "user-1" },
                new CommentDto { Id = 2, Text = "Comment 2", UserId = "user-2" },
            };

            _mockCommentService
                .Setup(service => service.GetCommentsForEventAsync(eventId, currentUserId))
                .ReturnsAsync(mockComments);

            // Act
            var result = await _controller.GetCommentsForEvent(eventId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;

            // Use Reflection to get the 'comments' property
            var commentsProperty = value.GetType().GetProperty("comments");
            Assert.NotNull(commentsProperty);

            var commentsList = commentsProperty.GetValue(value) as List<CommentDto>;
            Assert.NotNull(commentsList);
            Assert.Equal(2, commentsList.Count);
        }

        [Fact]
        public async Task CreateComment_ShouldReturnOk_WhenValid()
        {
            // Arrange
            SetUserContext("user-1");
            var createCommentDto = new CreateCommentDto
            {
                EventId = 123,
                Text = "Valid Comment",
                ParentId = null
            };

            var createdCommentDto = new CommentDto
            {
                Id = 1,
                Text = "Valid Comment",
                UserId = "user-1"
            };

            _mockCommentService
                .Setup(service => service.CreateCommentAsync(createCommentDto, "user-1"))
                .ReturnsAsync(createdCommentDto);

            // Act
            var result = await _controller.CreateComment(createCommentDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedComment = Assert.IsType<CommentDto>(okResult.Value);
            Assert.Equal("Valid Comment", returnedComment.Text);
            Assert.Equal("user-1", returnedComment.UserId);
        }

        [Fact]
        public async Task CreateComment_ShouldReturnBadRequest_WhenTextIsEmpty()
        {
            // Arrange
            SetUserContext("user-1");
            var createCommentDto = new CreateCommentDto
            {
                EventId = 123,
                Text = "",
                ParentId = null
            };

            // Act
            var result = await _controller.CreateComment(createCommentDto);

            // Assert
            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Comment text cannot be empty.", badResult.Value);
        }

        [Fact]
        public async Task CreateComment_ShouldReturnUnauthorized_WhenNoUser()
        {
            // Arrange
            // Not calling SetUserContext => no user principal
            var createCommentDto = new CreateCommentDto
            {
                EventId = 123,
                Text = "Some text",
                ParentId = null
            };

            // Act
            var result = await _controller.CreateComment(createCommentDto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("No valid user logged in.", unauthorizedResult.Value);
        }

        [Fact]
        public async Task DeleteComment_ShouldReturnNoContent_WhenSuccessful()
        {
            // Arrange
            SetUserContext("user-1");
            var commentId = 1;

            _mockCommentService
                .Setup(service => service.DeleteCommentAsync(commentId, "user-1"))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteComment(commentId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteComment_ShouldReturnNotFound_WhenCommentDoesNotExist()
        {
            // Arrange
            SetUserContext("user-1");
            var commentId = 999;

            // We need .ThrowsAsync(...) for an async method.
            _mockCommentService
                .Setup(service => service.DeleteCommentAsync(commentId, "user-1"))
                .ThrowsAsync(new KeyNotFoundException("Comment not found."));

            // Act
            var result = await _controller.DeleteComment(commentId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Comment not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task DeleteComment_ShouldReturnForbid_WhenUnauthorizedAccess()
        {
            // Arrange
            SetUserContext("user-1");
            var commentId = 1;

            _mockCommentService
                .Setup(service => service.DeleteCommentAsync(commentId, "user-1"))
                .ThrowsAsync(new UnauthorizedAccessException("You are not authorized to delete this comment."));

            // Act
            var result = await _controller.DeleteComment(commentId);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task UpdateComment_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            SetUserContext("user-1");
            var commentId = 1;
            var updateDto = new UpdateCommentDto { NewText = "Updated Text" };
            var updatedComment = new CommentDto
            {
                Id = 1,
                Text = "Updated Text",
                UserId = "user-1"
            };

            _mockCommentService
                .Setup(service => service.UpdateCommentAsync(commentId, updateDto, "user-1"))
                .ReturnsAsync(updatedComment);

            // Act
            var result = await _controller.UpdateComment(commentId, updateDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedComment = Assert.IsType<CommentDto>(okResult.Value);
            Assert.Equal("Updated Text", returnedComment.Text);
        }

        [Fact]
        public async Task UpdateComment_ShouldReturnNotFound_WhenNoCommentFound()
        {
            // Arrange
            SetUserContext("user-1");
            var commentId = 999;
            var updateDto = new UpdateCommentDto { NewText = "Text" };

            _mockCommentService
                .Setup(service => service.UpdateCommentAsync(commentId, updateDto, "user-1"))
                .ThrowsAsync(new KeyNotFoundException("Comment not found."));

            // Act
            var result = await _controller.UpdateComment(commentId, updateDto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Comment not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task ToggleLike_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            SetUserContext("user-1");
            var commentId = 1;
            var updatedComment = new CommentDto
            {
                Id = commentId,
                Text = "Some text",
                UserId = "user-1",
                Score = 1,
                HasLiked = true
            };

            _mockCommentService
                .Setup(service => service.ToggleLikeAsync(commentId, "user-1"))
                .ReturnsAsync(updatedComment);

            // Act
            var result = await _controller.ToggleLike(commentId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedComment = Assert.IsType<CommentDto>(okResult.Value);
            Assert.Equal(commentId, returnedComment.Id);
            Assert.Equal(1, returnedComment.Score);
            Assert.True(returnedComment.HasLiked);
        }

        [Fact]
        public async Task ToggleLike_ShouldReturnNotFound_WhenCommentNotFound()
        {
            // Arrange
            SetUserContext("user-1");
            var commentId = 999;

            _mockCommentService
                .Setup(service => service.ToggleLikeAsync(commentId, "user-1"))
                .ThrowsAsync(new KeyNotFoundException("Comment not found."));

            // Act
            var result = await _controller.ToggleLike(commentId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Comment not found.", notFoundResult.Value);
        }
    }
}
