using VehiStock.Entities;

namespace VehiStock.Application.Interfaces.IRepositories;

public interface IServiceRecordRepository
{
    Task<ServiceRecord?> GetByIdAsync(int serviceRecordId, CancellationToken cancellationToken = default);
    Task<List<ServiceRecord>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ServiceRecord> CreateAsync(ServiceRecord record, CancellationToken cancellationToken = default);
    Task<ServiceRecord> UpdateAsync(ServiceRecord record, CancellationToken cancellationToken = default);
    Task<Appointment?> GetAppointmentByIdAsync(int appointmentId, CancellationToken cancellationToken = default);
    Task<StaffProfile?> GetStaffProfileByUserIdAsync(string userId, CancellationToken cancellationToken = default);
}
