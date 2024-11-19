using System.Collections.Generic;

namespace Challenge.Application.DTOs
{
    /// <summary>
    /// DTO para crear un nuevo Show con todas las entidades relacionadas.
    /// </summary>
    public class CreateShowDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Language { get; set; }
        public CreateNetworkDto Network { get; set; }
        public CreateExternalsDto Externals { get; set; }
        public CreateRatingDto Rating { get; set; }
        public List<string> Genres { get; set; }
    }

}
