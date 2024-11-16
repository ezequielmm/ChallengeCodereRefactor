using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Challenge.Application.Interfaces;
using Challenge.Application.DTOs;
using Challenge.Domain.Entities;
using Challenge.UI.Controllers;
using Challenge.Infrastructure.Data;

namespace Challenge.Tests.UITests
{
    public class ShowsControllerTests
    {
        private readonly Mock<IShowService> _mockShowService;
        private readonly Mock<ApplicationDbContext> _mockContext;
        private readonly ShowsController _controller;

        public ShowsControllerTests()
        {
            _mockShowService = new Mock<IShowService>();
            _mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            _controller = new ShowsController(_mockShowService.Object, _mockContext.Object);
        }

        [Fact]
        public async Task GetAllShows_ReturnsOk_WithListOfShows()
        {
            // Arrange
            var shows = new List<Show>
            {
                new Show { Id = 1, Name = "Show 1" },
                new Show { Id = 2, Name = "Show 2" }
            };
            _mockShowService.Setup(s => s.GetAllShowsAsync()).ReturnsAsync(shows);

            // Act
            var result = await _controller.GetAllShows();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnShows = Assert.IsType<List<Show>>(okResult.Value);
            Assert.Equal(2, returnShows.Count);
        }

        [Fact]
        public async Task GetShowById_ReturnsNotFound_WhenShowDoesNotExist()
        {
            // Arrange
            _mockShowService.Setup(s => s.GetShowByIdAsync(It.IsAny<int>())).ReturnsAsync((Show)null);

            // Act
            var result = await _controller.GetShowById(1);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateShow_ReturnsBadRequest_WhenShowDtoIsNull()
        {
            // Act
            var result = await _controller.CreateShow(null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Los datos del show no pueden ser nulos.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateShow_ReturnsNotFound_WhenShowDoesNotExist()
        {
            // Arrange
            var updateDto = new UpdateShowDto { Name = "Updated Show" };
            _mockShowService.Setup(s => s.GetShowByIdAsync(It.IsAny<int>())).ReturnsAsync((Show)null);

            // Act
            var result = await _controller.UpdateShow(1, updateDto);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteShow_ReturnsNoContent_WhenShowExists()
        {
            // Arrange
            var existingShow = new Show { Id = 1, Name = "Test Show" };
            _mockShowService.Setup(s => s.GetShowByIdAsync(1)).ReturnsAsync(existingShow);

            // Act
            var result = await _controller.DeleteShow(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockShowService.Verify(s => s.DeleteShowAsync(1), Times.Once);
        }
    }
}
