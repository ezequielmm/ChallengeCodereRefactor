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
        public async Task GetAllShowsAsync_ShouldReturnAllShows_WithRelatedEntities()
        {
            // Arrange
            var show1 = new Show
            {
                Id = 1,
                Name = "Show 1",
                Genres = new List<Genre> { new Genre { Name = "Drama" } },
                Externals = new Externals { Imdb = "tt1234567", Tvrage = 123, Thetvdb = 456 },
                Rating = new Rating { Average = 8.5 },
                Network = new Network
                {
                    Name = "Network 1",
                    Country = new Country { Code = "US", Name = "United States", Timezone = "America/New_York" }
                }
            };

            var show2 = new Show
            {
                Id = 2,
                Name = "Show 2",
                Genres = new List<Genre> { new Genre { Name = "Comedy" } },
                Externals = new Externals { Imdb = "tt7654321", Tvrage = 321, Thetvdb = 654 },
                Rating = new Rating { Average = 7.5 },
                Network = new Network
                {
                    Name = "Network 2",
                    Country = new Country { Code = "UK", Name = "United Kingdom", Timezone = "Europe/London" }
                }
            };

            await _context.Shows.AddRangeAsync(show1, show2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllShowsAsync();

            // Assert
            Assert.Equal(2, result.Count());

            var firstShow = result.FirstOrDefault(s => s.Id == 1);
            Assert.NotNull(firstShow);
            Assert.Equal("Show 1", firstShow.Name);
            Assert.Single(firstShow.Genres);
            Assert.Equal("Drama", firstShow.Genres.First().Name);
            Assert.NotNull(firstShow.Externals);
            Assert.Equal("tt1234567", firstShow.Externals.Imdb);
            Assert.Equal(123, firstShow.Externals.Tvrage);
            Assert.Equal(456, firstShow.Externals.Thetvdb);
            Assert.NotNull(firstShow.Rating);
            Assert.Equal(8.5, firstShow.Rating.Average);
            Assert.NotNull(firstShow.Network);
            Assert.Equal("Network 1", firstShow.Network.Name);
            Assert.NotNull(firstShow.Network.Country);
            Assert.Equal("US", firstShow.Network.Country.Code);

            var secondShow = result.FirstOrDefault(s => s.Id == 2);
            Assert.NotNull(secondShow);
            Assert.Equal("Show 2", secondShow.Name);
            Assert.Single(secondShow.Genres);
            Assert.Equal("Comedy", secondShow.Genres.First().Name);
            Assert.NotNull(secondShow.Externals);
            Assert.Equal("tt7654321", secondShow.Externals.Imdb);
            Assert.Equal(321, secondShow.Externals.Tvrage);
            Assert.Equal(654, secondShow.Externals.Thetvdb);
            Assert.NotNull(secondShow.Rating);
            Assert.Equal(7.5, secondShow.Rating.Average);
            Assert.NotNull(secondShow.Network);
            Assert.Equal("Network 2", secondShow.Network.Name);
            Assert.NotNull(secondShow.Network.Country);
            Assert.Equal("UK", secondShow.Network.Country.Code);
        }

        [Fact]
        public async Task GetShowByIdAsync_ShouldReturnCorrectShow_WithRelatedEntities()
        {
            // Arrange
            var show = new Show
            {
                Id = 1,
                Name = "Test Show",
                Genres = new List<Genre> { new Genre { Name = "Drama" }, new Genre { Name = "Action" } },
                Externals = new Externals { Imdb = "tt1234567", Tvrage = 123, Thetvdb = 456 },
                Rating = new Rating { Average = 9.0 },
                Network = new Network
                {
                    Name = "Test Network",
                    Country = new Country { Code = "US", Name = "United States", Timezone = "America/New_York" }
                }
            };

            await _context.Shows.AddAsync(show);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetShowByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Show", result.Name);
            Assert.Equal(2, result.Genres.Count);
            Assert.Contains(result.Genres, g => g.Name == "Drama");
            Assert.Contains(result.Genres, g => g.Name == "Action");
            Assert.NotNull(result.Externals);
            Assert.Equal("tt1234567", result.Externals.Imdb);
            Assert.Equal(123, result.Externals.Tvrage);
            Assert.Equal(456, result.Externals.Thetvdb);
            Assert.NotNull(result.Rating);
            Assert.Equal(9.0, result.Rating.Average);
            Assert.NotNull(result.Network);
            Assert.Equal("Test Network", result.Network.Name);
            Assert.NotNull(result.Network.Country);
            Assert.Equal("US", result.Network.Country.Code);
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

        [Fact]
        public async Task DeleteShow_ShouldRemoveShowFromDatabase()
        {
            // Arrange
            var show = new Show { Id = 1, Name = "Test Show" };
            await _repository.AddShowAsync(show);
            await _repository.SaveChangesAsync();

            // Act
            _repository.DeleteShow(show);
            await _repository.SaveChangesAsync();

            // Assert
            var storedShows = await _repository.GetAllShowsAsync();
            Assert.Empty(storedShows);
        }

        [Fact]
        public async Task GetShowByIdAsync_ShouldReturnShow()
        {
            // Arrange
            var show = new Show { Id = 1, Name = "Test Show" };
            await _repository.AddShowAsync(show);
            await _repository.SaveChangesAsync();

            // Act
            var result = await _repository.GetShowByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Show", result.Name);
        }
    }
}
