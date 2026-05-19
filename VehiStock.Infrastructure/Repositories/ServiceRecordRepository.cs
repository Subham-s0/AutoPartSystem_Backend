using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
            .Include(x => x.Customer)
                .ThenInclude(c => c.User)
            .Include(x => x.Vehicle)
            .Include(x => x.StaffMember)
                .ThenInclude(s => s.User)
            .Include(x => x.PartsUsed)
                .ThenInclude(sp => sp.Part)
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

    public Task<Appointment?> GetAppointmentByIdAsync(int appointmentId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Appointments.SingleOrDefaultAsync(x => x.AppointmentId == appointmentId, cancellationToken);
    }

    public Task<StaffProfile?> GetStaffProfileByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.StaffProfiles.Include(s => s.User).SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<ServiceRecord>> GetListAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.ServiceRecords
            .Include(x => x.Customer)
                .ThenInclude(c => c.User)
            .Include(x => x.Vehicle)
            .Include(x => x.StaffMember)
                .ThenInclude(s => s.User)
            .Include(x => x.PartsUsed)
                .ThenInclude(sp => sp.Part)
            .Include(x => x.ServiceInvoice)
            .OrderBy(x => x.ServiceRecordId)
            .ToListAsync(cancellationToken);
    }

    public Task<CustomerProfile?> GetCustomerAsync(int customerId, CancellationToken cancellationToken = default)
    {
        return _dbContext.CustomerProfiles.Include(c => c.User).SingleOrDefaultAsync(x => x.CustomerId == customerId, cancellationToken);
    }

    public Task<Vehicle?> GetVehicleForCustomerAsync(int customerId, int vehicleId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Vehicles.SingleOrDefaultAsync(x => x.CustomerId == customerId && x.VehicleId == vehicleId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Part>> GetPartsByIdsAsync(IReadOnlyCollection<int> partIds, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Parts.Where(x => partIds.Contains(x.PartId)).ToListAsync(cancellationToken);
    }
}
