using VehiStock.Entities;

namespace VehiStock.Application.Interfaces.IRepositories
{
    public interface IPurchaseInvoiceRepository
    {
        Task<IEnumerable<PurchaseInvoice>> GetAllAsync();
        Task<PurchaseInvoice?> GetByIdAsync(int id);
        Task AddAsync(PurchaseInvoice invoice);
    }
}