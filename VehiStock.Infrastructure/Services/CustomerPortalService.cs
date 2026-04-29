using VehiStock.Application.Dtos.Customer;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Entities;

namespace VehiStock.Infrastructure.Services;

public class CustomerPortalService : ICustomerPortalService
{
    private readonly ICustomerPortalRepository _customerPortalRepository;

    public CustomerPortalService(ICustomerPortalRepository customerPortalRepository)
    {
        _customerPortalRepository = customerPortalRepository;
    }

    public async Task<IReadOnlyCollection<CustomerVehicleResponse>> GetVehiclesAsync(string userId, CancellationToken cancellationToken = default)
    {
        var customer = await GetCustomerProfileAsync(userId, cancellationToken);
        var vehicles = await _customerPortalRepository.GetVehiclesByCustomerIdAsync(customer.CustomerId, cancellationToken);

        return vehicles.Select(vehicle => new CustomerVehicleResponse
        {
            VehicleId = vehicle.VehicleId,
            VehicleNumber = vehicle.VehicleNumber,
            Make = vehicle.Make,
            Model = vehicle.Model,
            ManufactureYear = vehicle.ManufactureYear,
            MileageKm = vehicle.MileageKm,
            VehiclePhotoUrl = vehicle.VehiclePhotoUrl
        }).ToArray();
    }

    public async Task<AppointmentResponse> BookAppointmentAsync(string userId, BookAppointmentRequest request, CancellationToken cancellationToken = default)
    {
        var customer = await GetCustomerProfileAsync(userId, cancellationToken);
        if (request.PreferredDate < DateOnly.FromDateTime(DateTime.UtcNow.Date))
        {
            throw new InvalidOperationException("Preferred date cannot be in the past.");
        }

        var vehicle = await _customerPortalRepository.GetVehicleForCustomerAsync(customer.CustomerId, request.VehicleId, cancellationToken);
        if (vehicle is null)
        {
            throw new InvalidOperationException("Vehicle not found for this customer.");
        }

        var appointment = new Appointment
        {
            CustomerId = customer.CustomerId,
            VehicleId = vehicle.VehicleId,
            PreferredDate = request.PreferredDate,
            ServiceType = request.ServiceType.Trim(),
            ProblemDescription = request.ProblemDescription.Trim(),
            Status = AppointmentStatus.Pending
        };

        var createdAppointment = await _customerPortalRepository.CreateAppointmentAsync(appointment, cancellationToken);
        return MapAppointment(createdAppointment);
    }

