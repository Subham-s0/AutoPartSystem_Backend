using Microsoft.EntityFrameworkCore;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Entities;
using VehiStock.Infrastructure.Persistance;

namespace VehiStock.Infrastructure.Repositories;

public class CustomerPortalRepository : ICustomerPortalRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CustomerPortalRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<CustomerProfile?> GetCustomerProfileByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.CustomerProfiles
            .Include(x => x.User)
            .SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    public Task<Vehicle?> GetVehicleForCustomerAsync(int customerId, int vehicleId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Vehicles
            .SingleOrDefaultAsync(x => x.CustomerId == customerId && x.VehicleId == vehicleId, cancellationToken);
    }

    public async Task<Appointment> CreateAppointmentAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        _dbContext.Appointments.Add(appointment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await _dbContext.Appointments
            .Include(x => x.Vehicle)
            .SingleAsync(x => x.AppointmentId == appointment.AppointmentId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Appointment>> GetAppointmentsByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Appointments
            .Include(x => x.Vehicle)
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.BookedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<PartRequest> CreatePartRequestAsync(PartRequest partRequest, CancellationToken cancellationToken = default)
    {
        _dbContext.PartRequests.Add(partRequest);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await _dbContext.PartRequests
            .Include(x => x.Vehicle)
            .SingleAsync(x => x.PartRequestId == partRequest.PartRequestId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<PartRequest>> GetPartRequestsByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.PartRequests
            .Include(x => x.Vehicle)
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.RequestDate)
            .ToListAsync(cancellationToken);
    }

    public Task<ServiceRecord?> GetServiceRecordForCustomerAsync(int customerId, int serviceRecordId, CancellationToken cancellationToken = default)
    {
        return _dbContext.ServiceRecords
            .Include(x => x.Reviews)
            .Include(x => x.Vehicle)
            .SingleOrDefaultAsync(x => x.CustomerId == customerId && x.ServiceRecordId == serviceRecordId, cancellationToken);
    }

    public Task<bool> HasReviewForServiceRecordAsync(int customerId, int serviceRecordId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Reviews.AnyAsync(
            x => x.CustomerId == customerId && x.ServiceRecordId == serviceRecordId,
            cancellationToken);
    }

    public async Task<Review> CreateReviewAsync(Review review, CancellationToken cancellationToken = default)
    {
        _dbContext.Reviews.Add(review);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return review;
    }

    public async Task<IReadOnlyCollection<SalesInvoice>> GetPurchaseHistoryAsync(int customerId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.SalesInvoices
            .Include(x => x.Vehicle)
            .Include(x => x.Items)
                .ThenInclude(x => x.Part)
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.InvoiceDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ServiceRecord>> GetServiceHistoryAsync(int customerId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ServiceRecords
            .Include(x => x.Vehicle)
            .Include(x => x.PartsUsed)
                .ThenInclude(x => x.Part)
            .Include(x => x.Reviews)
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.ServiceDate)
            .ToListAsync(cancellationToken);
    }
}
