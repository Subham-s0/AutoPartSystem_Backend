using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiStock.Application.Dtos.Admin;
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
    public async Task<ActionResult<ApiResponse<PaginatedResponse<VendorResponse>>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _vendorService.GetVendorsPaginatedAsync(search, pageNumber, pageSize, cancellationToken);
        return Ok(ApiResponse<PaginatedResponse<VendorResponse>>.Ok(result, "Vendors retrieved successfully."));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<VendorResponse>>> GetById(
        int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _vendorService.GetVendorByIdAsync(id, cancellationToken);
            return Ok(ApiResponse<VendorResponse>.Ok(result, "Vendor retrieved successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<VendorResponse>.Fail(ex.Message));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<VendorResponse>>> Create(
        [FromBody] CreateVendorRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _vendorService.CreateVendorAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.VendorId }, ApiResponse<VendorResponse>.Ok(result, "Vendor created successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<VendorResponse>.Fail(ex.Message));
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<VendorResponse>>> Update(
        int id,
        [FromBody] UpdateVendorRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _vendorService.UpdateVendorAsync(id, request, cancellationToken);
            return Ok(ApiResponse<VendorResponse>.Ok(result, "Vendor updated successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<VendorResponse>.Fail(ex.Message));
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(
        int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _vendorService.DeleteVendorAsync(id, cancellationToken);
            return Ok(ApiResponse<object?>.Ok(null, "Vendor deleted successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<object?>.Fail(ex.Message));
        }
    }
}
