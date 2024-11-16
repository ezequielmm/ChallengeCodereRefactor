using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Challenge.Domain.Entities
{
    public class Rating
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ShowId { get; set; }
        [ForeignKey("ShowId")]
        [JsonIgnore]
        public Show Show { get; set; }

        [JsonPropertyName("average")]
        public double? Average { get; set; }
    }
}
