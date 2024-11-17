using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Challenge.Application.Interfaces;
using Challenge.UI.Controllers;

namespace Challenge.Tests.UITests
{
    public class JobControllerTests
    {
        private readonly Mock<IShowService> _mockShowService;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly JobController _controller;

        public JobControllerTests()
        {
            _mockShowService = new Mock<IShowService>();
            _mockConfiguration = new Mock<IConfiguration>();
            _controller = new JobController(_mockShowService.Object, _mockConfiguration.Object);
        }

        [Fact]
        public async Task RunJob_ReturnsUnauthorized_WhenApiKeyIsInvalid()
        {
            // Arrange
            _mockConfiguration.Setup(c => c["ApiKey"]).Returns("valid-api-key");
            var invalidApiKey = "invalid-api-key";

            // Act
            var result = await _controller.RunJob(invalidApiKey);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Invalid API key.", unauthorizedResult.Value);
        }

        [Fact]
        public async Task RunJob_ReturnsOk_WhenApiKeyIsValid()
        {
            // Arrange
            _mockConfiguration.Setup(c => c["ApiKey"]).Returns("valid-api-key");
            _mockShowService.Setup(s => s.FetchAndStoreShowsAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.RunJob("valid-api-key");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Job executed successfully.", okResult.Value);
            _mockShowService.Verify(s => s.FetchAndStoreShowsAsync(), Times.Once);
        }

        [Fact]
        public async Task RunJob_ReturnsInternalServerError_OnException()
        {
            // Arrange
            var apiKey = "valid-api-key";
            _mockConfiguration.Setup(c => c["ApiKey"]).Returns(apiKey);
            _mockShowService
                .Setup(s => s.FetchAndStoreShowsAsync())
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.RunJob(apiKey);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);

            // The Value might be an error message or an error object; adjust accordingly
            var expectedMessage = "An error occurred while executing the job.";
            Assert.Equal(expectedMessage, objectResult.Value);
        }

    }
}
