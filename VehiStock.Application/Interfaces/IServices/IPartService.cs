using VehiStock.Application.DTOs;

namespace VehiStock.Application.Interfaces.IServices
{
    public interface IPartService
    {
        Task<IEnumerable<PartDto>> GetAllAsync();
        Task<PartDto?> GetByIdAsync(int id);
        Task<string> CreateAsync(CreatePartDto dto);
        Task<string> UpdateAsync(UpdatePartDto dto);
        Task<string> DeleteAsync(int id);
    }
}