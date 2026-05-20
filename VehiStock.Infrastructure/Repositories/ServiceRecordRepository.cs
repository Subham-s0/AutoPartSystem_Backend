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
                .ThenInclude(pu => pu.Part)
            .Include(x => x.ServiceInvoice)
            .Include(x => x.Customer)
                .ThenInclude(c => c.User)
            .Include(x => x.StaffMember)
                .ThenInclude(sm => sm.User)
            .SingleOrDefaultAsync(x => x.ServiceRecordId == serviceRecordId, cancellationToken);
    }

    public Task<List<ServiceRecord>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.ServiceRecords
            .Include(x => x.Vehicle)
            .Include(x => x.PartsUsed)
                .ThenInclude(pu => pu.Part)
            .Include(x => x.ServiceInvoice)
            .Include(x => x.Customer)
                .ThenInclude(c => c.User)
            .Include(x => x.StaffMember)
                .ThenInclude(sm => sm.User)
            .ToListAsync(cancellationToken);
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

    public Task<Appointment?> GetAppointmentByIdAsync(int appointmentId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Appointments.SingleOrDefaultAsync(x => x.AppointmentId == appointmentId, cancellationToken);
    }

    public Task<StaffProfile?> GetStaffProfileByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.StaffProfiles.SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }
}
