using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Challenge.Domain.Entities
{
    /// <summary>
    /// Modelo para la network de un show.
    /// </summary>
    public class Network
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [ForeignKey("Country")]
        public string CountryCode { get; set; }
        public Country Country { get; set; }

        [JsonIgnore]
        public ICollection<Show> Shows { get; set; } = new List<Show>();
    }
}
