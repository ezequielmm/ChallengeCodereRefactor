using System.Text.Json.Serialization;

namespace Challenge.Application.DTOs
{
    /// <summary>
    /// DTO para los géneros de un show.
    /// </summary>
    public class GenreDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
