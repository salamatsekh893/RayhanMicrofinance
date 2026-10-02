using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RayhanMicrofinance.Application.Common;
using RayhanMicrofinance.Application.Interfaces;
using RayhanMicrofinance.Domain.Entities;
using RayhanMicrofinance.Domain.Enums;
using RayhanMicrofinance.Infrastructure.Data;

namespace RayhanMicrofinance.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BranchesController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public BranchesController(ApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<Branch>>>> GetBranches()
    {
        var query = _db.Branches.AsNoTracking().AsQueryable();
        if (!_currentUser.IsAdminOrSuperAdmin && _currentUser.BranchId.HasValue)
        {
            query = query.Where(b => b.Id == _currentUser.BranchId.Value);
        }

        var branches = await query.OrderBy(b => b.Id).ToListAsync();
        return Ok(ApiResponse<List<Branch>>.Ok(branches));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<Branch>>> GetBranchById(int id)
    {
        var branch = await _db.Branches.FirstOrDefaultAsync(b => b.Id == id);
        if (branch == null) return NotFound(ApiResponse<Branch>.Fail("Branch not found"));
        return Ok(ApiResponse<Branch>.Ok(branch));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Branch>>> CreateBranch([FromBody] Branch req)
    {
        if (!_currentUser.IsAdminOrSuperAdmin)
        {
            return Forbid();
        }

        var company = await _db.Companies.FirstOrDefaultAsync();
        if (company == null) return BadRequest(ApiResponse<Branch>.Fail("Company profile must exist"));

        req.CompanyId = company.Id;
        req.BranchCode = string.IsNullOrWhiteSpace(req.BranchCode) ? $"BR-{(await _db.Branches.CountAsync() + 1):D3}" : req.BranchCode.Trim().ToUpper();
        req.BranchName = req.BranchName?.Trim().ToUpper() ?? "NEW REGIONAL BRANCH";
        req.City = req.City?.Trim() ?? string.Empty;
        req.State = string.IsNullOrWhiteSpace(req.State) ? "West Bengal" : req.State.Trim();
        req.IsHeadOffice = false;

        _db.Branches.Add(req);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<Branch>.Ok(req, "New branch opened successfully"));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<Branch>>> UpdateBranch(int id, [FromBody] Branch req)
    {
        if (!_currentUser.IsAdminOrSuperAdmin)
        {
            return Forbid();
        }

        var branch = await _db.Branches.FirstOrDefaultAsync(b => b.Id == id);
        if (branch == null) return NotFound(ApiResponse<Branch>.Fail("Branch not found"));

        branch.BranchName = string.IsNullOrWhiteSpace(req.BranchName) ? branch.BranchName : req.BranchName.Trim().ToUpper();
        branch.BranchCode = string.IsNullOrWhiteSpace(req.BranchCode) ? branch.BranchCode : req.BranchCode.Trim().ToUpper();
        branch.Address = req.Address ?? branch.Address;
        branch.City = req.City ?? branch.City;
        branch.State = req.State ?? branch.State;
        branch.Pincode = req.Pincode ?? branch.Pincode;
        branch.Phone = req.Phone ?? branch.Phone;
        branch.Email = req.Email ?? branch.Email;
        branch.ManagerName = req.ManagerName ?? branch.ManagerName;
        branch.AreaName = req.AreaName ?? branch.AreaName;
        branch.CurrentCashBalance = req.CurrentCashBalance;
        branch.CurrentBankBalance = req.CurrentBankBalance;

        await _db.SaveChangesAsync();
        return Ok(ApiResponse<Branch>.Ok(branch, "Branch details updated successfully"));
    }
}

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public EmployeesController(ApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<Employee>>>> GetEmployees([FromQuery] int? branchId)
    {
        if (_currentUser.IsFieldOfficer)
        {
            return Forbid();
        }

        if (!_currentUser.IsAdminOrSuperAdmin && _currentUser.BranchId.HasValue)
        {
            branchId = _currentUser.BranchId.Value;
        }

        var query = _db.Employees
            .Include(e => e.Branch)
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .AsNoTracking()
            .AsQueryable();

        if (branchId.HasValue) query = query.Where(e => e.BranchId == branchId.Value);

        var list = await query.ToListAsync();
        return Ok(ApiResponse<List<Employee>>.Ok(list));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Employee>>> CreateEmployee([FromBody] Employee emp)
    {
        if (_currentUser.IsFieldOfficer)
        {
            return Forbid();
        }

        if (!_currentUser.IsAdminOrSuperAdmin && _currentUser.BranchId.HasValue)
        {
            emp.BranchId = _currentUser.BranchId.Value;
        }

        var count = await _db.Employees.CountAsync(e => e.BranchId == emp.BranchId);
        emp.EmployeeCode = $"EMP-{emp.BranchId:D2}-{(count + 1):D3}";
        _db.Employees.Add(emp);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<Employee>.Ok(emp, "Employee added successfully"));
    }
}
