using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Challenge.Domain.Entities
{
    /// <summary>
    /// Modelo para el rating de un show.
    /// </summary>
    public class Rating
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [ForeignKey("Show")]
        public int Id { get; set; }

        [JsonPropertyName("average")]
        public double? Average { get; set; }

        [JsonIgnore]
        public Show Show { get; set; }
    }
}
