using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Challenge.Application.DTOs
{
    /// <summary>
    /// DTO para actualizar un Show existente.
    /// </summary>
    public class UpdateShowDto
    {
        [Required]
        public string Name { get; set; }

        public string Language { get; set; }

        public UpdateNetworkDto Network { get; set; }

        public UpdateExternalsDto Externals { get; set; }

        public UpdateRatingDto Rating { get; set; }

        public List<string> Genres { get; set; }
    }
}
