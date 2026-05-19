using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiStock.Application.Dtos.Auth;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Domain.Constants;
using VehiStock.Entities;

namespace VehiStock.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserAuthService _userAuthService;

    public AuthController(IUserAuthService userAuthService)
    {
        _userAuthService = userAuthService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _userAuthService.LoginAsync(request, cancellationToken);
        if (!result.Succeeded)
        {
            return Unauthorized(ApiResponse<AuthResponse>.Fail("Login failed.", result.Errors));
        }

        return Ok(ApiResponse<AuthResponse>.Ok(result, "Login successful."));
    }

    [AllowAnonymous]
    [HttpPost("login/google")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> LoginWithGoogle(
        GoogleLoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _userAuthService.LoginWithGoogleAsync(request, cancellationToken);
        if (!result.Succeeded)
        {
            return Unauthorized(ApiResponse<AuthResponse>.Fail("Google login failed.", result.Errors));
        }

        return Ok(ApiResponse<AuthResponse>.Ok(result, "Google login successful."));
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Refresh(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _userAuthService.RefreshTokenAsync(request, cancellationToken);
        if (!result.Succeeded)
        {
            return Unauthorized(ApiResponse<AuthResponse>.Fail("Token refresh failed.", result.Errors));
        }

        return Ok(ApiResponse<AuthResponse>.Ok(result, "Token refreshed successfully."));
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<string>>> Logout(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        await _userAuthService.LogoutAsync(request.RefreshToken, cancellationToken);
        return Ok(ApiResponse<string>.Ok("Logged out successfully.", "Logged out successfully."));
    }

    [AllowAnonymous]
    [HttpPost("register/customer")]
    public async Task<ActionResult<ApiResponse<RegisterUserResponse>>> RegisterCustomer(
        CustomerSelfRegisterRequest request,
        CancellationToken cancellationToken)
    {
        var serviceRequest = new RegisterUserRequest
        {
            FullName = request.FullName,
            Email = request.Email,
            Password = request.Password,
            Role = RoleNames.Customer,
            PhoneNumber = request.PhoneNumber,
            ProfilePhotoUrl = request.ProfilePhotoUrl,
            Address = request.Address,
            RegistrationSource = request.RegistrationSource ?? RegistrationSource.SelfRegistered
        };

        var result = await _userAuthService.RegisterUserAsync(serviceRequest, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(ApiResponse<RegisterUserResponse>.Fail("Customer registration failed.", result.Errors));
        }

        return Ok(ApiResponse<RegisterUserResponse>.Ok(result, "Customer registered successfully."));
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost("register/staff")]
    public async Task<ActionResult<ApiResponse<RegisterUserResponse>>> RegisterStaff(
        RegisterStaffRequest request,
        CancellationToken cancellationToken)
    {
        var serviceRequest = new RegisterUserRequest
        {
            FullName = request.FullName,
            Email = request.Email,
            Password = request.Password,
            Role = RoleNames.Staff,
            PhoneNumber = request.PhoneNumber,
            ProfilePhotoUrl = request.ProfilePhotoUrl,
            JobTitle = request.JobTitle,
            HireDate = request.HireDate
        };

        var result = await _userAuthService.RegisterUserAsync(serviceRequest, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(ApiResponse<RegisterUserResponse>.Fail("Staff registration failed.", result.Errors));
        }

        return Ok(ApiResponse<RegisterUserResponse>.Ok(result, "Staff registered successfully."));
    }
}



