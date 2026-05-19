using Microsoft.EntityFrameworkCore;
using VehiStock.Application.DTOs;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Entities;
using VehiStock.Infrastructure.Persistance;

namespace VehiStock.Application.Services
{
    public class PartService : IPartService
    {
        private readonly IPartRepository _partRepository;
        private readonly ApplicationDbContext _context;

        public PartService(IPartRepository partRepository, ApplicationDbContext context)
        {
            _partRepository = partRepository;
            _context = context;
        }

        public async Task<IEnumerable<PartDto>> GetAllAsync()
        {
            var parts = await _partRepository.GetAllAsync();

            return parts.Select(p => new PartDto
            {
                PartId = p.PartId,
                PartCategoryId = p.PartCategoryId,
                CategoryName = p.PartCategory != null ? p.PartCategory.Name : "Default",
                PartCode = p.PartCode,
                PartName = p.PartName,
                Brand = p.Brand,
                UnitCost = p.UnitCost,
                UnitPrice = p.UnitPrice,
                StockQty = p.StockQty,
                MinimumStock = p.MinimumStock,
                IsActive = p.IsActive
            });
        }

        public async Task<PartDto?> GetByIdAsync(int id)
        {
            var part = await _partRepository.GetByIdAsync(id);

            if (part == null)
                return null;

            return new PartDto
            {
                PartId = part.PartId,
                PartCategoryId = part.PartCategoryId,
                CategoryName = part.PartCategory != null ? part.PartCategory.Name : "Default",
                PartCode = part.PartCode,
                PartName = part.PartName,
                Brand = part.Brand,
                UnitCost = part.UnitCost,
                UnitPrice = part.UnitPrice,
                StockQty = part.StockQty,
                MinimumStock = part.MinimumStock,
                IsActive = part.IsActive
            };
        }

        public async Task<string> CreateAsync(CreatePartDto dto)
        {
            var defaultCategory = await _context.PartCategories
                .FirstOrDefaultAsync(c => c.Name == "Default");

            if (defaultCategory == null)
            {
                defaultCategory = new PartCategory
                {
                    Name = "Default"
                };

                _context.PartCategories.Add(defaultCategory);
                await _context.SaveChangesAsync();
            }

            var part = new Part
            {
                PartCategoryId = defaultCategory.PartCategoryId,
                PartCode = dto.PartCode,
                PartName = dto.PartName,
                Brand = dto.Brand ?? string.Empty,
                UnitCost = dto.UnitCost,
                UnitPrice = dto.UnitPrice,
                StockQty = dto.StockQty,
                MinimumStock = dto.MinimumStock,
                IsActive = true
            };

            await _partRepository.AddAsync(part);

            return "Part created successfully.";
        }

        public async Task<string> UpdateAsync(UpdatePartDto dto)
        {
            var existing = await _partRepository.GetByIdAsync(dto.PartId);

            if (existing == null)
                return "Part not found.";

            existing.PartCode = dto.PartCode;
            existing.PartName = dto.PartName;
            existing.Brand = dto.Brand ?? string.Empty;
            existing.UnitCost = dto.UnitCost;
            existing.UnitPrice = dto.UnitPrice;
            existing.StockQty = dto.StockQty;
            existing.MinimumStock = dto.MinimumStock;
            existing.IsActive = dto.IsActive;

            await _partRepository.UpdateAsync(existing);

            return "Part updated successfully.";
        }

        public async Task<string> DeleteAsync(int id)
        {
            var existing = await _partRepository.GetByIdAsync(id);

            if (existing == null)
                return "Part not found.";

            await _partRepository.DeleteAsync(id);

            return "Part deleted successfully.";
        }
    }
}

