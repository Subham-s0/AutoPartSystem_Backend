using VehiStock.Entities;

namespace VehiStock.Application.Interfaces.IServices;

public interface IJwtService
{
    string GenerateToken(ApplicationUser user, IList<string> userRoles);
    DateTime GetAccessTokenExpiryUtc();
    DateTime GetRefreshTokenExpiryUtc();
    string GenerateRefreshToken();
    string HashRefreshToken(string refreshToken);
}
