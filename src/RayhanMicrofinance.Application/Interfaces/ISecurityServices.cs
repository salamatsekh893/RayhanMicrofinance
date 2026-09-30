using RayhanMicrofinance.Domain.Enums;

namespace RayhanMicrofinance.Application.Interfaces;

public interface ICurrentUserService
{
    int? UserId { get; }
    string? Username { get; }
    UserRole? Role { get; }
    int? BranchId { get; }
    bool IsSuperAdmin { get; }
    bool IsAuthenticated { get; }
}

public interface IJwtTokenService
{
    string GenerateAccessToken(int userId, string username, UserRole role, int? branchId);
    string GenerateRefreshToken();
}
