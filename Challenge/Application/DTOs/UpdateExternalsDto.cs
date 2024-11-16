namespace Challenge.Application.DTOs
{
    /// <summary>
    /// DTO para actualizar los Externals de un Show.
    /// </summary>
    public class UpdateExternalsDto
    {
        public string Imdb { get; set; }

        public int? Tvrage { get; set; }

        public int? Thetvdb { get; set; }
    }
}
