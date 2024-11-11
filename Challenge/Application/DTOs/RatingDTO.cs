using System.Text.Json.Serialization;

namespace Challenge.Application.DTOs
{
    /// <summary>
    /// DTO para el rating de un show.
    /// </summary>
    public class RatingDto
    {
        [JsonPropertyName("average")]
        public double? Average { get; set; }
    }
}
