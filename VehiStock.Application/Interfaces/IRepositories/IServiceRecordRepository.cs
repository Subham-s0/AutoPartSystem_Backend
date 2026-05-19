using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VehiStock.Entities;

namespace VehiStock.Application.Interfaces.IRepositories;

public interface IServiceRecordRepository
{
    Task<ServiceRecord?> GetByIdAsync(int serviceRecordId, CancellationToken cancellationToken = default);
    
    Task<ServiceRecord> CreateAsync(ServiceRecord record, CancellationToken cancellationToken = default);
    
    Task<ServiceRecord> UpdateAsync(ServiceRecord record, CancellationToken cancellationToken = default);

    Task<Appointment?> GetAppointmentByIdAsync(int appointmentId, CancellationToken cancellationToken = default);
    
    Task<StaffProfile?> GetStaffProfileByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ServiceRecord>> GetListAsync(CancellationToken cancellationToken = default);

    Task<CustomerProfile?> GetCustomerAsync(int customerId, CancellationToken cancellationToken = default);

    Task<Vehicle?> GetVehicleForCustomerAsync(int customerId, int vehicleId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Part>> GetPartsByIdsAsync(IReadOnlyCollection<int> partIds, CancellationToken cancellationToken = default);
}
