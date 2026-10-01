using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RayhanMicrofinance.Application.Common;
using RayhanMicrofinance.Application.DTOs;
using RayhanMicrofinance.Application.Interfaces;
using RayhanMicrofinance.Domain.Entities;
using RayhanMicrofinance.Infrastructure.Data;

namespace RayhanMicrofinance.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IJwtTokenService _jwtService;
    private readonly ICurrentUserService _currentUserService;

    public AuthController(
        ApplicationDbContext db,
        IJwtTokenService jwtService,
        ICurrentUserService currentUserService)
    {
        _db = db;
        _jwtService = jwtService;
        _currentUserService = currentUserService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginRequestDto req)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
        {
            return BadRequest(ApiResponse<LoginResponseDto>.Fail("Username and password are required."));
        }

        var user = await _db.Users
            .Include(u => u.Branch)
            .FirstOrDefaultAsync(u => u.Username.ToLower() == req.Username.Trim().ToLower());

        if (user == null || !user.IsActive)
        {
            return Unauthorized(ApiResponse<LoginResponseDto>.Fail("Invalid credentials or account is inactive."));
        }

        var isPasswordValid = BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            return Unauthorized(ApiResponse<LoginResponseDto>.Fail("Invalid credentials."));
        }

        // Generate tokens
        var token = _jwtService.GenerateAccessToken(user.Id, user.Username, user.Role, user.BranchId, user.EmployeeId);
        var refreshToken = _jwtService.GenerateRefreshToken();

        user.LastLoginAt = DateTime.UtcNow;
        user.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedByIp = HttpContext.Connection.RemoteIpAddress?.ToString()
        });

        await _db.SaveChangesAsync();

        var response = new LoginResponseDto
        {
            UserId = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Role = user.Role,
            BranchId = user.BranchId,
            BranchName = user.Branch?.BranchName,
            EmployeeId = user.EmployeeId,
            AccessToken = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };

        return Ok(ApiResponse<LoginResponseDto>.Ok(response, "Login successful"));
    }

    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUser()
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return Unauthorized(ApiResponse<UserDto>.Fail("Not authenticated"));
        }

        var user = await _db.Users
            .Include(u => u.Branch)
            .FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId.Value);

        if (user == null)
        {
            return NotFound(ApiResponse<UserDto>.Fail("User not found"));
        }

        var dto = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role,
            BranchId = user.BranchId,
            BranchName = user.Branch?.BranchName,
            EmployeeId = user.EmployeeId,
            IsActive = user.IsActive,
            LastLoginAt = user.LastLoginAt
        };

        return Ok(ApiResponse<UserDto>.Ok(dto));
    }
}
