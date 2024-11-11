using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Challenge.Domain.Repositories.Interfaces;
using Challenge.Application.Services;
using Challenge.Infrastructure.Data;

namespace Challenge.Tests.ApplicationTests
{
    /// <summary>
    /// Pruebas unitarias para el constructor de <see cref="ShowService"/>.
    /// </summary>
    public class ShowServiceConstructorTests
    {
        private Mock<IShowRepository> _showRepositoryMock;
        private Mock<IHttpClientFactory> _httpClientFactoryMock;
        private IConfiguration _configuration;
        private ApplicationDbContext _context;

        /// <summary>
        /// Constructor que inicializa los mocks.
        /// </summary>
        public ShowServiceConstructorTests()
        {
            // Inicializar los mocks
            _showRepositoryMock = new Mock<IShowRepository>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();

            // Configuración de la base de datos en memoria
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "ShowServiceConstructorTestDb")
                .Options;
            _context = new ApplicationDbContext(options);
        }

        /// <summary>
        /// Crea una instancia de IConfiguration con ApiUrl.
        /// </summary>
        /// <param name="apiUrl">La URL de la API.</param>
        /// <returns>Una instancia de IConfiguration.</returns>
        private IConfiguration GetConfiguration(string apiUrl = "http://api.tvmaze.com/")
        {
            var inMemorySettings = new Dictionary<string, string> {
                {"ApiUrl", apiUrl}
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        /// <summary>
        /// Verifica que el constructor de ShowService crea una instancia correctamente cuando todas las dependencias son válidas.
        /// </summary>
        [Fact]
        public void Constructor_ShouldCreateInstance_WhenAllDependenciesAreProvided()
        {
            // Arrange
            _configuration = GetConfiguration("http://api.tvmaze.com/");

            // Act
            var service = new ShowService(
                _showRepositoryMock.Object,
                _context,
                _httpClientFactoryMock.Object,
                _configuration
            );

            // Assert
            Assert.NotNull(service);
        }

        /// <summary>
        /// Verifica que el constructor lanza ArgumentNullException cuando showRepository es null.
        /// </summary>
        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenShowRepositoryIsNull()
        {
            // Arrange
            _configuration = GetConfiguration("http://api.tvmaze.com/");

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new ShowService(
                null, // showRepository es null
                _context,
                _httpClientFactoryMock.Object,
                _configuration
            ));

            Assert.Equal("showRepository", exception.ParamName);
        }

        /// <summary>
        /// Verifica que el constructor lanza ArgumentNullException cuando context es null.
        /// </summary>
        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenContextIsNull()
        {
            // Arrange
            _configuration = GetConfiguration("http://api.tvmaze.com/");

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new ShowService(
                _showRepositoryMock.Object,
                null, // context es null
                _httpClientFactoryMock.Object,
                _configuration
            ));

            Assert.Equal("context", exception.ParamName);
        }

        /// <summary>
        /// Verifica que el constructor lanza ArgumentNullException cuando httpClientFactory es null.
        /// </summary>
        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenHttpClientFactoryIsNull()
        {
            // Arrange
            _configuration = GetConfiguration("http://api.tvmaze.com/");

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new ShowService(
                _showRepositoryMock.Object,
                _context,
                null, // httpClientFactory es null
                _configuration
            ));

            Assert.Equal("httpClientFactory", exception.ParamName);
        }

        /// <summary>
        /// Verifica que el constructor lanza ArgumentNullException cuando configuration es null.
        /// </summary>
        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenConfigurationIsNull()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new ShowService(
                _showRepositoryMock.Object,
                _context,
                _httpClientFactoryMock.Object,
                null // configuration es null
            ));

            Assert.Equal("configuration", exception.ParamName);
        }

        /// <summary>
        /// Verifica que el constructor lanza ArgumentException cuando ApiUrl está vacío o contiene solo espacios en blanco.
        /// </summary>
        /// <param name="apiUrl">El valor de ApiUrl a probar.</param>
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_ShouldThrowArgumentException_WhenApiUrlIsEmptyOrWhitespace(string apiUrl)
        {
            // Arrange
            _configuration = GetConfiguration(apiUrl);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new ShowService(
                _showRepositoryMock.Object,
                _context,
                _httpClientFactoryMock.Object,
                _configuration
            ));

            Assert.Equal("configuration", exception.ParamName);
            Assert.Contains("ApiUrl configuration is missing or empty.", exception.Message);
        }
    }
}
