using VehiStock.Application.Dtos.Staff;

namespace VehiStock.Application.Interfaces.IServices;

public interface IStaffCustomerDeskService
{
    Task<IReadOnlyCollection<CustomerDeskDetailsResponse>> SearchAsync(
        string? fullname,
        string? customerPhone,
        string? vehicleNumber,
        int? customerId,
        string? emailId,
        CancellationToken cancellationToken = default);

    Task<CustomerDeskDetailsResponse> GetDetailsAsync(int customerId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<CustomerDeskHistoryLineResponse>> GetPurchaseHistoryAsync(
        int customerId,
        CancellationToken cancellationToken = default);
}
