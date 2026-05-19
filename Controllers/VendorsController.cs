using Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Domain.Constants;

namespace VehiStock.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.Admin)]
[Route("api/admin/vendors")]
public class VendorsController : ControllerBase
{
    private readonly IVendorService _vendorService;

    public VendorsController(IVendorService vendorService)
    {
        _vendorService = vendorService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<VendorDto>>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var allVendors = await _vendorService.GetAllAsync();

        if (!string.IsNullOrWhiteSpace(search))
        {
            allVendors = allVendors.Where(v => 
                v.VendorName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                v.VendorCode.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (v.ContactPerson != null && v.ContactPerson.Contains(search, StringComparison.OrdinalIgnoreCase)));
        }

        var totalRecords = allVendors.Count();
        var pagedVendors = allVendors
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var paginatedResult = new PaginatedResponse<VendorDto>
        {
            Items = pagedVendors,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
        };

        return Ok(ApiResponse<PaginatedResponse<VendorDto>>.Ok(paginatedResult, "Vendors retrieved successfully."));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<VendorDto>>> GetById(int id)
    {
        var vendor = await _vendorService.GetByIdAsync(id);
        if (vendor == null)
        {
            return NotFound(ApiResponse<VendorDto>.Fail("Vendor not found."));
        }

        return Ok(ApiResponse<VendorDto>.Ok(vendor, "Vendor retrieved successfully."));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<VendorDto>>> Create([FromBody] CreateVendorDto request)
    {
        try
        {
            var msg = await _vendorService.CreateAsync(request);
            var allVendors = await _vendorService.GetAllAsync();
            var newVendor = allVendors.LastOrDefault(v => v.VendorCode == request.VendorCode);

            if (newVendor == null)
            {
                newVendor = new VendorDto
                {
                    VendorCode = request.VendorCode,
                    VendorName = request.VendorName,
                    ContactPerson = request.ContactPerson,
                    Phone = request.Phone,
                    Email = request.Email,
                    Address = request.Address,
                    IsActive = true
                };
            }

            return CreatedAtAction(nameof(GetById), new { id = newVendor.VendorId }, ApiResponse<VendorDto>.Ok(newVendor, msg));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<VendorDto>.Fail(ex.Message));
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<VendorDto>>> Update(int id, [FromBody] UpdateVendorDto request)
    {
        try
        {
            request.VendorId = id;
            var msg = await _vendorService.UpdateAsync(request);

            if (msg == "Vendor not found.")
            {
                return NotFound(ApiResponse<VendorDto>.Fail(msg));
            }

            var updatedVendor = await _vendorService.GetByIdAsync(id);
            if (updatedVendor == null)
            {
                return NotFound(ApiResponse<VendorDto>.Fail("Vendor not found after update."));
            }

            return Ok(ApiResponse<VendorDto>.Ok(updatedVendor, msg));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<VendorDto>.Fail(ex.Message));
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(int id)
    {
        try
        {
            var msg = await _vendorService.DeleteAsync(id);
            if (msg == "Vendor not found.")
            {
                return NotFound(ApiResponse<object?>.Fail(msg));
            }

            return Ok(ApiResponse<object?>.Ok(null, msg));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object?>.Fail(ex.Message));
        }
    }
}
