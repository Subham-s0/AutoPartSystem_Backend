using Microsoft.EntityFrameworkCore;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Entities;
using VehiStock.Infrastructure.Persistance;

namespace VehiStock.Infrastructure.Repositories
{
    public class PartRepository : IPartRepository
    {
        private readonly ApplicationDbContext _context;

        public PartRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Part>> GetAllAsync()
        {
            return await _context.Parts
                .Include(p => p.PartCategory)
                .ToListAsync();
        }

        public async Task<Part?> GetByIdAsync(int id)
        {
            return await _context.Parts
                .Include(p => p.PartCategory)
                .FirstOrDefaultAsync(p => p.PartId == id);
        }

        public async Task AddAsync(Part part)
        {
            await _context.Parts.AddAsync(part);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Part part)
        {
            _context.Parts.Update(part);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var part = await _context.Parts.FindAsync(id);

            if (part != null)
            {
                _context.Parts.Remove(part);
                await _context.SaveChangesAsync();
            }
        }
    }
}