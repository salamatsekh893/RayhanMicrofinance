using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RayhanMicrofinance.Application.Interfaces;
using RayhanMicrofinance.Domain.Enums;

namespace RayhanMicrofinance.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateAccessToken(int userId, string username, UserRole role, int? branchId)
    {
        var secret = _configuration["Jwt:SecretKey"] ?? "RayhanMicrofinanceEnterpriseSecurityKey2026!#UltraSecure";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role.ToString()),
            new Claim("UserRole", role.ToString())
        };

        if (branchId.HasValue)
        {
            claims.Add(new Claim("BranchId", branchId.Value.ToString()));
        }

        var expiryMinutes = Convert.ToInt32(_configuration["Jwt:ExpiryMinutes"] ?? "1440"); // default 24h

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "RayhanMicrofinance",
            audience: _configuration["Jwt:Audience"] ?? "RayhanMicrofinanceClients",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
