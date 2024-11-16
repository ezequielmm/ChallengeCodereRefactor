using System.ComponentModel.DataAnnotations;

namespace Challenge.Application.DTOs
{
    /// <summary>
    /// DTO para actualizar un Country existente.
    /// </summary>
    public class UpdateCountryDto
    {
        [Required]
        public string Code { get; set; } // Identificador único del Country

        [Required]
        public string Name { get; set; }

        public string Timezone { get; set; }
    }
}
