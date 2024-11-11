using System.Collections.Generic;
using System.Threading.Tasks;
using Challenge.Domain.Entities;

namespace Challenge.Domain.Repositories.Interfaces
{
    /// <summary>
    /// Interfaz que define los métodos para operar con los datos de los shows.
    /// </summary>
    public interface IShowRepository
    {
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
        /// Agrega un nuevo show a la base de datos.
        /// </summary>
        /// <param name="show">El show a agregar.</param>
        Task AddShowAsync(Show show);

        /// <summary>
        /// Actualiza un show existente.
        /// </summary>
        /// <param name="show">El show con los datos actualizados.</param>
        void UpdateShow(Show show);

        /// <summary>
        /// Elimina un show de la base de datos.
        /// </summary>
        /// <param name="show">El show a eliminar.</param>
        void DeleteShow(Show show);

        /// <summary>
        /// Guarda los cambios realizados en la base de datos.
        /// </summary>
        /// <returns>Tarea que representa la operación asincrónica.</returns>
        Task SaveChangesAsync();
    }
}
