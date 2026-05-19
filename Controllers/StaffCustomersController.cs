using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehiStock.Application.Dtos.Common;
using VehiStock.Domain.Constants;
using VehiStock.Entities;
using VehiStock.Infrastructure.Persistance;

namespace VehiStock.Controllers;

[ApiController]
[Authorize(Roles = $"{RoleNames.Staff},{RoleNames.Admin}")]
[Route("api/staff/customers")]
public class StaffCustomersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public StaffCustomersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<StaffCustomer>>>> SearchCustomers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.CustomerProfiles
                .Include(c => c.User)
                .Include(c => c.Vehicles)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(c =>
                    c.User.FullName.ToLower().Contains(s) ||
                    (c.User.Email != null && c.User.Email.ToLower().Contains(s)) ||
                    (c.User.PhoneNumber != null && c.User.PhoneNumber.ToLower().Contains(s)) ||
                    c.Address.ToLower().Contains(s) ||
                    c.Vehicles.Any(v => v.VehicleNumber.ToLower().Contains(s))
                );
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(c => c.CustomerId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new StaffCustomer
                {
                    CustomerId = c.CustomerId,
                    UserId = c.UserId,
                    FullName = c.User.FullName,
                    Email = c.User.Email ?? string.Empty,
                    PhoneNumber = c.User.PhoneNumber,
                    Address = c.Address,
                    RegisteredAt = c.CreatedAt,
                    Vehicles = c.Vehicles.Select(v => new CustomerVehicleDto
                    {
                        VehicleId = v.VehicleId,
                        CustomerId = v.CustomerId,
                        VehicleNumber = v.VehicleNumber,
                        Brand = v.Make,
                        Model = v.Model,
                        Year = v.ManufactureYear,
                        Mileage = v.MileageKm
                    }).ToList()
                })
                .ToListAsync(cancellationToken);

            var response = new PaginatedResponse<StaffCustomer>
            {
                Items = items,
                TotalRecords = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
            return Ok(ApiResponse<PaginatedResponse<StaffCustomer>>.Ok(response, "Customers fetched successfully."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PaginatedResponse<StaffCustomer>>.Fail("An error occurred: " + ex.Message));
        }
    }

    [HttpGet("{customerId:int}/history")]
    public async Task<ActionResult<ApiResponse<StaffCustomerHistoryResponse>>> GetCustomerHistory(
        int customerId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var customer = await _context.CustomerProfiles
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);

            if (customer == null)
            {
                return NotFound(ApiResponse<StaffCustomerHistoryResponse>.Fail("Customer not found."));
            }

            var sales = await _context.SalesInvoices
                .Where(s => s.CustomerId == customerId)
                .Select(s => new StaffCustomerHistoryItem
                {
                    Type = "Sale",
                    Id = s.SalesInvoiceId,
                    Date = s.InvoiceDate.ToDateTime(TimeOnly.MinValue),
                    Description = $"Sales Invoice #{s.InvoiceNo}",
                    Amount = s.TotalAmount,
                    Status = s.PaymentStatus.ToString()
                })
                .ToListAsync(cancellationToken);

            var services = await _context.ServiceInvoices
                .Include(s => s.ServiceRecord)
                .Where(s => s.CustomerId == customerId)
                .Select(s => new StaffCustomerHistoryItem
                {
                    Type = "Service",
                    Id = s.ServiceInvoiceId,
                    Date = s.ServiceRecord.ServiceDate.ToDateTime(TimeOnly.MinValue),
                    Description = $"Service Invoice - {s.ServiceRecord.Diagnosis}",
                    Amount = s.TotalAmount,
                    Status = s.PaymentStatus.ToString()
                })
                .ToListAsync(cancellationToken);

            var historyItems = sales.Concat(services)
                .OrderByDescending(h => h.Date)
                .ToList();

            var totalSpent = historyItems.Sum(h => h.Amount);

            var response = new StaffCustomerHistoryResponse
            {
                CustomerId = customer.CustomerId,
                FullName = customer.User.FullName,
                Email = customer.User.Email ?? string.Empty,
                PhoneNumber = customer.User.PhoneNumber ?? string.Empty,
                TotalSpent = totalSpent,
                HistoryItems = historyItems
            };

            return Ok(ApiResponse<StaffCustomerHistoryResponse>.Ok(response, "Customer history fetched successfully."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<StaffCustomerHistoryResponse>.Fail("An error occurred: " + ex.Message));
        }
    }
}

public class StaffCustomer
{
    public int CustomerId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Address { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
    public List<CustomerVehicleDto> Vehicles { get; set; } = new();
}

public class CustomerVehicleDto
{
    public int VehicleId { get; set; }
    public int CustomerId { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Mileage { get; set; }
}

public class StaffCustomerHistoryItem
{
    public string Type { get; set; } = string.Empty;
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class StaffCustomerHistoryResponse
{
    public int CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public decimal TotalSpent { get; set; }
    public List<StaffCustomerHistoryItem> HistoryItems { get; set; } = new();
}
