using Microsoft.EntityFrameworkCore;
using Challenge.Domain.Entities;
using Challenge.Infrastructure.Persistence;
using Challenge.Infrastructure.Data;
using System;
using Xunit;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Challenge.Tests.InfrastructureTests
{
    public class ShowRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly ShowRepository _repository;

        public ShowRepositoryTests()
        {
            // Configuración de la base de datos en memoria
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Base de datos única para cada prueba
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new ShowRepository(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }



        [Fact]
        public async Task AddShowAsync_ShouldAddShowToDatabase()
        {
            // Arrange
            var show = new Show
            {
                Id = 1,
                Name = "New Show",
                Language = "English",
                Genres = new List<Genre> { new Genre { Name = "Drama" } },
                Externals = new Externals { Imdb = "tt1234567", Tvrage = 123, Thetvdb = 456 },
                Rating = new Rating { Average = 8.5 },
                Network = new Network { Name = "Test Network", Country = new Country { Code = "US", Name = "United States", Timezone = "America/New_York" } }
            };

            // Act
            await _repository.AddShowAsync(show);
            await _repository.SaveChangesAsync();

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


    }
}
