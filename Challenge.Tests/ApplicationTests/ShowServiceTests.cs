using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Challenge.Application.DTOs;
using Challenge.Application.Services;
using Challenge.Domain.Entities;
using Challenge.Domain.Repositories.Interfaces;
using Challenge.Infrastructure.Data;
using Challenge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Xunit;

namespace Challenge.Application.Tests
{
    public class ShowServiceTests
    {
        private readonly Mock<IShowRepository> _showRepositoryMock;
        private readonly ApplicationDbContext _context;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly ShowService _showService;

        public ShowServiceTests()
        {
            _showRepositoryMock = new Mock<IShowRepository>();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.Setup(c => c["ApiUrl"]).Returns("http://fakeapi.com/");

            _showService = new ShowService(
                _showRepositoryMock.Object,
                _context,
                _httpClientFactoryMock.Object,
                _configurationMock.Object
            );
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenShowRepositoryIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ShowService(
                null,
                _context,
                _httpClientFactoryMock.Object,
                _configurationMock.Object
            ));
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenContextIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ShowService(
                _showRepositoryMock.Object,
                null,
                _httpClientFactoryMock.Object,
                _configurationMock.Object
            ));
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenHttpClientFactoryIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ShowService(
                _showRepositoryMock.Object,
                _context,
                null,
                _configurationMock.Object
            ));
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenConfigurationIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ShowService(
                _showRepositoryMock.Object,
                _context,
                _httpClientFactoryMock.Object,
                null
            ));
        }



        [Fact]
        public async Task FetchAndStoreShowsAsync_ShouldFetchAndStoreShows_WhenApiReturnsData()
        {
            // Arrange
            var showsDto = new List<ShowDto>
            {
                new ShowDto
                {
                    Id = 1,
                    Name = "Test Show",
                    Language = "English",
                    Genres = new List<string> { "Drama", "Action" },
                    Externals = new ExternalsDto
                    {
                        Imdb = "tt1234567",
                        Tvrage = null,
                        Thetvdb = null
                    },
                    Rating = new RatingDto
                    {
                        Average = 8.5
                    },
                    Network = new NetworkDto
                    {
                        Id = 1,
                        Name = "Test Network",
                        Country = new CountryDto
                        {
                            Name = "USA",
                            Code = "US",
                            Timezone = "America/New_York"
                        }
                    }
                }
            };

            var content = JsonSerializer.Serialize(showsDto);

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent(content),
               })
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://fakeapi.com/"),
            };

            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            _showRepositoryMock.Setup(repo => repo.GetShowByIdAsync(It.IsAny<int>())).ReturnsAsync((Show)null);
            _showRepositoryMock.Setup(repo => repo.AddShowAsync(It.IsAny<Show>())).Returns(Task.CompletedTask);
            _showRepositoryMock.Setup(repo => repo.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            await _showService.FetchAndStoreShowsAsync();

            // Assert
            _showRepositoryMock.Verify(repo => repo.AddShowAsync(It.IsAny<Show>()), Times.Once);
            _showRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task FetchAndStoreShowsAsync_ShouldNotAddExistingShow()
        {
            // Arrange
            var showsDto = new List<ShowDto>
            {
                new ShowDto
                {
                    Id = 1,
                    Name = "Test Show",
                    Language = "English",
                    Genres = new List<string> { "Drama", "Action" },
                }
            };

            var content = JsonSerializer.Serialize(showsDto);

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent(content),
               })
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://fakeapi.com/"),
            };

            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            _showRepositoryMock.Setup(repo => repo.GetShowByIdAsync(It.IsAny<int>())).ReturnsAsync(new Show());

            // Act
            await _showService.FetchAndStoreShowsAsync();

            // Assert
            _showRepositoryMock.Verify(repo => repo.AddShowAsync(It.IsAny<Show>()), Times.Never);
            _showRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task FetchAndStoreShowsAsync_ShouldThrowHttpRequestException_OnNetworkError()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ThrowsAsync(new HttpRequestException("Network error"));

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://fakeapi.com/"),
            };

            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => _showService.FetchAndStoreShowsAsync());
        }

        [Fact]
        public async Task FetchAndStoreShowsAsync_ShouldThrowInvalidOperationException_WhenResponseNotSuccessful()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.BadRequest,
               })
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://fakeapi.com/"),
            };

            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _showService.FetchAndStoreShowsAsync());
        }

        [Fact]
        public async Task GetAllShowsAsync_ShouldReturnShows()
        {
            // Arrange
            var shows = new List<Show>
            {
                new Show { Id = 1, Name = "Show1" },
                new Show { Id = 2, Name = "Show2" }
            };

            _showRepositoryMock.Setup(repo => repo.GetAllShowsAsync()).ReturnsAsync(shows);

            // Act
            var result = await _showService.GetAllShowsAsync();

            // Assert
            Assert.Equal(2, result.Count());
            _showRepositoryMock.Verify(repo => repo.GetAllShowsAsync(), Times.Once);
        }

        [Fact]
        public async Task GetShowByIdAsync_ShouldReturnShow_WhenShowExists()
        {
            // Arrange
            var show = new Show { Id = 1, Name = "Show1" };
            _showRepositoryMock.Setup(repo => repo.GetShowByIdAsync(1)).ReturnsAsync(show);

            // Act
            var result = await _showService.GetShowByIdAsync(1);

            // Assert
            Assert.Equal(show, result);
            _showRepositoryMock.Verify(repo => repo.GetShowByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetShowByIdAsync_ShouldReturnNull_WhenShowDoesNotExist()
        {
            // Arrange
            _showRepositoryMock.Setup(repo => repo.GetShowByIdAsync(1)).ReturnsAsync((Show)null);

            // Act
            var result = await _showService.GetShowByIdAsync(1);

            // Assert
            Assert.Null(result);
            _showRepositoryMock.Verify(repo => repo.GetShowByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task AddShowAsync_ShouldThrowArgumentNullException_WhenShowIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _showService.AddShowAsync(null));
        }

        [Fact]
        public async Task AddShowAsync_ShouldAddShow_WhenValidShow()
        {
            // Arrange
            var show = new Show { Id = 1, Name = "Show1" };

            _showRepositoryMock.Setup(repo => repo.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            await _showService.AddShowAsync(show);

            // Assert
            var addedShow = await _context.Shows.FindAsync(show.Id);
            Assert.NotNull(addedShow);
            Assert.Equal(show.Name, addedShow.Name);
        }

        [Fact]
        public async Task UpdateShowAsync_ShouldThrowArgumentNullException_WhenShowIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _showService.UpdateShowAsync(null));
        }





        [Fact]
        public async Task DeleteShowAsync_ShouldDoNothing_WhenShowDoesNotExist()
        {
            // Arrange
            _showRepositoryMock.Setup(repo => repo.GetShowByIdAsync(1)).ReturnsAsync((Show)null);

            // Act
            await _showService.DeleteShowAsync(1);

            // Assert
            _showRepositoryMock.Verify(repo => repo.DeleteShow(It.IsAny<Show>()), Times.Never);
            _showRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteShowAsync_ShouldThrowException_WhenErrorOccurs()
        {
            // Arrange
            var show = new Show { Id = 1, Name = "Show1" };
            _showRepositoryMock.Setup(repo => repo.GetShowByIdAsync(1)).ReturnsAsync(show);
            _showRepositoryMock.Setup(repo => repo.DeleteShow(show));
            _showRepositoryMock.Setup(repo => repo.SaveChangesAsync()).ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _showService.DeleteShowAsync(1));
        }
    }
}
