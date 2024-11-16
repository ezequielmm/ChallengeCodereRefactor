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
                        Thetvdb = showDto.Externals.Thetvdb,
                        Show = show
                    };
                }

                // Maneja el rating si existe
                if (showDto.Rating != null)
                {
                    show.Rating = new Rating
                    {
                        Average = showDto.Rating.Average,
                        Show = show
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



        public async Task<IEnumerable<Show>> GetAllShowsAsync()
        {
            // Obtiene todos los shows desde el repositorio
            return await _showRepository.GetAllShowsAsync();
        }


        public async Task<Show> GetShowByIdAsync(int id)
        {
            // Obtiene un show específico por su ID desde el repositorio
            return await _showRepository.GetShowByIdAsync(id);
        }


        public async Task AddShowAsync(Show show)
        {
            if (show == null)
                throw new ArgumentNullException(nameof(show));

            // Manejo de Network
            if (show.Network != null)
            {
                if (show.Network.Id > 0)
                {
                    // Network existente
                    var existingNetwork = await _context.Networks
                        .Include(n => n.Country)
                        .FirstOrDefaultAsync(n => n.Id == show.Network.Id);

                    if (existingNetwork != null)
                    {
                        show.Network = existingNetwork;
                    }
                    else
                    {
                        // Manejo del Country de la Network
                        if (show.Network.Country != null)
                        {
                            var existingCountry = await _context.Countries
                                .FirstOrDefaultAsync(c => c.Code == show.Network.Country.Code);

                            if (existingCountry != null)
                            {
                                show.Network.Country = existingCountry;
                            }
                            else
                            {
                                _context.Countries.Add(show.Network.Country);
                            }
                        }
                        _context.Networks.Add(show.Network);
                    }
                }
                else
                {
                    // Network nueva
                    if (show.Network.Country != null)
                    {
                        var existingCountry = await _context.Countries
                            .FirstOrDefaultAsync(c => c.Code == show.Network.Country.Code);

                        if (existingCountry != null)
                        {
                            show.Network.Country = existingCountry;
                        }
                        else
                        {
                            _context.Countries.Add(show.Network.Country);
                        }
                    }
                    _context.Networks.Add(show.Network);
                }
            }

            // Manejo de Géneros
            if (show.Genres != null && show.Genres.Any())
            {
                var genreNames = show.Genres.Select(g => g.Name).ToList();
                var existingGenres = await _context.Genres
                    .Where(g => genreNames.Contains(g.Name))
                    .ToListAsync();

                var newGenres = show.Genres.Where(g => !existingGenres.Any(eg => eg.Name == g.Name)).ToList();

                show.Genres = existingGenres.Concat(newGenres).ToList();

                foreach (var genre in newGenres)
                {
                    _context.Genres.Add(genre);
                }
            }

            // Manejo de Externals
            if (show.Externals != null)
            {
                show.Externals.Show = show;
                _context.Externals.Add(show.Externals);
            }

            // Manejo de Rating
            if (show.Rating != null)
            {
                show.Rating.Show = show;
                _context.Ratings.Add(show.Rating);
            }

            // Agrega el show al contexto
            await _context.Shows.AddAsync(show);

            // Guarda los cambios
            await _showRepository.SaveChangesAsync();
        }


        public async Task UpdateShowAsync(Show show)
        {
            if (show == null)
                throw new ArgumentNullException(nameof(show));

            // Obtiene el show existente
            var existingShow = await _context.Shows
                .Include(s => s.Genres)
                .Include(s => s.Externals)
                .Include(s => s.Rating)
                .Include(s => s.Network)
                    .ThenInclude(n => n.Country)
                .FirstOrDefaultAsync(s => s.Id == show.Id);

            if (existingShow == null)
                throw new InvalidOperationException("Show not found.");

            // Actualiza las propiedades del show
            existingShow.Name = show.Name;
            existingShow.Language = show.Language;

            // Manejo de Network
            if (show.Network != null)
            {
                if (show.Network.Id > 0)
                {
                    // Network existente
                    var existingNetwork = await _context.Networks
                        .Include(n => n.Country)
                        .FirstOrDefaultAsync(n => n.Id == show.Network.Id);

                    if (existingNetwork != null)
                    {
                        existingShow.Network = existingNetwork;
                    }
                    else
                    {
                        // Manejo del Country de la Network
                        if (show.Network.Country != null)
                        {
                            var existingCountry = await _context.Countries
                                .FirstOrDefaultAsync(c => c.Code == show.Network.Country.Code);

                            if (existingCountry != null)
                            {
                                show.Network.Country = existingCountry;
                            }
                            else
                            {
                                _context.Countries.Add(show.Network.Country);
                            }
                        }
                        _context.Networks.Add(show.Network);
                        existingShow.Network = show.Network;
                    }
                }
                else
                {
                    // Network nueva
                    if (show.Network.Country != null)
                    {
                        var existingCountry = await _context.Countries
                            .FirstOrDefaultAsync(c => c.Code == show.Network.Country.Code);

                        if (existingCountry != null)
                        {
                            show.Network.Country = existingCountry;
                        }
                        else
                        {
                            _context.Countries.Add(show.Network.Country);
                        }
                    }
                    _context.Networks.Add(show.Network);
                    existingShow.Network = show.Network;
                }
            }
            else
            {
                existingShow.Network = null;
            }

            // Manejo de Externals
            if (show.Externals != null)
            {
                if (existingShow.Externals == null)
                {
                    show.Externals.Show = existingShow;
                    _context.Externals.Add(show.Externals);
                    existingShow.Externals = show.Externals;
                }
                else
                {
                    existingShow.Externals.Imdb = show.Externals.Imdb;
                    existingShow.Externals.Tvrage = show.Externals.Tvrage;
                    existingShow.Externals.Thetvdb = show.Externals.Thetvdb;
                }
            }
            else
            {
                if (existingShow.Externals != null)
                {
                    _context.Externals.Remove(existingShow.Externals);
                    existingShow.Externals = null;
                }
            }

            // Manejo de Rating
            if (show.Rating != null)
            {
                if (existingShow.Rating == null)
                {
                    show.Rating.Show = existingShow;
                    _context.Ratings.Add(show.Rating);
                    existingShow.Rating = show.Rating;
                }
                else
                {
                    existingShow.Rating.Average = show.Rating.Average;
                }
            }
            else
            {
                if (existingShow.Rating != null)
                {
                    _context.Ratings.Remove(existingShow.Rating);
                    existingShow.Rating = null;
                }
            }

            // Manejo de Géneros
            if (show.Genres != null && show.Genres.Any())
            {
                // Limpia los géneros actuales
                existingShow.Genres.Clear();

                // Adjunta los nuevos géneros
                var genreNames = show.Genres.Select(g => g.Name).ToList();
                var existingGenres = await _context.Genres
                    .Where(g => genreNames.Contains(g.Name))
                    .ToListAsync();

                var newGenres = show.Genres.Where(g => !existingGenres.Any(eg => eg.Name == g.Name)).ToList();

                foreach (var genre in newGenres)
                {
                    _context.Genres.Add(genre);
                }

                existingShow.Genres = existingGenres.Concat(newGenres).ToList();
            }
            else
            {
                existingShow.Genres.Clear();
            }

            // Guarda los cambios
            await _showRepository.SaveChangesAsync();
        }


        public async Task DeleteShowAsync(int id)
        {
            var show = await _showRepository.GetShowByIdAsync(id);
            if (show != null)
            {
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

                // Busca el género en el contexto o en la base de datos
                var genre = await _context.Genres.FirstOrDefaultAsync(g => g.Name == genreName);
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
            if (networkDto == null)
                return null;

            // Primero, verifica si el contexto ya está rastreando la Network en su caché local
            var existingNetwork = _context.Networks.Local.FirstOrDefault(n => n.Id == networkDto.Id);
            if (existingNetwork != null)
            {
                return existingNetwork; // Retorna la Network ya rastreada
            }

            // Si no está en la caché local, busca en la base de datos
            existingNetwork = await _context.Networks
                .Include(n => n.Country)
                .FirstOrDefaultAsync(n => n.Id == networkDto.Id);

            if (existingNetwork != null)
            {
                return existingNetwork; // Retorna la Network existente
            }

            // Maneja el país
            Country country = null;
            if (networkDto.Country != null)
            {
                country = await GetOrCreateCountryAsync(networkDto.Country);
            }

            // Crea una nueva Network
            var newNetwork = new Network
            {
                Name = networkDto.Name,
                Country = country
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
            if (countryDto == null)
                return null;

            // Primero, verifica si el contexto ya está rastreando el país en su caché local
            var existingCountry = _context.Countries.Local.FirstOrDefault(c => c.Code == countryDto.Code);
            if (existingCountry != null)
            {
                return existingCountry; // Retorna el país ya rastreado
            }

            // Si no está en la caché local, busca en la base de datos
            existingCountry = await _context.Countries.FirstOrDefaultAsync(c => c.Code == countryDto.Code);

            if (existingCountry != null)
            {
                return existingCountry; // Retorna el país existente
            }

            // Si no existe, crea un nuevo país
            var country = new Country
            {
                Code = countryDto.Code,
                Name = countryDto.Name,
                Timezone = countryDto.Timezone
            };

            _context.Countries.Add(country);
            return country;
        }

    }
}
