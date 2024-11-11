using Microsoft.EntityFrameworkCore;
using Challenge.Domain.Entities;
using Challenge.Infrastructure.Persistence;
using Challenge.Infrastructure.Data;

namespace Challenge.Tests.InfrastructureTests
{
    /// <summary>
    /// Pruebas unitarias para <see cref="ShowRepository"/>.
    /// </summary>
    public class ShowRepositoryTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;
        private ApplicationDbContext _context;
        private ShowRepository _showRepository;

        /// <summary>
        /// Constructor que configura el contexto de la base de datos en memoria.
        /// </summary>
        public ShowRepositoryTests()
        {
            // Configuración de la base de datos en memoria
            _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "ShowRepositoryTests")
                .Options;
        }

        /// <summary>
        /// Inicializa el contexto y el repositorio antes de cada prueba.
        /// </summary>
        private void InitializeRepository()
        {
            // Inicializa el contexto y asegura que la base de datos está limpia
            _context = new ApplicationDbContext(_dbContextOptions);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
            _showRepository = new ShowRepository(_context);
        }

        /// <summary>
        /// Verifica que GetAllShowsAsync retorna todos los shows almacenados.
        /// </summary>
        [Fact]
        public async Task GetAllShowsAsync_ShouldReturnAllShows()
        {
            // Arrange
            InitializeRepository();

            var shows = new List<Show>
            {
                new Show { Id = 1, Name = "Show 1" },
                new Show { Id = 2, Name = "Show 2" }
            };

            _context.Shows.AddRange(shows);
            await _context.SaveChangesAsync();

            // Act
            var result = await _showRepository.GetAllShowsAsync();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, s => s.Name == "Show 1");
            Assert.Contains(result, s => s.Name == "Show 2");
        }

        /// <summary>
        /// Verifica que GetShowByIdAsync retorna el show correcto cuando existe.
        /// </summary>
        [Fact]
        public async Task GetShowByIdAsync_ShouldReturnShow_WhenShowExists()
        {
            // Arrange
            InitializeRepository();

            var show = new Show { Id = 1, Name = "Show 1" };
            _context.Shows.Add(show);
            await _context.SaveChangesAsync();

            // Act
            var result = await _showRepository.GetShowByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Show 1", result.Name);
        }

        /// <summary>
        /// Verifica que GetShowByIdAsync retorna null cuando el show no existe.
        /// </summary>
        [Fact]
        public async Task GetShowByIdAsync_ShouldReturnNull_WhenShowDoesNotExist()
        {
            // Arrange
            InitializeRepository();

            // Act
            var result = await _showRepository.GetShowByIdAsync(1);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Verifica que AddShowAsync agrega correctamente un nuevo show.
        /// </summary>
        [Fact]
        public async Task AddShowAsync_ShouldAddShow()
        {
            // Arrange
            InitializeRepository();

            var show = new Show { Id = 1, Name = "New Show" };

            // Act
            await _showRepository.AddShowAsync(show);
            await _showRepository.SaveChangesAsync();

            // Assert
            var showsInDb = await _context.Shows.ToListAsync();
            Assert.Single(showsInDb);
            Assert.Equal("New Show", showsInDb.First().Name);
        }

        /// <summary>
        /// Verifica que UpdateShow actualiza correctamente un show existente.
        /// </summary>
        [Fact]
        public async Task UpdateShow_ShouldUpdateShow()
        {
            // Arrange
            InitializeRepository();

            var show = new Show { Id = 1, Name = "Original Show" };
            _context.Shows.Add(show);
            await _context.SaveChangesAsync();

            // Act
            show.Name = "Updated Show";
            _showRepository.UpdateShow(show);
            await _showRepository.SaveChangesAsync();

            // Assert
            var updatedShow = await _context.Shows.FindAsync(1);
            Assert.NotNull(updatedShow);
            Assert.Equal("Updated Show", updatedShow.Name);
        }

        /// <summary>
        /// Verifica que DeleteShow elimina correctamente un show existente.
        /// </summary>
        [Fact]
        public async Task DeleteShow_ShouldDeleteShow()
        {
            // Arrange
            InitializeRepository();

            var show = new Show { Id = 1, Name = "Show to Delete" };
            _context.Shows.Add(show);
            await _context.SaveChangesAsync();

            // Act
            _showRepository.DeleteShow(show);
            await _showRepository.SaveChangesAsync();

            // Assert
            var deletedShow = await _context.Shows.FindAsync(1);
            Assert.Null(deletedShow);
        }
    }
}
