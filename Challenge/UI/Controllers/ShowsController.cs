using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Challenge.Domain.Entities;
using Challenge.Application.DTOs;
using Challenge.Application.Interfaces;
using Challenge.Infrastructure.Data;

namespace Challenge.UI.Controllers
{
    /// <summary>
    /// Controlador para manejar las operaciones CRUD de los shows.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ShowsController : ControllerBase
    {
        private readonly IShowService _showService;
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Constructor que inyecta el servicio de shows y el contexto de la aplicación.
        /// </summary>
        /// <param name="showService">Servicio para manejar los shows.</param>
        /// <param name="context">Contexto de la aplicación.</param>
        public ShowsController(IShowService showService, ApplicationDbContext context)
        {
            _showService = showService;
            _context = context;
        }

        /// <summary>
        /// Obtiene todos los shows almacenados.
        /// </summary>
        /// <returns>Lista de shows.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Show>>> GetAllShows()
        {
            var shows = await _showService.GetAllShowsAsync();
            return Ok(shows);
        }

        /// <summary>
        /// Obtiene un show específico por ID.
        /// </summary>
        /// <param name="id">ID del show.</param>
        /// <returns>El show solicitado.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Show>> GetShowById(int id)
        {
            var show = await _showService.GetShowByIdAsync(id);

            if (show == null)
            {
                return NotFound();
            }

            return Ok(show);
        }

        /// <summary>
        /// Crea un nuevo show.
        /// </summary>
        /// <param name="showDto">Datos del show a crear.</param>
        /// <returns>El show creado.</returns>
        [HttpPost]
        public async Task<ActionResult<Show>> CreateShow([FromBody] CreateShowDto showDto)
        {
            if (showDto == null)
            {
                return BadRequest("Los datos del show no pueden ser nulos.");
            }

            // Mapear el DTO a la entidad Show
            var show = new Show
            {
                Name = showDto.Name,
                Language = showDto.Language,
                Genres = new List<Genre>()
            };

            // Manejo de Network
            if (showDto.Network != null)
            {
                show.Network = await GetOrCreateNetworkAsync(showDto.Network);
            }

            // Manejo de Externals
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

            // Manejo de Rating
            if (showDto.Rating != null)
            {
                show.Rating = new Rating
                {
                    Average = showDto.Rating.Average,
                    Show = show
                };
            }

            // Manejo de Géneros
            if (showDto.Genres != null && showDto.Genres.Any())
            {
                var genreNames = showDto.Genres;
                var existingGenres = await _context.Genres
                    .Where(g => genreNames.Contains(g.Name))
                    .ToListAsync();

                var newGenres = genreNames
                    .Where(gn => !existingGenres.Any(eg => eg.Name == gn))
                    .Select(gn => new Genre { Name = gn })
                    .ToList();

                show.Genres = existingGenres.Concat(newGenres).ToList();
            }

            // Agrega el show utilizando el servicio
            await _showService.AddShowAsync(show);

            return CreatedAtAction(nameof(GetShowById), new { id = show.Id }, show);
        }

        /// <summary>
        /// Actualiza un show existente.
        /// </summary>
        /// <param name="id">ID del show a actualizar.</param>
        /// <param name="showDto">Datos actualizados del show.</param>
        /// <returns>Resultado de la operación.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateShow(int id, [FromBody] UpdateShowDto showDto)
        {
            if (showDto == null)
            {
                return BadRequest("Los datos del show no pueden ser nulos.");
            }

            var existingShow = await _showService.GetShowByIdAsync(id);

            if (existingShow == null)
            {
                return NotFound();
            }

            // Actualiza las propiedades del show
            existingShow.Name = showDto.Name;
            existingShow.Language = showDto.Language;

            // Actualiza la Network
            if (showDto.Network != null)
            {
                existingShow.Network = await UpdateOrCreateNetworkAsync(showDto.Network);
            }
            else
            {
                existingShow.Network = null;
            }

            // Actualiza Externals
            if (showDto.Externals != null)
            {
                if (existingShow.Externals == null)
                {
                    existingShow.Externals = new Externals
                    {
                        Imdb = showDto.Externals.Imdb,
                        Tvrage = showDto.Externals.Tvrage,
                        Thetvdb = showDto.Externals.Thetvdb,
                        ShowId = existingShow.Id
                    };
                    _context.Externals.Add(existingShow.Externals);
                }
                else
                {
                    existingShow.Externals.Imdb = showDto.Externals.Imdb;
                    existingShow.Externals.Tvrage = showDto.Externals.Tvrage;
                    existingShow.Externals.Thetvdb = showDto.Externals.Thetvdb;
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

            // Actualiza Rating
            if (showDto.Rating != null)
            {
                if (existingShow.Rating == null)
                {
                    existingShow.Rating = new Rating
                    {
                        Average = showDto.Rating.Average,
                        ShowId = existingShow.Id
                    };
                    _context.Ratings.Add(existingShow.Rating);
                }
                else
                {
                    existingShow.Rating.Average = showDto.Rating.Average;
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

            // Actualiza Géneros
            if (showDto.Genres != null && showDto.Genres.Any())
            {
                existingShow.Genres.Clear();

                var genreNames = showDto.Genres;
                var existingGenres = await _context.Genres
                    .Where(g => genreNames.Contains(g.Name))
                    .ToListAsync();

                var newGenres = genreNames
                    .Where(gn => !existingGenres.Any(eg => eg.Name == gn))
                    .Select(gn => new Genre { Name = gn })
                    .ToList();

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

            // Actualiza el show utilizando el servicio
            await _showService.UpdateShowAsync(existingShow);

            return NoContent();
        }

        /// <summary>
        /// Elimina un show por ID.
        /// </summary>
        /// <param name="id">ID del show a eliminar.</param>
        /// <returns>Resultado de la operación.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShow(int id)
        {
            var existingShow = await _showService.GetShowByIdAsync(id);

            if (existingShow == null)
            {
                return NotFound();
            }

            await _showService.DeleteShowAsync(id);
            return NoContent();
        }

        // Métodos auxiliares para manejar entidades relacionadas

        /// <summary>
        /// Obtiene o crea una Network a partir del CreateNetworkDto proporcionado.
        /// </summary>
        /// <param name="networkDto">DTO de la Network.</param>
        /// <returns>La Network correspondiente.</returns>
        private async Task<Network> GetOrCreateNetworkAsync(CreateNetworkDto networkDto)
        {
            if (networkDto == null)
                return null;

            Network network;

            if (networkDto.Id.HasValue)
            {
                // Busca la Network existente
                network = await _context.Networks
                    .Include(n => n.Country)
                    .FirstOrDefaultAsync(n => n.Id == networkDto.Id.Value);

                if (network != null)
                {
                    // Actualiza los datos de la Network
                    network.Name = networkDto.Name;

                    // Actualiza o crea el Country
                    network.Country = await GetOrCreateCountryAsync(networkDto.Country);
                }
                else
                {
                    // Crea una nueva Network
                    network = new Network
                    {
                        Name = networkDto.Name,
                        Country = await GetOrCreateCountryAsync(networkDto.Country)
                    };
                    _context.Networks.Add(network);
                }
            }
            else
            {
                // Crea una nueva Network
                network = new Network
                {
                    Name = networkDto.Name,
                    Country = await GetOrCreateCountryAsync(networkDto.Country)
                };
                _context.Networks.Add(network);
            }

            return network;
        }

        /// <summary>
        /// Obtiene o crea un Country a partir del CreateCountryDto proporcionado.
        /// </summary>
        /// <param name="countryDto">DTO del Country.</param>
        /// <returns>El Country correspondiente.</returns>
        private async Task<Country> GetOrCreateCountryAsync(CreateCountryDto countryDto)
        {
            if (countryDto == null)
                return null;

            var existingCountry = await _context.Countries
                .FirstOrDefaultAsync(c => c.Code == countryDto.Code);

            if (existingCountry != null)
            {
                // Actualiza los datos del Country
                existingCountry.Name = countryDto.Name;
                existingCountry.Timezone = countryDto.Timezone;
                return existingCountry;
            }
            else
            {
                // Crea un nuevo Country
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

        /// <summary>
        /// Actualiza o crea una Network a partir del UpdateNetworkDto proporcionado.
        /// </summary>
        /// <param name="networkDto">DTO de la Network.</param>
        /// <returns>La Network correspondiente.</returns>
        private async Task<Network> UpdateOrCreateNetworkAsync(UpdateNetworkDto networkDto)
        {
            if (networkDto == null)
                return null;

            Network network;

            if (networkDto.Id.HasValue)
            {
                // Busca la Network existente
                network = await _context.Networks
                    .Include(n => n.Country)
                    .FirstOrDefaultAsync(n => n.Id == networkDto.Id.Value);

                if (network != null)
                {
                    // Actualiza los datos de la Network
                    network.Name = networkDto.Name;

                    // Actualiza o crea el Country
                    network.Country = await UpdateOrCreateCountryAsync(networkDto.Country);
                }
                else
                {
                    // Crea una nueva Network
                    network = new Network
                    {
                        Name = networkDto.Name,
                        Country = await UpdateOrCreateCountryAsync(networkDto.Country)
                    };
                    _context.Networks.Add(network);
                }
            }
            else
            {
                // Crea una nueva Network
                network = new Network
                {
                    Name = networkDto.Name,
                    Country = await UpdateOrCreateCountryAsync(networkDto.Country)
                };
                _context.Networks.Add(network);
            }

            return network;
        }

        /// <summary>
        /// Actualiza o crea un Country a partir del UpdateCountryDto proporcionado.
        /// </summary>
        /// <param name="countryDto">DTO del Country.</param>
        /// <returns>El Country correspondiente.</returns>
        private async Task<Country> UpdateOrCreateCountryAsync(UpdateCountryDto countryDto)
        {
            if (countryDto == null)
                return null;

            var existingCountry = await _context.Countries
                .FirstOrDefaultAsync(c => c.Code == countryDto.Code);

            if (existingCountry != null)
            {
                // Actualiza los datos del Country
                existingCountry.Name = countryDto.Name;
                existingCountry.Timezone = countryDto.Timezone;
                return existingCountry;
            }
            else
            {
                // Crea un nuevo Country
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
}
