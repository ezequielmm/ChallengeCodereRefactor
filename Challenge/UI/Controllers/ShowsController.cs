using Microsoft.AspNetCore.Mvc;
using Challenge.Domain.Entities;
using Challenge.Application.Interfaces;
using Challenge.Application.DTOs;

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

        /// <summary>
        /// Constructor que inyecta el servicio de shows.
        /// </summary>
        /// <param name="showService">Servicio para manejar los shows.</param>
        public ShowsController(IShowService showService)
        {
            _showService = showService;
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
        /// <param name="show">Objeto show a crear.</param>
        /// <returns>El show creado.</returns>
        [HttpPost]
        public async Task<ActionResult<Show>> CreateShow([FromBody] CreateShowDto showDto)
        {
            if (showDto == null)
            {
                return BadRequest("Los datos del Show no pueden ser nulos.");
            }

            var show = new Show
            {
                Name = showDto.Name,
                Language = showDto.Language,
                Externals = showDto.Externals != null ? new Externals
                {
                    Imdb = showDto.Externals.Imdb,
                    Tvrage = showDto.Externals.Tvrage,
                    Thetvdb = showDto.Externals.Thetvdb
                } : null,
                Rating = showDto.Rating != null ? new Rating
                {
                    Average = showDto.Rating.Average
                } : null,
                Network = showDto.Network != null ? new Network
                {
                    Id = showDto.Network.Id ?? 0, // Si es nueva, el Id será 0
                    Name = showDto.Network.Name,
                    Country = showDto.Network.Country != null ? new Country
                    {
                        Code = showDto.Network.Country.Code,
                        Name = showDto.Network.Country.Name,
                        Timezone = showDto.Network.Country.Timezone
                    } : null
                } : null,
                Genres = new List<Genre>()
            };

            // Manejo de Géneros
            if (showDto.Genres != null)
            {
                foreach (var genreName in showDto.Genres)
                {
                    if (!string.IsNullOrWhiteSpace(genreName))
                    {
                        show.Genres.Add(new Genre { Name = genreName });
                    }
                }
            }

            await _showService.AddShowAsync(show);

            return CreatedAtAction(nameof(GetShowById), new { id = show.Id }, show);
        }



        /// <summary>
        /// Actualiza un show existente.
        /// </summary>
        /// <param name="id">ID del show a actualizar.</param>
        /// <param name="show">Datos actualizados del show.</param>
        /// <returns>Resultado de la operación.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateShow(int id, [FromBody] Show show)
        {
            if (show == null || id != show.Id)
            {
                return BadRequest("Show data is invalid.");
            }

            var existingShow = await _showService.GetShowByIdAsync(id);

            if (existingShow == null)
            {
                return NotFound();
            }

            await _showService.UpdateShowAsync(show);
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
    }
}
