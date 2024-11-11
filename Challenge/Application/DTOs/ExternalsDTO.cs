using System.Text.Json.Serialization;

namespace Challenge.Application.DTOs
{
    /// <summary>
    /// DTO para los datos externos de un show.
    /// </summary>
    public class ExternalsDto
    {
        [JsonPropertyName("tvrage")]
        public int? Tvrage { get; set; }

        [JsonPropertyName("thetvdb")]
        public int? Thetvdb { get; set; }

        [JsonPropertyName("imdb")]
        public string Imdb { get; set; }
    }
}
