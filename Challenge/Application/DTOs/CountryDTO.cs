using System.Text.Json.Serialization;

namespace Challenge.Application.DTOs
{
    /// <summary>
    /// DTO para el país asociado a una network.
    /// </summary>
    public class CountryDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("timezone")]
        public string Timezone { get; set; }
    }
}
