using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Net;
using Challenge.Domain.Entities;
using Challenge.Application.Services;
using Challenge.Infrastructure.Data;
using Challenge.Domain.Repositories.Interfaces;
using Challenge.Application.DTOs;
using Moq.Protected;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Generic;
using Challenge.Infrastructure.Persistence;

namespace Challenge.Tests.ApplicationTests
{
    public class ShowServiceTests : IDisposable
    {
        private readonly IShowRepository _repository;
        private readonly ApplicationDbContext _context;
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ShowService _service;

        public ShowServiceTests()
        {
            // Configuración de la base de datos en memoria con un nombre único para cada prueba
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Base de datos única para cada prueba
                .Options;
            _context = new ApplicationDbContext(options);

            // Inicialización del repositorio real
            _repository = new ShowRepository(_context);

            // Mock del IHttpClientFactory
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();

            // Configuración de la aplicación con ApiUrl
            var inMemorySettings = new Dictionary<string, string>
            {
                { "ApiUrl", "http://api.tvmaze.com/" }
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            // Inicialización del ShowService con las dependencias
            _service = new ShowService(_repository, _context, _mockHttpClientFactory.Object, _configuration);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task FetchAndStoreShowsAsync_ShouldFetchAndStoreNewShows()
        {
            // Arrange
            var showsList = new List<ShowDto>
            {
                new ShowDto { Id = 1, Name = "Show 1", Language = "English", Genres = new List<string> { "Drama" } },
                new ShowDto { Id = 2, Name = "Show 2", Language = "Spanish", Genres = new List<string> { "Comedy" } }
            };

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.AbsoluteUri == "http://api.tvmaze.com/shows"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(showsList))
                })
                .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://api.tvmaze.com/")
            };

            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Act
            await _service.FetchAndStoreShowsAsync();

            // Assert
            var storedShows = await _repository.GetAllShowsAsync();
            Assert.Equal(2, storedShows.Count());
            Assert.Contains(storedShows, s => s.Name == "Show 1");
            Assert.Contains(storedShows, s => s.Name == "Show 2");

            // Verificar que la solicitud HTTP fue realizada correctamente
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.AbsoluteUri == "http://api.tvmaze.com/shows"),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task FetchAndStoreShowsAsync_ShouldSkipExistingShows()
        {
            // Arrange
            var existingShow = new Show { Name = "Show 1", Language = "English" };
            await _repository.AddShowAsync(existingShow);
            await _repository.SaveChangesAsync();

            var showsList = new List<ShowDto>
            {
                new ShowDto { Id = 1, Name = "Show 1", Language = "English", Genres = new List<string> { "Drama" } }, // Existing
                new ShowDto { Id = 2, Name = "Show 2", Language = "Spanish", Genres = new List<string> { "Comedy" } }  // New
            };

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.AbsoluteUri == "http://api.tvmaze.com/shows"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(showsList))
                })
                .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://api.tvmaze.com/")
            };

            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Act
            await _service.FetchAndStoreShowsAsync();

            // Assert
            var storedShows = await _repository.GetAllShowsAsync();
            Assert.Equal(2, storedShows.Count());
            Assert.Contains(storedShows, s => s.Name == "Show 1"); // Existing
            Assert.Contains(storedShows, s => s.Name == "Show 2"); // New

            // Verificar que la solicitud HTTP fue realizada correctamente
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.AbsoluteUri == "http://api.tvmaze.com/shows"),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task FetchAndStoreShowsAsync_ShouldHandleApiErrorGracefully()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                })
                .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://api.tvmaze.com/")
            };

            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.FetchAndStoreShowsAsync());
            Assert.Equal("Failed to fetch shows from API.", exception.Message);

            // Verificar que la solicitud HTTP fue realizada correctamente
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.AbsoluteUri == "http://api.tvmaze.com/shows"),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task FetchAndStoreShowsAsync_ShouldThrowHttpRequestException_OnHttpRequestException()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"))
                .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://api.tvmaze.com/")
            };

            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<HttpRequestException>(() => _service.FetchAndStoreShowsAsync());
            Assert.Contains("Error fetching shows from API.", exception.Message);

            // Verificar que la solicitud HTTP fue realizada correctamente
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.AbsoluteUri == "http://api.tvmaze.com/shows"),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task AddShowAsync_ShouldAddShowCorrectly()
        {
            // Arrange
            var newShow = new Show
            {
                Name = "New Show",
                Language = "English",
                Genres = new List<Genre> { new Genre { Name = "Drama" } },
                Externals = new Externals { Imdb = "tt1234567", Tvrage = 123, Thetvdb = 456 },
                Rating = new Rating { Average = 8.5 },
                Network = new Network
                {
                    Name = "Test Network",
                    Country = new Country { Code = "US", Name = "United States", Timezone = "America/New_York" }
                }
            };

            // Act
            await _service.AddShowAsync(newShow);

            // Assert
            var storedShows = await _repository.GetAllShowsAsync();
            Assert.Single(storedShows);
            var storedShow = storedShows.First();
            Assert.Equal("New Show", storedShow.Name);
            Assert.Equal("English", storedShow.Language);
            Assert.Single(storedShow.Genres);
            Assert.Equal("Drama", storedShow.Genres.First().Name);
            Assert.NotNull(storedShow.Externals);
            Assert.Equal("tt1234567", storedShow.Externals.Imdb);
            Assert.Equal(123, storedShow.Externals.Tvrage);
            Assert.Equal(456, storedShow.Externals.Thetvdb);
            Assert.NotNull(storedShow.Rating);
            Assert.Equal(8.5, storedShow.Rating.Average);
            Assert.NotNull(storedShow.Network);
            Assert.Equal("Test Network", storedShow.Network.Name);
            Assert.NotNull(storedShow.Network.Country);
            Assert.Equal("US", storedShow.Network.Country.Code);
        }

        [Fact]
        public async Task AddShowAsync_ShouldThrowArgumentNullException_WhenShowIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.AddShowAsync(null));
        }



        [Fact]
        public async Task DeleteShowAsync_ShouldNotThrow_WhenShowDoesNotExist()
        {
            // Act
            var exception = await Record.ExceptionAsync(() => _service.DeleteShowAsync(999)); // ID que no existe

            // Assert
            Assert.Null(exception); // No se espera ninguna excepción
            var storedShows = await _repository.GetAllShowsAsync();
            Assert.Empty(storedShows); // Asegurarse de que no hay shows
        }



        [Fact]
        public async Task GetShowByIdAsync_ShouldReturnNull_WhenShowDoesNotExist()
        {
            // Act
            var result = await _service.GetShowByIdAsync(999); // ID que no existe

            // Assert
            Assert.Null(result);
        }
    }
}
