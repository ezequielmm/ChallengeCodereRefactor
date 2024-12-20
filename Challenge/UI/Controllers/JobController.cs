﻿using Microsoft.AspNetCore.Mvc;
using Challenge.Application.Interfaces;

namespace Challenge.UI.Controllers
{
    /// <summary>
    /// Controlador para ejecutar trabajos relacionados con la obtención y almacenamiento de shows.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class JobController : ControllerBase
    {
        private readonly IShowService _showService;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Constructor que inyecta el servicio de shows y la configuración de la aplicación.
        /// </summary>
        /// <param name="showService">Servicio para manejar los shows.</param>
        /// <param name="configuration">Configuración de la aplicación.</param>
        public JobController(IShowService showService, IConfiguration configuration)
        {
            _showService = showService;
            _configuration = configuration;
        }

        /// <summary>
        /// Endpoint para ejecutar el trabajo de forma manual.
        /// </summary>
        /// <param name="apiKey">Clave API proporcionada en el encabezado de la solicitud.</param>
        /// <returns>Resultado de la ejecución del trabajo.</returns>
        [HttpPost("run")]
        public async Task<IActionResult> RunJob([FromHeader(Name = "x-api-key")] string apiKey)
        {
            var configuredApiKey = _configuration["ApiKey"];
            if (apiKey != configuredApiKey)
            {
                return Unauthorized("Invalid API key.");
            }

            try
            {
                await _showService.FetchAndStoreShowsAsync();
                return Ok("Job executed successfully.");
            }
            catch (Exception ex)
            {
                // Log the exception if necessary
                return StatusCode(500, "An error occurred while executing the job.");
            }
        }

    }
}
