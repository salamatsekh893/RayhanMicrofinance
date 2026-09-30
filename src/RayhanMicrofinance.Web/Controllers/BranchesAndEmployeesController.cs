using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RayhanMicrofinance.Application.Common;
using RayhanMicrofinance.Domain.Entities;
using RayhanMicrofinance.Domain.Enums;
using RayhanMicrofinance.Infrastructure.Data;

namespace RayhanMicrofinance.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BranchesController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public BranchesController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<Branch>>>> GetBranches()
    {
        var branches = await _db.Branches.AsNoTracking().ToListAsync();
        return Ok(ApiResponse<List<Branch>>.Ok(branches));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Branch>>> CreateBranch([FromBody] Branch req)
    {
        var company = await _db.Companies.FirstOrDefaultAsync();
        if (company == null) return BadRequest(ApiResponse<Branch>.Fail("Company profile must exist"));

        req.CompanyId = company.Id;
        _db.Branches.Add(req);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<Branch>.Ok(req, "Branch created successfully"));
    }
}

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public EmployeesController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<Employee>>>> GetEmployees([FromQuery] int? branchId)
    {
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
        var count = await _db.Employees.CountAsync(e => e.BranchId == emp.BranchId);
        emp.EmployeeCode = $"EMP-{emp.BranchId:D2}-{(count + 1):D3}";
        _db.Employees.Add(emp);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<Employee>.Ok(emp, "Employee added successfully"));
    }
}
