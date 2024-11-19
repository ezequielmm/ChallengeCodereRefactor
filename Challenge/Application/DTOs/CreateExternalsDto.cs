/// <summary>
/// DTO para los Externals del Show.
/// </summary>
namespace Challenge.Application.DTOs
{
    public class CreateExternalsDto
    {
        public string Imdb { get; set; }
        public int? Tvrage { get; set; }
        public int? Thetvdb { get; set; }
    }
}