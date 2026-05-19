using VehiStock.Application.Dtos.Staff;

namespace VehiStock.Application.Interfaces.IServices;

public interface IServiceRecordService
{
    Task<ServiceRecordResponse> CreateAsync(int customerId, int vehicleId, int staffMemberId, int? appointmentId, CancellationToken cancellationToken = default);
    
    Task<ServiceRecordResponse> UpdateAsync(int serviceRecordId, UpdateServiceRecordRequest request, CancellationToken cancellationToken = default);
    
    Task<ServiceRecordResponse> GetAsync(int serviceRecordId, CancellationToken cancellationToken = default);

    Task<ServiceRecordResponse> CreateFromAppointmentAsync(string staffUserId, CreateServiceRecordFromAppointmentRequest request, CancellationToken cancellationToken = default);
}
