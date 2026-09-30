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
public class CustomersController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CustomersController(ApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<CustomerDto>>>> GetCustomers(
        [FromQuery] string? search,
        [FromQuery] int? branchId,
        [FromQuery] int? centerId,
        [FromQuery] int? groupId,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 15)
    {
        if (!_currentUser.IsSuperAdmin && _currentUser.BranchId.HasValue)
        {
            branchId = _currentUser.BranchId.Value;
        }

        var query = _db.Customers
            .Include(c => c.Branch)
            .Include(c => c.Center)
            .Include(c => c.Group)
            .AsNoTracking()
            .AsQueryable();

        if (branchId.HasValue) query = query.Where(c => c.BranchId == branchId.Value);
        if (centerId.HasValue) query = query.Where(c => c.CenterId == centerId.Value);
        if (groupId.HasValue) query = query.Where(c => c.GroupId == groupId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(c =>
                c.FirstName.ToLower().Contains(s) ||
                c.LastName.ToLower().Contains(s) ||
                c.CustomerCode.ToLower().Contains(s) ||
                c.Phone.Contains(s) ||
                c.AadhaarNumber.Contains(s));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(c => c.Id)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                CustomerCode = c.CustomerCode,
                BranchId = c.BranchId,
                BranchName = c.Branch != null ? c.Branch.BranchName : null,
                CenterId = c.CenterId,
                CenterName = c.Center != null ? c.Center.CenterName : null,
                GroupId = c.GroupId,
                GroupName = c.Group != null ? c.Group.GroupName : null,
                FirstName = c.FirstName,
                LastName = c.LastName,
                GuardianName = c.GuardianName,
                RelationWithGuardian = c.RelationWithGuardian,
                Gender = c.Gender,
                DateOfBirth = c.DateOfBirth,
                MaritalStatus = c.MaritalStatus,
                Phone = c.Phone,
                Email = c.Email,
                Address = c.Address,
                City = c.City,
                State = c.State,
                Pincode = c.Pincode,
                Occupation = c.Occupation,
                MonthlyIncome = c.MonthlyIncome,
                FamilyMemberCount = c.FamilyMemberCount,
                PrimaryKycType = c.PrimaryKycType,
                AadhaarNumber = c.AadhaarNumber,
                PanNumber = c.PanNumber,
                VoterIdNumber = c.VoterIdNumber,
                IsKycVerified = c.IsKycVerified,
                PhotoUrl = c.PhotoUrl,
                SignatureUrl = c.SignatureUrl,
                Latitude = c.Latitude,
                Longitude = c.Longitude,
                NomineeName = c.NomineeName,
                NomineeRelation = c.NomineeRelation,
                NomineePhone = c.NomineePhone,
                GuarantorName = c.GuarantorName,
                GuarantorPhone = c.GuarantorPhone,
                IsBlacklisted = c.IsBlacklisted,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        var result = new PagedResult<CustomerDto>
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalCount = total,
            Items = items
        };

        return Ok(ApiResponse<PagedResult<CustomerDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> GetCustomerById(int id)
    {
        var c = await _db.Customers
            .Include(x => x.Branch)
            .Include(x => x.Center)
            .Include(x => x.Group)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (c == null) return NotFound(ApiResponse<CustomerDto>.Fail("Customer not found"));

        var dto = new CustomerDto
        {
            Id = c.Id,
            CustomerCode = c.CustomerCode,
            BranchId = c.BranchId,
            BranchName = c.Branch?.BranchName,
            CenterId = c.CenterId,
            CenterName = c.Center?.CenterName,
            GroupId = c.GroupId,
            GroupName = c.Group?.GroupName,
            FirstName = c.FirstName,
            LastName = c.LastName,
            GuardianName = c.GuardianName,
            RelationWithGuardian = c.RelationWithGuardian,
            Gender = c.Gender,
            DateOfBirth = c.DateOfBirth,
            MaritalStatus = c.MaritalStatus,
            Phone = c.Phone,
            Email = c.Email,
            Address = c.Address,
            City = c.City,
            State = c.State,
            Pincode = c.Pincode,
            Occupation = c.Occupation,
            MonthlyIncome = c.MonthlyIncome,
            FamilyMemberCount = c.FamilyMemberCount,
            PrimaryKycType = c.PrimaryKycType,
            AadhaarNumber = c.AadhaarNumber,
            PanNumber = c.PanNumber,
            VoterIdNumber = c.VoterIdNumber,
            IsKycVerified = c.IsKycVerified,
            PhotoUrl = c.PhotoUrl,
            SignatureUrl = c.SignatureUrl,
            Latitude = c.Latitude,
            Longitude = c.Longitude,
            NomineeName = c.NomineeName,
            NomineeRelation = c.NomineeRelation,
            NomineePhone = c.NomineePhone,
            GuarantorName = c.GuarantorName,
            GuarantorPhone = c.GuarantorPhone,
            IsBlacklisted = c.IsBlacklisted,
            CreatedAt = c.CreatedAt
        };

        return Ok(ApiResponse<CustomerDto>.Ok(dto));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> CreateCustomer([FromBody] CreateCustomerDto req)
    {
        if (string.IsNullOrWhiteSpace(req.FirstName) || string.IsNullOrWhiteSpace(req.Phone))
        {
            return BadRequest(ApiResponse<CustomerDto>.Fail("First name and phone number are required."));
        }

        var branchId = req.BranchId;
        if (!_currentUser.IsSuperAdmin && _currentUser.BranchId.HasValue)
        {
            branchId = _currentUser.BranchId.Value;
        }

        // Generate Customer Code
        var count = await _db.Customers.CountAsync(c => c.BranchId == branchId);
        var branch = await _db.Branches.FindAsync(branchId);
        var branchCode = branch?.BranchCode ?? "BR";
        var custCode = $"CUST-{branchCode}-{DateTime.UtcNow.Year % 100}{(count + 1):D4}";

        var customer = new Customer
        {
            CustomerCode = custCode,
            BranchId = branchId,
            CenterId = req.CenterId,
            GroupId = req.GroupId,
            FirstName = req.FirstName.Trim(),
            LastName = req.LastName.Trim(),
            GuardianName = req.GuardianName.Trim(),
            RelationWithGuardian = req.RelationWithGuardian,
            Gender = req.Gender,
            DateOfBirth = req.DateOfBirth,
            MaritalStatus = req.MaritalStatus,
            Phone = req.Phone.Trim(),
            Email = req.Email?.Trim(),
            Address = req.Address.Trim(),
            Landmark = req.Landmark.Trim(),
            VillageOrTown = req.VillageOrTown.Trim(),
            City = req.City.Trim(),
            State = req.State.Trim(),
            Pincode = req.Pincode.Trim(),
            Occupation = req.Occupation.Trim(),
            MonthlyIncome = req.MonthlyIncome,
            FamilyMemberCount = req.FamilyMemberCount,
            TotalFamilyIncome = req.TotalFamilyIncome,
            PrimaryKycType = req.PrimaryKycType,
            AadhaarNumber = req.AadhaarNumber.Trim(),
            PanNumber = req.PanNumber?.Trim(),
            VoterIdNumber = req.VoterIdNumber?.Trim(),
            PassportNumber = req.PassportNumber?.Trim(),
            PhotoUrl = req.PhotoUrl,
            SignatureUrl = req.SignatureUrl,
            Latitude = req.Latitude,
            Longitude = req.Longitude,
            NomineeName = req.NomineeName.Trim(),
            NomineeRelation = req.NomineeRelation.Trim(),
            NomineePhone = req.NomineePhone?.Trim(),
            NomineeAadhaar = req.NomineeAadhaar?.Trim(),
            GuarantorName = req.GuarantorName?.Trim(),
            GuarantorRelation = req.GuarantorRelation?.Trim(),
            GuarantorPhone = req.GuarantorPhone?.Trim(),
            GuarantorAddress = req.GuarantorAddress?.Trim(),
            GuarantorAadhaar = req.GuarantorAadhaar?.Trim(),
            BankAccountNumber = req.BankAccountNumber?.Trim(),
            BankName = req.BankName?.Trim(),
            IfscCode = req.IfscCode?.Trim(),
            IsKycVerified = true
        };

        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCustomerById), new { id = customer.Id },
            ApiResponse<CustomerDto>.Ok(new CustomerDto
            {
                Id = customer.Id,
                CustomerCode = customer.CustomerCode,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Phone = customer.Phone,
                BranchId = customer.BranchId
            }, "Customer registered successfully."));
    }

    [HttpPost("{id}/verify-kyc")]
    public async Task<ActionResult<ApiResponse<bool>>> VerifyKyc(int id)
    {
        var cust = await _db.Customers.FindAsync(id);
        if (cust == null) return NotFound(ApiResponse<bool>.Fail("Customer not found"));

        cust.IsKycVerified = true;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Ok(true, "KYC verified successfully"));
    }
}
