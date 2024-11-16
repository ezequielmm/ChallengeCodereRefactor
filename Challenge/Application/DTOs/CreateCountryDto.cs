using System.ComponentModel.DataAnnotations;

public class CreateCountryDto
{
    [Required]
    public string Code { get; set; } // Identificador único del Country

    [Required]
    public string Name { get; set; }

    public string Timezone { get; set; }
}
