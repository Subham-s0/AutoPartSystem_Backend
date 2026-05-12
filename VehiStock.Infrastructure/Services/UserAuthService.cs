using System.ComponentModel.DataAnnotations;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VehiStock.Domain.Constants;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Application.Dtos.Auth;
using VehiStock.Entities;
using Microsoft.Extensions.Options;
using VehiStock.Infrastructure.Settings;

namespace VehiStock.Infrastructure.Services;

public class UserAuthService : IUserAuthService
{
    private static readonly HashSet<string> SupportedRoles = new(RoleNames.Registrable, StringComparer.OrdinalIgnoreCase);
    private const string GoogleLoginProvider = "Google";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IUserAuthRepository _userAuthRepository;
    private readonly IJwtService _jwtService;
    private readonly GoogleAuthSettings _googleAuthSettings;

    public UserAuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IUserAuthRepository userAuthRepository,
        IJwtService jwtService,
        IOptions<GoogleAuthSettings> googleAuthOptions)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _userAuthRepository = userAuthRepository;
        _jwtService = jwtService;
        _googleAuthSettings = googleAuthOptions.Value;
    }

    public async Task<RegisterUserResponse> RegisterUserAsync(RegisterUserRequest request, CancellationToken cancellationToken = default)
    {
        var validationErrors = ValidateRequest(request).ToList();
        if (validationErrors.Count > 0)
        {
            return RegisterUserResponse.Failure(validationErrors);
        }

        var normalizedRole = NormalizeRoleName(request.Role);
        var email = request.Email.Trim();

        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser is not null)
        {
            return RegisterUserResponse.Failure("A user with this email already exists.");
        }

        await EnsureRoleExistsAsync(normalizedRole);

        var user = new ApplicationUser
        {
            FullName = request.FullName.Trim(),
            UserName = email,
            Email = email,
            PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
            ProfilePhotoUrl = string.IsNullOrWhiteSpace(request.ProfilePhotoUrl) ? null : request.ProfilePhotoUrl.Trim(),
            IsActive = true
        };

        await using var transaction = await _userAuthRepository.BeginTransactionAsync(cancellationToken);
        try
        {
            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                await transaction.RollbackAsync(cancellationToken);
                return RegisterUserResponse.Failure(createResult.Errors.Select(x => x.Description));
            }

            var roleResult = await _userManager.AddToRoleAsync(user, normalizedRole);
            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync(cancellationToken);
                return RegisterUserResponse.Failure(roleResult.Errors.Select(x => x.Description));
            }

            int? customerId = null;
            int? staffMemberId = null;

            if (string.Equals(normalizedRole, RoleNames.Customer, StringComparison.Ordinal))
            {
                var customerProfile = await _userAuthRepository.CreateCustomerProfileAsync(
                    user.Id,
                    request.Address!.Trim(),
                    request.RegistrationSource ?? RegistrationSource.SelfRegistered,
                    cancellationToken);
                customerId = customerProfile.CustomerId;
            }
            else if (string.Equals(normalizedRole, RoleNames.Staff, StringComparison.Ordinal))
            {
                var staffProfile = await _userAuthRepository.CreateStaffProfileAsync(
                    user.Id,
                    request.JobTitle!.Trim(),
                    request.HireDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
                    cancellationToken);
                staffMemberId = staffProfile.StaffMemberId;
            }

            await transaction.CommitAsync(cancellationToken);

            return RegisterUserResponse.Success(user.Id, normalizedRole, customerId, staffMemberId);
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return RegisterUserResponse.Failure("Unable to complete registration because the profile data conflicts with an existing record.");
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return AuthResponse.Failure("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return AuthResponse.Failure("Password is required.");
        }

        var user = await _userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null || !user.IsActive)
        {
            return AuthResponse.Failure("Invalid email or password.");
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            return AuthResponse.Failure("Invalid email or password.");
        }

        return await IssueTokensAsync(user, revokeAllActiveTokens: true, cancellationToken);
    }

    public async Task<AuthResponse> LoginWithGoogleAsync(GoogleLoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.IdToken))
        {
            return AuthResponse.Failure("Google ID token is required.");
        }

        if (string.IsNullOrWhiteSpace(_googleAuthSettings.ClientId))
        {
            return AuthResponse.Failure("Google authentication is not configured.");
        }

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(
                request.IdToken.Trim(),
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = [_googleAuthSettings.ClientId]
                });
        }
        catch (InvalidJwtException)
        {
            return AuthResponse.Failure("Invalid Google ID token.");
        }

        if (string.IsNullOrWhiteSpace(payload.Email) || !payload.EmailVerified)
        {
            return AuthResponse.Failure("Google account email is not verified.");
        }

        var user = await _userManager.FindByLoginAsync(GoogleLoginProvider, payload.Subject);
        if (user is null)
        {
            user = await _userManager.FindByEmailAsync(payload.Email.Trim());
            if (user is null)
            {
                return await RegisterCustomerWithGoogleAsync(payload, request.Address, cancellationToken);
            }

            var existingRoles = await _userManager.GetRolesAsync(user);
            var existingRole = existingRoles.SingleOrDefault();
            if (string.IsNullOrWhiteSpace(existingRole) ||
                !string.Equals(existingRole, RoleNames.Staff, StringComparison.Ordinal) &&
                !string.Equals(existingRole, RoleNames.Customer, StringComparison.Ordinal))
            {
                return AuthResponse.Failure("This account is not allowed to use Google login.");
            }

            var addLoginResult = await _userManager.AddLoginAsync(
                user,
                new UserLoginInfo(GoogleLoginProvider, payload.Subject, GoogleLoginProvider));

            if (!addLoginResult.Succeeded)
            {
                return AuthResponse.Failure(addLoginResult.Errors.Select(x => x.Description));
            }
        }

        if (!user.IsActive)
        {
            return AuthResponse.Failure("This account is inactive.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.SingleOrDefault();
        if (string.IsNullOrWhiteSpace(role) ||
            !string.Equals(role, RoleNames.Staff, StringComparison.Ordinal) &&
            !string.Equals(role, RoleNames.Customer, StringComparison.Ordinal))
        {
            return AuthResponse.Failure("This account is not allowed to use Google login.");
        }

        return await IssueTokensAsync(user, revokeAllActiveTokens: true, cancellationToken);
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return AuthResponse.Failure("Refresh token is required.");
        }

        var tokenHash = _jwtService.HashRefreshToken(request.RefreshToken.Trim());

        var storedToken = await _userAuthRepository.GetRefreshTokenByHashAsync(tokenHash, includeUser: true, cancellationToken);

        if (storedToken is null)
        {
            return AuthResponse.Failure("Invalid refresh token.");
        }

        if (storedToken.RevokedAt.HasValue)
        {
            return AuthResponse.Failure("Refresh token has already been used or revoked.");
        }

        if (storedToken.ExpiresAt <= DateTime.UtcNow)
        {
            return AuthResponse.Failure("Refresh token has expired.");
        }

        if (!storedToken.User.IsActive)
        {
            return AuthResponse.Failure("The user account is inactive.");
        }

        await _userAuthRepository.RevokeRefreshTokenAsync(storedToken, DateTime.UtcNow, cancellationToken);
        return await IssueTokensAsync(storedToken.User, revokeAllActiveTokens: false, cancellationToken);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return;
        }

        var tokenHash = _jwtService.HashRefreshToken(refreshToken.Trim());
        var storedToken = await _userAuthRepository.GetRefreshTokenByHashAsync(tokenHash, includeUser: false, cancellationToken);

        if (storedToken is not null && !storedToken.RevokedAt.HasValue)
        {
            await _userAuthRepository.RevokeRefreshTokenAsync(storedToken, DateTime.UtcNow, cancellationToken);
        }
    }

    private static IEnumerable<string> ValidateRequest(RegisterUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            yield return "Full name is required.";
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            yield return "Email is required.";
        }
        else if (!new EmailAddressAttribute().IsValid(request.Email))
        {
            yield return "A valid email address is required.";
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            yield return "Password is required.";
        }

        if (string.IsNullOrWhiteSpace(request.Role))
        {
            yield return "Role is required.";
            yield break;
        }

        if (string.Equals(request.Role, RoleNames.Admin, StringComparison.OrdinalIgnoreCase))
        {
            yield return "Admin accounts are seeded separately and can only log in.";
            yield break;
        }

        if (!SupportedRoles.Contains(request.Role))
        {
            yield return "Role must be Staff or Customer.";
            yield break;
        }

        if (string.Equals(request.Role, RoleNames.Customer, StringComparison.OrdinalIgnoreCase) &&
            string.IsNullOrWhiteSpace(request.Address))
        {
            yield return "Address is required when registering a customer.";
        }

        if (string.Equals(request.Role, RoleNames.Staff, StringComparison.OrdinalIgnoreCase) &&
            string.IsNullOrWhiteSpace(request.JobTitle))
        {
            yield return "Job title is required when registering a staff member.";
        }
    }

    private static string NormalizeRoleName(string role)
    {
        return role.Trim().ToLowerInvariant() switch
        {
            "staff" => RoleNames.Staff,
            "customer" => RoleNames.Customer,
            _ => throw new InvalidOperationException("Unsupported role.")
        };
    }

    private async Task<AuthResponse> RegisterCustomerWithGoogleAsync(
        GoogleJsonWebSignature.Payload payload,
        string? address,
        CancellationToken cancellationToken)
    {
        await EnsureRoleExistsAsync(RoleNames.Customer);

        var email = payload.Email!.Trim();
        var user = new ApplicationUser
        {
            FullName = string.IsNullOrWhiteSpace(payload.Name) ? email : payload.Name.Trim(),
            UserName = email,
            Email = email,
            EmailConfirmed = payload.EmailVerified,
            ProfilePhotoUrl = string.IsNullOrWhiteSpace(payload.Picture) ? null : payload.Picture.Trim(),
            IsActive = true
        };

        await using var transaction = await _userAuthRepository.BeginTransactionAsync(cancellationToken);
        try
        {
            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                await transaction.RollbackAsync(cancellationToken);
                return AuthResponse.Failure(createResult.Errors.Select(x => x.Description));
            }

            var roleResult = await _userManager.AddToRoleAsync(user, RoleNames.Customer);
            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync(cancellationToken);
                return AuthResponse.Failure(roleResult.Errors.Select(x => x.Description));
            }

            var addLoginResult = await _userManager.AddLoginAsync(
                user,
                new UserLoginInfo(GoogleLoginProvider, payload.Subject, GoogleLoginProvider));

            if (!addLoginResult.Succeeded)
            {
                await transaction.RollbackAsync(cancellationToken);
                return AuthResponse.Failure(addLoginResult.Errors.Select(x => x.Description));
            }

            await _userAuthRepository.CreateCustomerProfileAsync(
                user.Id,
                address?.Trim() ?? string.Empty,
                RegistrationSource.SelfRegistered,
                cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        return await IssueTokensAsync(user, revokeAllActiveTokens: true, cancellationToken);
    }

    private async Task EnsureRoleExistsAsync(string roleName)
    {
        if (await _roleManager.RoleExistsAsync(roleName))
        {
            return;
        }

        var createRoleResult = await _roleManager.CreateAsync(new ApplicationRole
        {
            Name = roleName,
            NormalizedName = roleName.ToUpperInvariant()
        });

        if (!createRoleResult.Succeeded)
        {
            throw new InvalidOperationException($"Unable to create the '{roleName}' role.");
        }
    }

    private async Task<AuthResponse> IssueTokensAsync(
        ApplicationUser user,
        bool revokeAllActiveTokens,
        CancellationToken cancellationToken)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.SingleOrDefault();
        if (string.IsNullOrWhiteSpace(role))
        {
            return AuthResponse.Failure("The user does not have an assigned role.");
        }

        if (revokeAllActiveTokens)
        {
            await _userAuthRepository.RevokeActiveRefreshTokensAsync(user.Id, DateTime.UtcNow, cancellationToken);
        }

        var accessToken = _jwtService.GenerateToken(user, roles);
        var accessTokenExpiresAtUtc = _jwtService.GetAccessTokenExpiryUtc();
        var refreshToken = _jwtService.GenerateRefreshToken();
        var refreshTokenExpiresAtUtc = _jwtService.GetRefreshTokenExpiryUtc();

        await _userAuthRepository.AddRefreshTokenAsync(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _jwtService.HashRefreshToken(refreshToken),
            ExpiresAt = refreshTokenExpiresAtUtc
        }, cancellationToken);

        var customerId = await _userAuthRepository.GetCustomerIdByUserIdAsync(user.Id, cancellationToken);
        var staffMemberId = await _userAuthRepository.GetStaffMemberIdByUserIdAsync(user.Id, cancellationToken);

        return AuthResponse.Success(
            user.Id,
            user.Email ?? string.Empty,
            user.FullName,
            role,
            accessToken,
            accessTokenExpiresAtUtc,
            refreshToken,
            refreshTokenExpiresAtUtc,
            customerId,
            staffMemberId);
    }
}
