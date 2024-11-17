using Microsoft.EntityFrameworkCore;
using Challenge.Domain.Entities;
using Challenge.Domain.Repositories.Interfaces;
using Challenge.Infrastructure.Data;

namespace Challenge.Infrastructure.Persistence
{
    public class ShowRepository : IShowRepository
    {
        private readonly ApplicationDbContext _context;

        public ShowRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Show>> GetAllShowsAsync()
        {
            return await _context.Shows
                .AsNoTracking()
                .Include(s => s.Genres)
                .Include(s => s.Externals)
                .Include(s => s.Rating)
                .Include(s => s.Network)
                    .ThenInclude(n => n.Country)
                .ToListAsync();
        }

        public async Task<Show> GetShowByIdAsync(int id)
        {
            return await _context.Shows
                .Include(s => s.Genres)
                .Include(s => s.Externals)
                .Include(s => s.Rating)
                .Include(s => s.Network)
                    .ThenInclude(n => n.Country)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Show> GetShowByExternalIdAsync(int externalId)
        {
            return await _context.Shows
                .Include(s => s.Genres)
                .Include(s => s.Externals)
                .Include(s => s.Rating)
                .Include(s => s.Network)
                    .ThenInclude(n => n.Country)
                .FirstOrDefaultAsync(s => s.ExternalId == externalId);
        }

        public async Task AddShowAsync(Show show)
        {
            await _context.Shows.AddAsync(show);
        }

        public void UpdateShow(Show show)
        {
            _context.Shows.Update(show);
        }

        public void DeleteShow(Show show)
        {
            _context.Shows.Remove(show);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
