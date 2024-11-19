using System.ComponentModel.DataAnnotations;

namespace Challenge.Application.DTOs
{
    public class CreateCountryDto
    {
        [Required]
        public string Code { get; set; } // Identificador único del Country

        [Required]
        public string Name { get; set; }

        public string Timezone { get; set; }
    }
}