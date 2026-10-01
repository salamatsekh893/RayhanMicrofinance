using RayhanMicrofinance.Domain.Enums;

namespace RayhanMicrofinance.Application.Interfaces;

public interface ICurrentUserService
{
    int? UserId { get; }
    string? Username { get; }
    UserRole? Role { get; }
    int? BranchId { get; }
    int? EmployeeId { get; }
    bool IsSuperAdmin { get; }
    bool IsAdminOrSuperAdmin { get; }
    bool IsBranchManager { get; }
    bool IsFieldOfficer { get; }
    bool IsAuthenticated { get; }
}

public interface IJwtTokenService
{
    string GenerateAccessToken(int userId, string username, UserRole role, int? branchId, int? employeeId = null);
    string GenerateRefreshToken();
}
