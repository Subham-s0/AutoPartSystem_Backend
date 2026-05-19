using VehiStock.Application.DTOs;

namespace VehiStock.Application.Interfaces.IServices
{
    public interface IPurchaseInvoiceService
    {
        Task<IEnumerable<PurchaseInvoiceDto>> GetAllAsync();
        Task<PurchaseInvoiceDto?> GetByIdAsync(int id);
        Task<string> CreateAsync(CreatePurchaseInvoiceDto dto);
    }
}