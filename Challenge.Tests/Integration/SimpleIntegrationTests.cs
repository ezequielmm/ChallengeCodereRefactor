using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Challenge.Infrastructure.Data;
using Challenge.Application.Interfaces;
using Challenge.Domain.Repositories.Interfaces;
using Challenge.Application.Services;
using Challenge.Infrastructure.Persistence;

namespace Challenge.Tests.Integration
{
    public class SimpleIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public SimpleIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remover el contexto de base de datos existente
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Agregar un DbContext en memoria para pruebas
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("InMemoryDbForTesting");
                    });

                    // Asegurarse de que los servicios se registren correctamente
                    services.AddScoped<IShowService, ShowService>();
                    services.AddScoped<IShowRepository, ShowRepository>();
                });

                builder.UseEnvironment("Development");
            });
        }

        /// <summary>
        /// Verifica que Swagger UI está disponible en modo desarrollo.
        /// </summary>
        [Fact]
        public async Task Get_SwaggerEndpoint_ReturnsSuccessInDevelopment()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/swagger/index.html");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.ToString().Should().Contain("text/html");
        }

        /// <summary>
        /// Verifica que los servicios están registrados correctamente en el contenedor de dependencias.
        /// </summary>
        [Fact]
        public void Services_AreRegisteredCorrectly()
        {
            // Arrange
            var scopeFactory = _factory.Services.GetService<IServiceScopeFactory>();

            using (var scope = scopeFactory.CreateScope())
            {
                var services = scope.ServiceProvider;

                // Act
                var showService = services.GetService<IShowService>();
                var showRepository = services.GetService<IShowRepository>();
                var dbContext = services.GetService<ApplicationDbContext>();

                // Assert
                showService.Should().NotBeNull();
                showRepository.Should().NotBeNull();
                dbContext.Should().NotBeNull();
            }
        }


    }
}
