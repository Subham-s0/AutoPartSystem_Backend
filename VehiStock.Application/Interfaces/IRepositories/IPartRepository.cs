using VehiStock.Entities;

namespace VehiStock.Application.Interfaces.IRepositories
{
    public interface IPartRepository
    {
        Task<IEnumerable<Part>> GetAllAsync();
        Task<Part?> GetByIdAsync(int id);
        Task AddAsync(Part part);
        Task UpdateAsync(Part part);
        Task DeleteAsync(int id);
    }
}