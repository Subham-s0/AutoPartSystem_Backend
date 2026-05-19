using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.DTOs.Admin;
using VehiStock.Application.Dtos.Customer;
using VehiStock.Domain.Constants;
using VehiStock.Entities;
using VehiStock.Infrastructure.Persistance;

namespace VehiStock.Controllers;

[ApiController]
[Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Staff}")]
[Route("api/admin/part-requests")]
public class AdminPartRequestsController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public AdminPartRequestsController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<AdminPartRequestDto>>>> GetAll(
        [FromQuery] string? searchText,
        [FromQuery] string? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        IQueryable<PartRequest> query = _dbContext.PartRequests
            .Include(x => x.Customer)
                .ThenInclude(c => c.User)
            .Include(x => x.Vehicle);

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var term = searchText.Trim().ToLower();
            query = query.Where(x =>
                x.RequestedPartName.ToLower().Contains(term) ||
                (x.Vehicle != null && x.Vehicle.VehicleNumber.ToLower().Contains(term)) ||
                (x.Details != null && x.Details.ToLower().Contains(term)) ||
                (x.Customer != null && x.Customer.User != null && x.Customer.User.FullName.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<PartRequestStatus>(status.Trim(), true, out var parsedStatus))
        {
            query = query.Where(x => x.Status == parsedStatus);
        }

        // Apply sorting (by RequestDate Descending)
        query = query.OrderByDescending(x => x.RequestDate);

        var totalRecords = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AdminPartRequestDto
            {
                PartRequestId = x.PartRequestId,
                CustomerId = x.CustomerId,
                CustomerName = x.Customer != null && x.Customer.User != null ? x.Customer.User.FullName : "Unknown",
                VehicleId = x.VehicleId,
                VehicleNumber = x.Vehicle != null ? x.Vehicle.VehicleNumber : null,
                PartName = x.RequestedPartName,
                Description = x.Details,
                Status = x.Status.ToString(),
                RequestDate = x.RequestDate
            })
            .ToListAsync(cancellationToken);

        var response = new PaginatedResponse<AdminPartRequestDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalRecords == 0 ? 0 : (int)Math.Ceiling(totalRecords / (double)pageSize)
        };

        return Ok(ApiResponse<PaginatedResponse<AdminPartRequestDto>>.Ok(response, "Part requests retrieved successfully."));
    }

    [HttpPatch("{partRequestId:int}/status")]
    public async Task<ActionResult<ApiResponse<AdminPartRequestDto>>> UpdateStatus(
        int partRequestId,
        [FromBody] UpdatePartRequestStatusDto request,
        CancellationToken cancellationToken = default)
    {
        var partRequest = await _dbContext.PartRequests
            .Include(x => x.Customer)
                .ThenInclude(c => c.User)
            .Include(x => x.Vehicle)
            .FirstOrDefaultAsync(x => x.PartRequestId == partRequestId, cancellationToken);

        if (partRequest == null)
        {
            return NotFound(ApiResponse<AdminPartRequestDto>.Fail("Part request not found."));
        }

        if (!Enum.TryParse<PartRequestStatus>(request.Status, true, out var newStatus))
        {
            return BadRequest(ApiResponse<AdminPartRequestDto>.Fail("Invalid status value."));
        }

        partRequest.Status = newStatus;
        await _dbContext.SaveChangesAsync(cancellationToken);

        var dto = new AdminPartRequestDto
        {
            PartRequestId = partRequest.PartRequestId,
            CustomerId = partRequest.CustomerId,
            CustomerName = partRequest.Customer != null && partRequest.Customer.User != null ? partRequest.Customer.User.FullName : "Unknown",
            VehicleId = partRequest.VehicleId,
            VehicleNumber = partRequest.Vehicle != null ? partRequest.Vehicle.VehicleNumber : null,
            PartName = partRequest.RequestedPartName,
            Description = partRequest.Details,
            Status = partRequest.Status.ToString(),
            RequestDate = partRequest.RequestDate
        };

        return Ok(ApiResponse<AdminPartRequestDto>.Ok(dto, "Part request status updated successfully."));
    }
}
