using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VehiStock.Application.Dtos.Staff;

namespace VehiStock.Application.Interfaces.IServices;

public interface IServiceRecordService
{
    Task<ServiceRecordResponse> CreateAsync(int customerId, int vehicleId, int staffMemberId, int? appointmentId, CancellationToken cancellationToken = default);
    
    Task<ServiceRecordResponse> UpdateAsync(int serviceRecordId, UpdateServiceRecordRequest request, CancellationToken cancellationToken = default);
    
    Task<ServiceRecordResponse> GetAsync(int serviceRecordId, CancellationToken cancellationToken = default);

    Task<ServiceRecordResponse> CreateFromAppointmentAsync(string staffUserId, CreateServiceRecordFromAppointmentRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ServiceRecordResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<ServiceRecordResponse> CreateAsync(string staffUserId, CreateServiceRecordRequest request, CancellationToken cancellationToken = default);
}
