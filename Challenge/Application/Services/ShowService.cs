using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Challenge.Domain.Entities;
using Challenge.Application.DTOs;
using Challenge.Domain.Repositories.Interfaces;
using Challenge.Application.Interfaces;
using Challenge.Infrastructure.Data;

namespace Challenge.Application.Services
{
    /// <summary>
    /// Implementación de <see cref="IShowService"/> que maneja la lógica de negocio para los shows.
    /// </summary>
    public class ShowService : IShowService
    {
        private readonly IShowRepository _showRepository;
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiUrl;

        /// <summary>
        /// Constructor que inyecta las dependencias necesarias.
        /// </summary>
        /// <param name="showRepository">Repositorio de shows.</param>
        /// <param name="context">Contexto de la aplicación.</param>
        /// <param name="httpClientFactory">Factoría de HttpClient.</param>
        /// <param name="configuration">Configuración de la aplicación.</param>
        public ShowService(
            IShowRepository showRepository,
            ApplicationDbContext context,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _showRepository = showRepository ?? throw new ArgumentNullException(nameof(showRepository));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));

            _apiUrl = configuration?["ApiUrl"] ?? throw new ArgumentNullException(nameof(configuration));
            if (string.IsNullOrWhiteSpace(_apiUrl))
                throw new ArgumentException("ApiUrl configuration is missing or empty.", nameof(configuration));
        }

        /// <inheritdoc />
        public async Task FetchAndStoreShowsAsync()
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_apiUrl);

            HttpResponseMessage response;
            try
            {
                // Realiza la solicitud GET a la API externa
                response = await client.GetAsync("shows");
            }
            catch (HttpRequestException ex)
            {
                // Lanza una excepción si hay un error de red
                throw new HttpRequestException("Error fetching shows from API.", ex);
            }

            if (!response.IsSuccessStatusCode)
            {
                // Lanza una excepción si la respuesta no es exitosa
                throw new InvalidOperationException("Failed to fetch shows from API.");
            }

            // Lee el contenido de la respuesta
            var content = await response.Content.ReadAsStringAsync();
            var showsDto = JsonSerializer.Deserialize<List<ShowDto>>(content, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            bool changesMade = false;

            foreach (var showDto in showsDto)
            {
                // Verifica si el show ya existe en la base de datos
                if (await _showRepository.GetShowByIdAsync(showDto.Id) != null)
                {
                    continue; // Omite si ya existe
                }

                // Mapea el DTO a la entidad Show
                var show = new Show
                {
                    Id = showDto.Id,
                    Name = showDto.Name,
                    Language = showDto.Language,
                    Genres = new List<Genre>()
                };

                // Agrega los géneros al show
                await AddGenresToShowAsync(show, showDto.Genres);

                // Maneja los datos externos si existen
                if (showDto.Externals != null)
                {
                    show.Externals = new Externals
                    {
                        Imdb = showDto.Externals.Imdb,
                        Tvrage = showDto.Externals.Tvrage,
                        Thetvdb = showDto.Externals.Thetvdb
                    };
                }

                // Maneja el rating si existe
                if (showDto.Rating != null)
                {
                    show.Rating = new Rating
                    {
                        Average = showDto.Rating.Average
                    };
                }

                // Maneja la network si existe
                if (showDto.Network != null)
                {
                    show.Network = await GetOrCreateNetworkAsync(showDto.Network);
                }

                // Agrega el show al repositorio
                await _showRepository.AddShowAsync(show);
                changesMade = true;
            }

            if (changesMade)
            {
                // Guarda los cambios en la base de datos si se realizaron modificaciones
                await _showRepository.SaveChangesAsync();
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Show>> GetAllShowsAsync()
        {
            // Obtiene todos los shows desde el repositorio
            return await _showRepository.GetAllShowsAsync();
        }

        /// <inheritdoc />
        public async Task<Show> GetShowByIdAsync(int id)
        {
            // Obtiene un show específico por su ID desde el repositorio
            return await _showRepository.GetShowByIdAsync(id);
        }

        /// <inheritdoc />
        public async Task AddShowAsync(Show show)
        {
            // Agrega un nuevo show y guarda los cambios
            await _showRepository.AddShowAsync(show);
            await _showRepository.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task UpdateShowAsync(Show show)
        {
            // Actualiza un show existente y guarda los cambios
            _showRepository.UpdateShow(show);
            await _showRepository.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task DeleteShowAsync(int id)
        {
            // Obtiene el show por ID
            var show = await _showRepository.GetShowByIdAsync(id);
            if (show != null)
            {
                // Si existe, lo elimina y guarda los cambios
                _showRepository.DeleteShow(show);
                await _showRepository.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Agrega géneros al show, creando nuevos géneros si no existen.
        /// </summary>
        /// <param name="show">El show al que se agregarán los géneros.</param>
        /// <param name="genres">Lista de nombres de géneros.</param>
        /// <returns>Tarea que representa la operación asincrónica.</returns>
        private async Task AddGenresToShowAsync(Show show, List<string> genres)
        {
            if (genres == null) return;

            foreach (var genreName in genres)
            {
                if (string.IsNullOrWhiteSpace(genreName)) continue;

                // Busca el género en el contexto local o en la base de datos
                var genre = _context.Genres.Local.FirstOrDefault(g => g.Name == genreName)
                             ?? await _context.Genres.FirstOrDefaultAsync(g => g.Name == genreName);
                if (genre == null)
                {
                    // Si el género no existe, lo crea
                    genre = new Genre { Name = genreName };
                    _context.Genres.Add(genre);
                }
                show.Genres.Add(genre);
            }
        }

        /// <summary>
        /// Obtiene una network existente o crea una nueva a partir del DTO proporcionado.
        /// </summary>
        /// <param name="networkDto">DTO de la network.</param>
        /// <returns>La network correspondiente.</returns>
        private async Task<Network> GetOrCreateNetworkAsync(NetworkDto networkDto)
        {
            // Busca la network en el contexto local o en la base de datos
            var existingNetwork = _context.Networks.Local.FirstOrDefault(n => n.Id == networkDto.Id)
                                  ?? await _context.Networks.FirstOrDefaultAsync(n => n.Id == networkDto.Id);

            if (existingNetwork != null)
            {
                return existingNetwork; // Retorna la network existente
            }

            // Crea una nueva network
            var newNetwork = new Network
            {
                Id = networkDto.Id,
                Name = networkDto.Name,
                Country = networkDto.Country != null ? await GetOrCreateCountryAsync(networkDto.Country) : null
            };

            _context.Networks.Add(newNetwork);
            return newNetwork;
        }

        /// <summary>
        /// Obtiene un país existente o crea uno nuevo a partir del DTO proporcionado.
        /// </summary>
        /// <param name="countryDto">DTO del país.</param>
        /// <returns>El país correspondiente.</returns>
        private async Task<Country> GetOrCreateCountryAsync(CountryDto countryDto)
        {
            // Busca el país en el contexto local o en la base de datos
            var existingCountry = _context.Countries.Local.FirstOrDefault(c => c.Id == countryDto.Id)
                                 ?? await _context.Countries.FirstOrDefaultAsync(c => c.Id == countryDto.Id);

            if (existingCountry != null)
            {
                return existingCountry; // Retorna el país existente
            }

            // Crea un nuevo país
            var country = new Country
            {
                Id = countryDto.Id,
                Code = countryDto.Code,
                Name = countryDto.Name,
                Timezone = countryDto.Timezone
            };

            _context.Countries.Add(country);
            return country;
        }
    }
}
