using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Customer;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Domain.Constants;

namespace VehiStock.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.Customer)]
[Route("api/customer/profile")]
public class CustomerProfileController : ControllerBase
{
    private readonly ICustomerProfileService _customerProfileService;

    public CustomerProfileController(ICustomerProfileService customerProfileService)
    {
        _customerProfileService = customerProfileService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<CustomerProfileResponse>>> GetProfile(CancellationToken cancellationToken)
    {
        try
        {
            var profile = await _customerProfileService.GetProfileAsync(GetCurrentUserId(), cancellationToken);
            return Ok(ApiResponse<CustomerProfileResponse>.Ok(profile, "Profile fetched successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<CustomerProfileResponse>.Fail(ex.Message));
        }
    }

    private string GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            throw new InvalidOperationException("Authenticated user ID is missing.");
        return userId;
    }
}
