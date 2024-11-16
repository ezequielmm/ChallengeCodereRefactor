using System.Collections.Generic;

namespace Challenge.Application.DTOs
{
    /// <summary>
    /// DTO para crear un nuevo Show con todas las entidades relacionadas.
    /// </summary>
    public class CreateShowDto
    {
        public string Name { get; set; }
        public string Language { get; set; }
        public CreateNetworkDto Network { get; set; }
        public CreateExternalsDto Externals { get; set; }
        public CreateRatingDto Rating { get; set; }
        public List<string> Genres { get; set; }
    }

    /// <summary>
    /// DTO para la Network del Show.
    /// </summary>
    public class CreateNetworkDto
    {
        public int? Id { get; set; } // Usar si se refiere a una Network existente
        public string Name { get; set; }
        public CreateCountryDto Country { get; set; }
    }

    /// <summary>
    /// DTO para el Country de la Network.
    /// </summary>
    public class CreateCountryDto
    {
        public string Code { get; set; } // Utilizar como identificador único
        public string Name { get; set; }
        public string Timezone { get; set; }
    }

    /// <summary>
    /// DTO para los Externals del Show.
    /// </summary>
    public class CreateExternalsDto
    {
        public string Imdb { get; set; }
        public int? Tvrage { get; set; }
        public int? Thetvdb { get; set; }
    }

    /// <summary>
    /// DTO para el Rating del Show.
    /// </summary>
    public class CreateRatingDto
    {
        public double? Average { get; set; }
    }
}