    public async Task<IReadOnlyCollection<AppointmentResponse>> GetAppointmentsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var customer = await GetCustomerProfileAsync(userId, cancellationToken);
        var appointments = await _customerPortalRepository.GetAppointmentsByCustomerIdAsync(customer.CustomerId, cancellationToken);
        return appointments.Select(MapAppointment).ToArray();
    }

    public async Task<PartRequestResponse> CreatePartRequestAsync(string userId, CreatePartRequestRequest request, CancellationToken cancellationToken = default)
    {
        var customer = await GetCustomerProfileAsync(userId, cancellationToken);
        Vehicle? vehicle = null;
        if (request.VehicleId.HasValue)
        {
            vehicle = await _customerPortalRepository.GetVehicleForCustomerAsync(customer.CustomerId, request.VehicleId.Value, cancellationToken);
            if (vehicle is null)
            {
                throw new InvalidOperationException("Vehicle not found for this customer.");
            }
        }

        var partRequest = new PartRequest
        {
            CustomerId = customer.CustomerId,
            VehicleId = vehicle?.VehicleId,
            RequestedPartName = request.RequestedPartName.Trim(),
            Quantity = request.Quantity,
            Details = string.IsNullOrWhiteSpace(request.Details) ? null : request.Details.Trim(),
            Status = PartRequestStatus.Pending
        };

        var createdPartRequest = await _customerPortalRepository.CreatePartRequestAsync(partRequest, cancellationToken);
        return MapPartRequest(createdPartRequest);
    }

    public async Task<IReadOnlyCollection<PartRequestResponse>> GetPartRequestsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var customer = await GetCustomerProfileAsync(userId, cancellationToken);
        var partRequests = await _customerPortalRepository.GetPartRequestsByCustomerIdAsync(customer.CustomerId, cancellationToken);
        return partRequests.Select(MapPartRequest).ToArray();
    }

    public async Task<ReviewResponse> CreateReviewAsync(string userId, CreateReviewRequest request, CancellationToken cancellationToken = default)
    {
        var customer = await GetCustomerProfileAsync(userId, cancellationToken);
        var serviceRecord = await _customerPortalRepository.GetServiceRecordForCustomerAsync(customer.CustomerId, request.ServiceRecordId, cancellationToken);
        if (serviceRecord is null)
        {
            throw new InvalidOperationException("Service record not found for this customer.");
        }

        var hasExistingReview = await _customerPortalRepository.HasReviewForServiceRecordAsync(customer.CustomerId, request.ServiceRecordId, cancellationToken);
        if (hasExistingReview)
        {
            throw new InvalidOperationException("A review has already been submitted for this service.");
        }

        var review = new Review
        {
            CustomerId = customer.CustomerId,
            ServiceRecordId = serviceRecord.ServiceRecordId,
            Rating = request.Rating,
            ReviewText = request.ReviewText.Trim()
        };

        var createdReview = await _customerPortalRepository.CreateReviewAsync(review, cancellationToken);
        return new ReviewResponse
        {
            ReviewId = createdReview.ReviewId,
            ServiceRecordId = createdReview.ServiceRecordId,
            Rating = createdReview.Rating,
            ReviewText = createdReview.ReviewText,
            CreatedAt = createdReview.CreatedAt
        };
    }

    public async Task<PaginatedResponse<PurchaseHistoryResponse>> GetPurchaseHistoryPageAsync(string userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var customer = await GetCustomerProfileAsync(userId, cancellationToken);
        var normalizedPageNumber = NormalizePageNumber(pageNumber);
        var normalizedPageSize = NormalizePageSize(pageSize);
        var purchases = await _customerPortalRepository.GetPurchaseHistoryPageAsync(
            customer.CustomerId,
            normalizedPageNumber,
            normalizedPageSize,
            cancellationToken);

        return new PaginatedResponse<PurchaseHistoryResponse>
        {
            Items = purchases.Items.Select(MapPurchaseHistory).ToArray(),
            PageNumber = purchases.PageNumber,
            PageSize = purchases.PageSize,
            TotalRecords = purchases.TotalRecords,
            TotalPages = purchases.TotalPages
        };
    }

    public async Task<PaginatedResponse<ServiceHistoryResponse>> GetServiceHistoryPageAsync(string userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var customer = await GetCustomerProfileAsync(userId, cancellationToken);
        var normalizedPageNumber = NormalizePageNumber(pageNumber);
        var normalizedPageSize = NormalizePageSize(pageSize);
        var services = await _customerPortalRepository.GetServiceHistoryPageAsync(
            customer.CustomerId,
            normalizedPageNumber,
            normalizedPageSize,
            cancellationToken);

        return new PaginatedResponse<ServiceHistoryResponse>
        {
            Items = services.Items.Select(MapServiceHistory).ToArray(),
            PageNumber = services.PageNumber,
            PageSize = services.PageSize,
            TotalRecords = services.TotalRecords,
            TotalPages = services.TotalPages
        };
    }

    public async Task<CustomerHistoryResponse> GetHistoryAsync(string userId, CancellationToken cancellationToken = default)
    {
        var customer = await GetCustomerProfileAsync(userId, cancellationToken);
        var purchases = await _customerPortalRepository.GetPurchaseHistoryAsync(customer.CustomerId, cancellationToken);
        var services = await _customerPortalRepository.GetServiceHistoryAsync(customer.CustomerId, cancellationToken);

        return new CustomerHistoryResponse
        {
            Purchases = purchases.Select(MapPurchaseHistory).ToArray(),
            Services = services.Select(MapServiceHistory).ToArray()
        };
    }

    private async Task<CustomerProfile> GetCustomerProfileAsync(string userId, CancellationToken cancellationToken)
    {
        var customer = await _customerPortalRepository.GetCustomerProfileByUserIdAsync(userId, cancellationToken);
        if (customer is null)
        {
            throw new InvalidOperationException("Customer profile was not found for this account.");
        }

        return customer;
    }

    private static AppointmentResponse MapAppointment(Appointment appointment)
    {
        return new AppointmentResponse
        {
            AppointmentId = appointment.AppointmentId,
            VehicleId = appointment.VehicleId,
            VehicleNumber = appointment.Vehicle.VehicleNumber,
            PreferredDate = appointment.PreferredDate,
            ServiceType = appointment.ServiceType,
            ProblemDescription = appointment.ProblemDescription,
            Status = appointment.Status.ToString(),
            BookedAt = appointment.BookedAt
        };
    }

    private static PartRequestResponse MapPartRequest(PartRequest partRequest)
    {
        return new PartRequestResponse
        {
            PartRequestId = partRequest.PartRequestId,
            VehicleId = partRequest.VehicleId,
            VehicleNumber = partRequest.Vehicle?.VehicleNumber,
            RequestedPartName = partRequest.RequestedPartName,
            Quantity = partRequest.Quantity,
            Details = partRequest.Details,
            Status = partRequest.Status.ToString(),
            RequestDate = partRequest.RequestDate
        };
    }

    private static PurchaseHistoryResponse MapPurchaseHistory(SalesInvoice salesInvoice)
    {
        return new PurchaseHistoryResponse
        {
            SalesInvoiceId = salesInvoice.SalesInvoiceId,
            InvoiceNo = salesInvoice.InvoiceNo,
            InvoiceDate = salesInvoice.InvoiceDate,
            VehicleNumber = salesInvoice.Vehicle.VehicleNumber,
            TotalAmount = salesInvoice.TotalAmount,
            AmountPaid = salesInvoice.AmountPaid,
            BalanceDue = salesInvoice.BalanceDue,
            PaymentStatus = salesInvoice.PaymentStatus.ToString(),
            Items = salesInvoice.Items.Select(item => new PurchaseHistoryItemResponse
            {
                PartName = item.Part.PartName,
                Brand = item.Part.Brand,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                DiscountAmount = item.DiscountAmount,
                LineTotal = item.LineTotal
            }).ToArray()
        };
    }

    private static ServiceHistoryResponse MapServiceHistory(ServiceRecord serviceRecord)
    {
        return new ServiceHistoryResponse
        {
            ServiceRecordId = serviceRecord.ServiceRecordId,
            ServiceDate = serviceRecord.ServiceDate,
            VehicleNumber = serviceRecord.Vehicle.VehicleNumber,
            Diagnosis = serviceRecord.Diagnosis,
            WorkDone = serviceRecord.WorkDone,
            LaborCharge = serviceRecord.LaborCharge,
            PartsCharge = serviceRecord.PartsCharge,
            TotalCharge = serviceRecord.TotalCharge,
            Notes = serviceRecord.Notes,
            PartsUsed = serviceRecord.PartsUsed.Select(part => new ServiceHistoryPartResponse
            {
                PartName = part.Part.PartName,
                Brand = part.Part.Brand,
                Quantity = part.Quantity,
                UnitPrice = part.UnitPrice,
                LineTotal = part.LineTotal
            }).ToArray(),
            Review = serviceRecord.Reviews.Select(review => new ReviewResponse
            {
                ReviewId = review.ReviewId,
                ServiceRecordId = review.ServiceRecordId,
                Rating = review.Rating,
                ReviewText = review.ReviewText,
                CreatedAt = review.CreatedAt
            }).SingleOrDefault()
        };
    }

    private static int NormalizePageNumber(int pageNumber)
    {
        return pageNumber < 1 ? 1 : pageNumber;
    }

    private static int NormalizePageSize(int pageSize)
    {
        return Math.Clamp(pageSize, 1, 50);
    }
}
