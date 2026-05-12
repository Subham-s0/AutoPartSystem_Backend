using Microsoft.EntityFrameworkCore;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Customer;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Entities;
using VehiStock.Infrastructure.Persistance;

namespace VehiStock.Infrastructure.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly ApplicationDbContext _dbContext;

    public AppointmentRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<CustomerProfile?> GetCustomerProfileByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.CustomerProfiles
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

    public Task<Appointment?> GetAppointmentForCustomerAsync(
        int customerId,
        int appointmentId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Appointments
            .Include(x => x.Vehicle)
            .SingleOrDefaultAsync(
                x => x.CustomerId == customerId && x.AppointmentId == appointmentId,
                cancellationToken);
    }

    public async Task<Appointment> UpdateAppointmentAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await _dbContext.Appointments
            .Include(x => x.Vehicle)
            .SingleAsync(x => x.AppointmentId == appointment.AppointmentId, cancellationToken);
    }

    public async Task<PaginatedResponse<Appointment>> GetAppointmentsPageAsync(
        int customerId,
        AppointmentQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Appointments
            .Include(x => x.Vehicle)
            .Where(x => x.CustomerId == customerId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var searchText = request.SearchText.Trim().ToLower();
            query = query.Where(x =>
                x.Vehicle.VehicleNumber.ToLower().Contains(searchText) ||
                x.ServiceType.ToLower().Contains(searchText));
        }

        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<AppointmentStatus>(request.Status.Trim(), true, out var status))
        {
            query = query.Where(x => x.Status == status);
        }

        query = ApplySorting(query, request.SortBy);

        var totalRecords = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResponse<Appointment>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalRecords = totalRecords,
            TotalPages = totalRecords == 0 ? 0 : (int)Math.Ceiling(totalRecords / (double)request.PageSize)
        };
    }

    private static IQueryable<Appointment> ApplySorting(IQueryable<Appointment> query, string? sortBy)
    {
        return sortBy?.Trim().ToLowerInvariant() switch
        {
            "oldest" => query.OrderBy(x => x.BookedAt).ThenBy(x => x.AppointmentId),
            "preferreddate" or "preferreddateasc" => query
                .OrderBy(x => x.PreferredDate)
                .ThenByDescending(x => x.BookedAt),
            "preferreddatedesc" => query
                .OrderByDescending(x => x.PreferredDate)
                .ThenByDescending(x => x.BookedAt),
            _ => query.OrderByDescending(x => x.BookedAt).ThenByDescending(x => x.AppointmentId)
        };
    }
}
