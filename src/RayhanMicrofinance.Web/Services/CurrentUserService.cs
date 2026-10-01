using System.Security.Claims;
using RayhanMicrofinance.Application.Interfaces;
using RayhanMicrofinance.Domain.Enums;

namespace RayhanMicrofinance.Web.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? UserId
    {
        get
        {
            var idClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out var id) ? id : null;
        }
    }

    public string? Username => _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;

    public UserRole? Role
    {
        get
        {
            var roleClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value
                ?? _httpContextAccessor.HttpContext?.User.FindFirst("UserRole")?.Value;

            return Enum.TryParse<UserRole>(roleClaim, out var role) ? role : null;
        }
    }

    public int? BranchId
    {
        get
        {
            var branchClaim = _httpContextAccessor.HttpContext?.User.FindFirst("BranchId")?.Value;
            return int.TryParse(branchClaim, out var id) ? id : null;
        }
    }

    public int? EmployeeId
    {
        get
        {
            var empClaim = _httpContextAccessor.HttpContext?.User.FindFirst("EmployeeId")?.Value;
            return int.TryParse(empClaim, out var id) ? id : null;
        }
    }

    public bool IsSuperAdmin => Role == UserRole.SuperAdmin;
    public bool IsAdminOrSuperAdmin => Role == UserRole.SuperAdmin || Role == UserRole.Admin;
    public bool IsBranchManager => Role == UserRole.BranchManager;
    public bool IsFieldOfficer => Role == UserRole.FieldOfficer;
    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}
