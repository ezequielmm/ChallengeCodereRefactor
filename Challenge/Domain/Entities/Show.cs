﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Challenge.Domain.Entities
{
    public class Show
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }


        public int ExternalId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("language")]
        public string Language { get; set; }

        public int? NetworkId { get; set; }
        public Network Network { get; set; }

        public Externals Externals { get; set; }

        public Rating Rating { get; set; }

        public ICollection<Genre> Genres { get; set; } = new List<Genre>();
    }
}
