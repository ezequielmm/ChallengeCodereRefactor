﻿using System.Text.Json.Serialization;

namespace Challenge.Application.DTOs
{
    /// <summary>
    /// DTO para los shows obtenidos de la API.
    /// </summary>
    public class ShowDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("language")]
        public string Language { get; set; }

        [JsonPropertyName("genres")]
        public List<string> Genres { get; set; }

        [JsonPropertyName("externals")]
        public ExternalsDto Externals { get; set; }

        [JsonPropertyName("network")]
        public NetworkDto Network { get; set; }

        [JsonPropertyName("rating")]
        public RatingDto Rating { get; set; }
    }
}
