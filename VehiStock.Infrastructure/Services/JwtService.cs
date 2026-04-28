using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Entities;
using VehiStock.Infrastructure.Settings;

namespace VehiStock.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly JwtSettings _jwtSettings;

    public JwtService(IOptions<JwtSettings> jwtOptions)
    {
        _jwtSettings = jwtOptions.Value;
    }

    public string GenerateToken(ApplicationUser user, IList<string> userRoles)
    {
        if (string.IsNullOrWhiteSpace(user.Email))
        {
            throw new InvalidOperationException("Cannot generate a token for a user without an email address.");
        }

        if (string.IsNullOrWhiteSpace(_jwtSettings.SecretKey))
        {
            throw new InvalidOperationException("JWT SecretKey is not configured.");
        }

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (!string.IsNullOrWhiteSpace(user.FullName))
        {
            claims.Add(new Claim(ClaimTypes.Name, user.FullName));
        }

        claims.Add(new Claim(ClaimTypes.Email, user.Email));
        claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));

        claims.AddRange(
            userRoles
                .Where(role => !string.IsNullOrWhiteSpace(role))
                .Select(role => new Claim(ClaimTypes.Role, role))
        );

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public DateTime GetAccessTokenExpiryUtc()
    {
        var expiryMinutes = _jwtSettings.AccessTokenExpirationMinutes > 0
            ? _jwtSettings.AccessTokenExpirationMinutes
            : 15;

        return DateTime.UtcNow.AddMinutes(expiryMinutes);
    }

    public DateTime GetRefreshTokenExpiryUtc()
    {
        var expiryDays = _jwtSettings.RefreshTokenExpirationDays > 0
            ? _jwtSettings.RefreshTokenExpirationDays
            : 7;

        return DateTime.UtcNow.AddDays(expiryDays);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }

    public string HashRefreshToken(string refreshToken)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToHexString(hashBytes);
    }
}
