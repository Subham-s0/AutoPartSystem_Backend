using Microsoft.EntityFrameworkCore;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Entities;
using VehiStock.Infrastructure.Persistance;

namespace VehiStock.Infrastructure.Repositories;

public class ServiceRecordRepository : IServiceRecordRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ServiceRecordRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ServiceRecord?> GetByIdAsync(int serviceRecordId, CancellationToken cancellationToken = default)
    {
        return _dbContext.ServiceRecords
            .Include(x => x.Vehicle)
            .Include(x => x.PartsUsed)
            .Include(x => x.ServiceInvoice)
            .SingleOrDefaultAsync(x => x.ServiceRecordId == serviceRecordId, cancellationToken);
    }

    public async Task<ServiceRecord> CreateAsync(ServiceRecord record, CancellationToken cancellationToken = default)
    {
        _dbContext.ServiceRecords.Add(record);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(record.ServiceRecordId, cancellationToken) 
            ?? throw new InvalidOperationException("Service record creation failed.");
    }

    public async Task<ServiceRecord> UpdateAsync(ServiceRecord record, CancellationToken cancellationToken = default)
    {
        _dbContext.ServiceRecords.Update(record);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(record.ServiceRecordId, cancellationToken) 
            ?? throw new InvalidOperationException("Service record update failed.");
    }
}
