using ForumAPI.Controllers;
using ForumApi.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace ForumAPI.Tests.Controllers
{
    [TestFixture]
    public class PostControllerTests
    {
        private Mock<IPostRepository> _postRepoMock;
        private PostService _postService;
        private PostController _controller;

        [SetUp]
        public void Setup()
        {
            _postRepoMock = new Mock<IPostRepository>();
            _postService = new PostService(_postRepoMock.Object);
            _controller = new PostController(_postService);
        }

        [Test]
        public async Task GetAll_ReturnsOkResults()
        {
            _postRepoMock
                .Setup(s => s.GetAllAsync())
                .ReturnsAsync(new List<Post>());

            var result = await _controller.GetAll();

            Assert.IsInstanceOf<OkObjectResult>(result.Result);

        }

        [Test]
        public async Task GetById_ExistingId_ReturnsOkResults()
        {
            var postId = "123";
            var post = new Post 
            {
                Id = postId,
                AuthorId = "author1",
                Title ="Test title",
                Body = "Test body"
            };

            _postRepoMock
                .Setup(s => s.GetByIdAsync(postId))
                .ReturnsAsync(post);
            
            var result = await _controller.GetById(postId);

            Assert.IsInstanceOf<OkObjectResult>(result.Result);
        }

        [Test]
        public async Task Create_ValidPost_ReturnsCreated()
        {
            var post = new Post
            {
                Id = "123",
                AuthorId = "author1",
                Title = "Test title",
                Body = "Test body"
            };

            _postRepoMock
                .Setup(r => r.CreateAsync(post))
                .Returns(Task.CompletedTask);

            var result = await _controller.Create(post);
            Assert.IsInstanceOf<CreatedAtActionResult>(result);
        }

        [Test]
        public async Task Create_ValidPost_CallsRepository()
        {
            var post = new Post
            {
                Id = "456",
                AuthorId = "author2",
                Title = "Test title2",
                Body = "Test body2"
            };

            await _controller.Create(post);

            _postRepoMock.Verify(
                r => r.CreateAsync(post),
                Times.Once
            );
        }

        [Test]
        public async Task Update_ExistingPost_ReturnsOk()
        {
              var post = new Post
            {
                Id = "123",
                AuthorId = "author1",
                Title = "Updated title",
                Body = "Updated body"
            };

            _postRepoMock
                .Setup(r => r.GetByIdAsync(post.Id))
                .ReturnsAsync(post);

            _postRepoMock
                .Setup(r => r.UpdateAsync(post))
                .Returns(Task.CompletedTask);

            var result = await _controller.Update(post);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task Update_NonExistingPost_ReturnsNotFound()
        {
              var post = new Post
            {
                Id = "999",
                AuthorId = "author1",
                Title = "Title",
                Body = "Body"
            };

            _postRepoMock
                .Setup(r => r.GetByIdAsync(post.Id))
                .ReturnsAsync((Post)null);

            var result = await _controller.Update(post);
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task Update_InvalidPost_ReturnsBadRequest()
        {
            var result = await _controller.Update(null);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task Delete_ExistingPost_ReturnsNoContent()
        {
            var postId = "123";
            var post = new Post 
            {
                Id = postId,
                AuthorId = "author1",
                Title ="Title",
                Body = "Body"
            };

            _postRepoMock
                .Setup(r => r.GetByIdAsync(postId))
                .ReturnsAsync(post);

            _postRepoMock
                .Setup(r => r.DeleteAsync(post))
                .Returns(Task.CompletedTask);
            
            var result = await _controller.Delete(postId);

            Assert.IsInstanceOf<NoContentResult>(result);
        }

        [Test]
        public async Task Delete_NonExistingPost_ReturnsNotFound()
        {
              var postId = "999";
            
            _postRepoMock
                .Setup(r => r.GetByIdAsync(postId))
                .ReturnsAsync((Post)null);

            var result = await _controller.Delete(postId);
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task LikePost_ValidUser_ReturnsOk()
        {
            var postId = "123";
            var userId = "user1";

            _postRepoMock
                .Setup(r => r.GetByIdAsync(postId))
                .ReturnsAsync( new Post 
                {
                    Id = postId,
                    AuthorId = "author1",
                    Title ="Title",
                    Body = "Body"
                });

            

            _postRepoMock
                .Setup(r => r.UpdateAsync(It.IsAny<Post>()))
                .Returns(Task.CompletedTask);
            
            var result = await _controller.LikePost(postId, userId);

            Assert.IsInstanceOf<OkResult>(result);
        }

        [Test]
        public async Task LikePost_MissingUser_ReturnsBadRequest()
        {
            var result = await _controller.LikePost("123","");
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task LikePost_NonExistingPost_ReturnsNotFound()
        {
              var postId = "999";
              var userId = "user1";
            
            _postRepoMock
                .Setup(r => r.GetByIdAsync(postId))
                .ReturnsAsync((Post)null);

            var result = await _controller.LikePost(postId, userId);
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

    }
}