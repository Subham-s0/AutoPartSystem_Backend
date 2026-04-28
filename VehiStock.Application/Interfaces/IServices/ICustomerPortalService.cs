using VehiStock.Application.Dtos.Customer;

namespace VehiStock.Application.Interfaces.IServices;

public interface ICustomerPortalService
{
    Task<AppointmentResponse> BookAppointmentAsync(string userId, BookAppointmentRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<AppointmentResponse>> GetAppointmentsAsync(string userId, CancellationToken cancellationToken = default);
    Task<PartRequestResponse> CreatePartRequestAsync(string userId, CreatePartRequestRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PartRequestResponse>> GetPartRequestsAsync(string userId, CancellationToken cancellationToken = default);
    Task<ReviewResponse> CreateReviewAsync(string userId, CreateReviewRequest request, CancellationToken cancellationToken = default);
    Task<CustomerHistoryResponse> GetHistoryAsync(string userId, CancellationToken cancellationToken = default);
}
