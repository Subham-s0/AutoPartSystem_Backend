using VehiStock.Application.Dtos.Auth;

namespace VehiStock.Application.Interfaces.IServices;

public interface IUserAuthService
{
    Task<RegisterUserResponse> RegisterUserAsync(RegisterUserRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginWithGoogleAsync(GoogleLoginRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
}
