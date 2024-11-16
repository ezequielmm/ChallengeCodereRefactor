using System.ComponentModel.DataAnnotations;

namespace Challenge.Application.DTOs
{
    /// <summary>
    /// DTO para actualizar una Network existente.
    /// </summary>
    public class UpdateNetworkDto
    {
        public int? Id { get; set; } // Usar si se refiere a una Network existente

        [Required]
        public string Name { get; set; }

        public UpdateCountryDto Country { get; set; }
    }
}
