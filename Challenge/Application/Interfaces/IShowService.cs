using Challenge.Domain.Entities;

namespace Challenge.Application.Interfaces
{
    /// <summary>
    /// Interfaz que define los métodos para manejar la lógica de negocio relacionada con los shows.
    /// </summary>
    public interface IShowService
    {
        /// <summary>
        /// Obtiene los shows desde una API externa y los almacena en la base de datos.
        /// </summary>
        /// <returns>Tarea que representa la operación asincrónica.</returns>
        Task FetchAndStoreShowsAsync();

        /// <summary>
        /// Obtiene todos los shows almacenados.
        /// </summary>
        /// <returns>Una colección de shows.</returns>
        Task<IEnumerable<Show>> GetAllShowsAsync();

        /// <summary>
        /// Obtiene un show específico por su ID.
        /// </summary>
        /// <param name="id">ID del show.</param>
        /// <returns>El show correspondiente si existe; de lo contrario, null.</returns>
        Task<Show> GetShowByIdAsync(int id);

        /// <summary>
        /// Agrega un nuevo show.
        /// </summary>
        /// <param name="show">El show a agregar.</param>
        Task AddShowAsync(Show show);

        /// <summary>
        /// Actualiza un show existente.
        /// </summary>
        /// <param name="show">El show con los datos actualizados.</param>
        Task UpdateShowAsync(Show show);

        /// <summary>
        /// Elimina un show por su ID.
        /// </summary>
        /// <param name="id">ID del show a eliminar.</param>
        Task DeleteShowAsync(int id);
    }
}
