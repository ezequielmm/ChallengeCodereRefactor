using Challenge.Application.DTOs;
using System.ComponentModel.DataAnnotations;

public class CreateNetworkDto
{
    public int? Id { get; set; } // Usar si se refiere a una Network existente

    [Required]
    public string Name { get; set; }

    public CreateCountryDto Country { get; set; }
}
