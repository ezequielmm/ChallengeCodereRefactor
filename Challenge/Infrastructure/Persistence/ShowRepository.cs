using Challenge.Domain.Entities;
using Challenge.Domain.Repositories.Interfaces;
using Challenge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Challenge.Infrastructure.Persistence
{
    /// <summary>
    /// Implementación de la interfaz <see cref="IShowRepository"/> para manejar operaciones con los datos de los shows.
    /// </summary>
    public class ShowRepository : IShowRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Constructor que inyecta el contexto de la base de datos.
        /// </summary>
        /// <param name="context">Contexto de la aplicación.</param>
        public ShowRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Show>> GetAllShowsAsync()
        {
            // Obtiene todos los shows incluyendo sus relaciones
            return await _context.Shows
                .AsNoTracking()
                .Include(s => s.Genres)
                .Include(s => s.Externals)
                .Include(s => s.Rating)
                .Include(s => s.Network)
                    .ThenInclude(n => n.Country)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<Show> GetShowByIdAsync(int id)
        {
            // Busca un show por su ID incluyendo sus relaciones
            return await _context.Shows
                .Include(s => s.Genres)
                .Include(s => s.Externals)
                .Include(s => s.Rating)
                .Include(s => s.Network)
                    .ThenInclude(n => n.Country)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        /// <inheritdoc />
        public async Task AddShowAsync(Show show)
        {
            // Agrega un nuevo show al contexto
            await _context.Shows.AddAsync(show);
        }

        /// <inheritdoc />
        public void UpdateShow(Show show)
        {
            // Actualiza el show en el contexto
            _context.Shows.Update(show);
        }

        /// <inheritdoc />
        public void DeleteShow(Show show)
        {
            // Elimina el show del contexto
            _context.Shows.Remove(show);
        }

        /// <inheritdoc />
        public async Task SaveChangesAsync()
        {
            // Guarda los cambios en la base de datos
            await _context.SaveChangesAsync();
        }
    }
}
