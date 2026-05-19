using Microsoft.EntityFrameworkCore;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Customer;
using static VehiStock.Application.Dtos.Common.SortDirection;
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

        query = ApplySorting(query, request.Sorts);

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

    private static IQueryable<Appointment> ApplySorting(IQueryable<Appointment> query, List<SortRequest> sorts)
    {
        if (sorts.Count == 0)
        {
            return query.OrderByDescending(x => x.BookedAt).ThenByDescending(x => x.AppointmentId);
        }

        IOrderedQueryable<Appointment>? ordered = null;

        foreach (var sort in sorts)
        {
            var asc = sort.SortDirection == SortDirection.Asc;

            ordered = sort.SortBy.Trim().ToLowerInvariant() switch
            {
                "preferreddate" => ordered is null
                    ? asc ? query.OrderBy(x => x.PreferredDate) : query.OrderByDescending(x => x.PreferredDate)
                    : asc ? ordered.ThenBy(x => x.PreferredDate) : ordered.ThenByDescending(x => x.PreferredDate),
                "status" => ordered is null
                    ? asc ? query.OrderBy(x => x.Status) : query.OrderByDescending(x => x.Status)
                    : asc ? ordered.ThenBy(x => x.Status) : ordered.ThenByDescending(x => x.Status),
                _ => ordered is null
                    ? asc ? query.OrderBy(x => x.BookedAt) : query.OrderByDescending(x => x.BookedAt)
                    : asc ? ordered.ThenBy(x => x.BookedAt) : ordered.ThenByDescending(x => x.BookedAt),
            };
        }

        return ordered!;
    }
}
