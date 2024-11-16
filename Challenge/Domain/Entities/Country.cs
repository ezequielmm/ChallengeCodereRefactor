using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Challenge.Domain.Entities
{
    public class Country
    {
        [Key]
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("timezone")]
        public string Timezone { get; set; }
    }
}
