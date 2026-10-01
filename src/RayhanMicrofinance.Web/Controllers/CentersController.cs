using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RayhanMicrofinance.Application.Common;
using RayhanMicrofinance.Application.DTOs;
using RayhanMicrofinance.Application.Interfaces;
using RayhanMicrofinance.Domain.Entities;
using RayhanMicrofinance.Domain.Enums;
using RayhanMicrofinance.Infrastructure.Data;

namespace RayhanMicrofinance.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CentersController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CentersController(ApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CenterDto>>>> GetCenters([FromQuery] int? branchId)
    {
        if (!_currentUser.IsAdminOrSuperAdmin && _currentUser.BranchId.HasValue)
        {
            branchId = _currentUser.BranchId.Value;
        }

        var query = _db.Centers
            .Include(c => c.Branch)
            .Include(c => c.FieldOfficer)
            .Include(c => c.Groups)
            .Include(c => c.Customers)
            .AsNoTracking()
            .AsQueryable();

        if (branchId.HasValue) query = query.Where(c => c.BranchId == branchId.Value);

        if (_currentUser.IsFieldOfficer && _currentUser.EmployeeId.HasValue)
        {
            query = query.Where(c => c.FieldOfficerId == _currentUser.EmployeeId.Value);
        }

        var list = await query
            .Select(c => new CenterDto
            {
                Id = c.Id,
                CenterCode = c.CenterCode,
                CenterName = c.CenterName,
                BranchId = c.BranchId,
                BranchName = c.Branch != null ? c.Branch.BranchName : null,
                FieldOfficerId = c.FieldOfficerId,
                FieldOfficerName = c.FieldOfficer != null ? c.FieldOfficer.FullName : null,
                MeetingDay = c.MeetingDay,
                MeetingTime = c.MeetingTime.ToString(@"hh\:mm"),
                MeetingPlace = c.MeetingPlace,
                VillageOrTown = c.VillageOrTown,
                TotalGroups = c.Groups.Count,
                TotalMembers = c.Customers.Count
            })
            .ToListAsync();

        return Ok(ApiResponse<List<CenterDto>>.Ok(list));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CenterDto>>> CreateCenter([FromBody] CreateCenterDto req)
    {
        var branchId = req.BranchId;
        if (!_currentUser.IsAdminOrSuperAdmin && _currentUser.BranchId.HasValue)
        {
            branchId = _currentUser.BranchId.Value;
        }

        var fieldOfficerId = req.FieldOfficerId;
        if (_currentUser.IsFieldOfficer && _currentUser.EmployeeId.HasValue)
        {
            fieldOfficerId = _currentUser.EmployeeId.Value;
        }

        var count = await _db.Centers.CountAsync(c => c.BranchId == branchId);
        var center = new Center
        {
            CenterCode = $"CTR-{branchId:D2}-{(count + 1):D3}",
            CenterName = req.CenterName.Trim(),
            BranchId = branchId,
            FieldOfficerId = fieldOfficerId,
            MeetingDay = req.MeetingDay,
            MeetingTime = TimeSpan.TryParse(req.MeetingTime, out var t) ? t : new TimeSpan(10, 0, 0),
            MeetingPlace = req.MeetingPlace.Trim(),
            VillageOrTown = req.VillageOrTown.Trim(),
            Latitude = req.Latitude,
            Longitude = req.Longitude
        };

        _db.Centers.Add(center);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<CenterDto>.Ok(new CenterDto
        {
            Id = center.Id,
            CenterCode = center.CenterCode,
            CenterName = center.CenterName,
            BranchId = center.BranchId
        }, "Center created successfully."));
    }

    [HttpGet("groups")]
    public async Task<ActionResult<ApiResponse<List<GroupDto>>>> GetGroups([FromQuery] int? centerId, [FromQuery] int? branchId)
    {
        if (!_currentUser.IsAdminOrSuperAdmin && _currentUser.BranchId.HasValue)
        {
            branchId = _currentUser.BranchId.Value;
        }

        var query = _db.LoanGroups
            .Include(g => g.Center)
            .Include(g => g.Branch)
            .Include(g => g.GroupLeader)
            .Include(g => g.Members)
            .AsNoTracking()
            .AsQueryable();

        if (branchId.HasValue) query = query.Where(g => g.BranchId == branchId.Value);
        if (centerId.HasValue) query = query.Where(g => g.CenterId == centerId.Value);

        if (_currentUser.IsFieldOfficer && _currentUser.EmployeeId.HasValue)
        {
            query = query.Where(g => g.Center != null && g.Center.FieldOfficerId == _currentUser.EmployeeId.Value);
        }

        var list = await query
            .Select(g => new GroupDto
            {
                Id = g.Id,
                GroupCode = g.GroupCode,
                GroupName = g.GroupName,
                CenterId = g.CenterId,
                CenterName = g.Center != null ? g.Center.CenterName : null,
                BranchId = g.BranchId,
                BranchName = g.Branch != null ? g.Branch.BranchName : null,
                GroupLeaderId = g.GroupLeaderId,
                GroupLeaderName = g.GroupLeader != null ? g.GroupLeader.FullName : null,
                Status = g.Status,
                MemberCount = g.Members.Count
            })
            .ToListAsync();

        return Ok(ApiResponse<List<GroupDto>>.Ok(list));
    }

    [HttpPost("groups")]
    public async Task<ActionResult<ApiResponse<GroupDto>>> CreateGroup([FromBody] CreateGroupDto req)
    {
        var count = await _db.LoanGroups.CountAsync(g => g.CenterId == req.CenterId);
        var group = new LoanGroup
        {
            GroupCode = $"GRP-{req.CenterId:D3}-{(count + 1):D2}",
            GroupName = req.GroupName.Trim(),
            CenterId = req.CenterId,
            BranchId = req.BranchId,
            GroupLeaderId = req.GroupLeaderId,
            Status = GroupStatus.Active,
            FormedDate = DateTime.UtcNow
        };

        _db.LoanGroups.Add(group);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<GroupDto>.Ok(new GroupDto
        {
            Id = group.Id,
            GroupCode = group.GroupCode,
            GroupName = group.GroupName,
            CenterId = group.CenterId
        }, "JLG Group created successfully."));
    }
}
