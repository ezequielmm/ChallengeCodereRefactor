using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Challenge.Domain.Entities
{
    public class Externals
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ShowId { get; set; }
        [ForeignKey("ShowId")]
        [JsonIgnore]
        public Show Show { get; set; }

        [JsonPropertyName("imdb")]
        public string? Imdb { get; set; } // Propiedad nullable

        [JsonPropertyName("tvrage")]
        public int? Tvrage { get; set; }

        [JsonPropertyName("thetvdb")]
        public int? Thetvdb { get; set; }
    }
}
