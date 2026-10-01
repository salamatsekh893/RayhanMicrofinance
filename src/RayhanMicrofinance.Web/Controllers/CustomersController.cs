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
        if (!_currentUser.IsAdminOrSuperAdmin && _currentUser.BranchId.HasValue)
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

        // Strict Field Officer portfolio isolation
        if (_currentUser.IsFieldOfficer && _currentUser.EmployeeId.HasValue)
        {
            query = query.Where(c => c.Center != null && c.Center.FieldOfficerId == _currentUser.EmployeeId.Value);
        }

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
                KycDocumentFrontUrl = c.KycDocumentFrontUrl,
                KycDocumentBackUrl = c.KycDocumentBackUrl,
                BankAccountNumber = c.BankAccountNumber,
                BankName = c.BankName,
                IfscCode = c.IfscCode,
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

        if (!_currentUser.IsAdminOrSuperAdmin && _currentUser.BranchId.HasValue && c.BranchId != _currentUser.BranchId.Value)
        {
            return Forbid();
        }

        if (_currentUser.IsFieldOfficer && _currentUser.EmployeeId.HasValue && c.Center != null && c.Center.FieldOfficerId.HasValue && c.Center.FieldOfficerId.Value != _currentUser.EmployeeId.Value)
        {
            return Forbid();
        }

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
            KycDocumentFrontUrl = c.KycDocumentFrontUrl,
            KycDocumentBackUrl = c.KycDocumentBackUrl,
            BankAccountNumber = c.BankAccountNumber,
            BankName = c.BankName,
            IfscCode = c.IfscCode,
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
        if (!_currentUser.IsAdminOrSuperAdmin && _currentUser.BranchId.HasValue)
        {
            branchId = _currentUser.BranchId.Value;
        }

        // Validate Field Officer center assignment
        if (_currentUser.IsFieldOfficer && _currentUser.EmployeeId.HasValue && req.CenterId.HasValue)
        {
            var center = await _db.Centers.FindAsync(req.CenterId.Value);
            if (center != null && center.FieldOfficerId.HasValue && center.FieldOfficerId.Value != _currentUser.EmployeeId.Value)
            {
                return BadRequest(ApiResponse<CustomerDto>.Fail("You can only enroll members into your own assigned centers."));
            }
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
            IsKycVerified = false // By default newly registered customer has pending KYC until documents are uploaded and verified
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
                BranchId = customer.BranchId,
                IsKycVerified = false
            }, "Customer registered successfully with KYC PENDING. Please upload verification documents."));
    }

    [HttpPost("{id}/upload-document")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> UploadDocument(int id, [FromBody] UploadDocumentRequest req)
    {
        var cust = await _db.Customers
            .Include(c => c.Branch)
            .Include(c => c.Center)
            .Include(c => c.Group)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cust == null) return NotFound(ApiResponse<CustomerDto>.Fail("Customer not found"));

        string? savedUrl = null;
        if (!string.IsNullOrWhiteSpace(req.Base64Data))
        {
            try
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "kyc", id.ToString());
                Directory.CreateDirectory(uploadsFolder);

                var ext = ".jpg";
                var base64Clean = req.Base64Data;
                if (req.Base64Data.Contains(","))
                {
                    var parts = req.Base64Data.Split(',');
                    base64Clean = parts[1];
                    if (parts[0].Contains("png")) ext = ".png";
                    else if (parts[0].Contains("pdf")) ext = ".pdf";
                    else if (parts[0].Contains("jpeg") || parts[0].Contains("jpg")) ext = ".jpg";
                }

                var fileName = $"{req.DocumentType.ToLower()}_{DateTime.UtcNow.Ticks}{ext}";
                var filePath = Path.Combine(uploadsFolder, fileName);
                var bytes = Convert.FromBase64String(base64Clean);
                await System.IO.File.WriteAllBytesAsync(filePath, bytes);
                savedUrl = $"/uploads/kyc/{id}/{fileName}";
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<CustomerDto>.Fail($"Failed to process document file: {ex.Message}"));
            }
        }

        switch (req.DocumentType?.ToLower())
        {
            case "aadhaar_front":
                if (savedUrl != null) cust.KycDocumentFrontUrl = savedUrl;
                if (!string.IsNullOrWhiteSpace(req.DocumentNumber)) cust.AadhaarNumber = req.DocumentNumber.Trim();
                break;
            case "aadhaar_back":
                if (savedUrl != null) cust.KycDocumentBackUrl = savedUrl;
                break;
            case "pan":
                if (!string.IsNullOrWhiteSpace(req.DocumentNumber)) cust.PanNumber = req.DocumentNumber.Trim();
                break;
            case "voter":
                if (!string.IsNullOrWhiteSpace(req.DocumentNumber)) cust.VoterIdNumber = req.DocumentNumber.Trim();
                break;
            case "photo":
                if (savedUrl != null) cust.PhotoUrl = savedUrl;
                break;
            case "signature":
                if (savedUrl != null) cust.SignatureUrl = savedUrl;
                break;
            case "passbook":
                if (!string.IsNullOrWhiteSpace(req.DocumentNumber)) cust.BankAccountNumber = req.DocumentNumber.Trim();
                break;
        }

        await _db.SaveChangesAsync();

        var dto = new CustomerDto
        {
            Id = cust.Id,
            CustomerCode = cust.CustomerCode,
            BranchId = cust.BranchId,
            BranchName = cust.Branch?.BranchName,
            CenterId = cust.CenterId,
            CenterName = cust.Center?.CenterName,
            GroupId = cust.GroupId,
            GroupName = cust.Group?.GroupName,
            FirstName = cust.FirstName,
            LastName = cust.LastName,
            GuardianName = cust.GuardianName,
            RelationWithGuardian = cust.RelationWithGuardian,
            Gender = cust.Gender,
            DateOfBirth = cust.DateOfBirth,
            MaritalStatus = cust.MaritalStatus,
            Phone = cust.Phone,
            Email = cust.Email,
            Address = cust.Address,
            City = cust.City,
            State = cust.State,
            Pincode = cust.Pincode,
            Occupation = cust.Occupation,
            MonthlyIncome = cust.MonthlyIncome,
            PrimaryKycType = cust.PrimaryKycType,
            AadhaarNumber = cust.AadhaarNumber,
            PanNumber = cust.PanNumber,
            VoterIdNumber = cust.VoterIdNumber,
            IsKycVerified = cust.IsKycVerified,
            PhotoUrl = cust.PhotoUrl,
            SignatureUrl = cust.SignatureUrl,
            KycDocumentFrontUrl = cust.KycDocumentFrontUrl,
            KycDocumentBackUrl = cust.KycDocumentBackUrl,
            BankAccountNumber = cust.BankAccountNumber,
            BankName = cust.BankName,
            IfscCode = cust.IfscCode,
            IsBlacklisted = cust.IsBlacklisted,
            CreatedAt = cust.CreatedAt
        };

        return Ok(ApiResponse<CustomerDto>.Ok(dto, "KYC Document uploaded and attached to member record."));
    }

    [HttpPost("{id}/verify-kyc")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> VerifyKyc(int id, [FromQuery] bool isVerified = true)
    {
        var cust = await _db.Customers
            .Include(c => c.Branch)
            .Include(c => c.Center)
            .Include(c => c.Group)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cust == null) return NotFound(ApiResponse<CustomerDto>.Fail("Customer not found"));

        cust.IsKycVerified = isVerified;
        await _db.SaveChangesAsync();

        var dto = new CustomerDto
        {
            Id = cust.Id,
            CustomerCode = cust.CustomerCode,
            FirstName = cust.FirstName,
            LastName = cust.LastName,
            AadhaarNumber = cust.AadhaarNumber,
            IsKycVerified = cust.IsKycVerified
        };

        var msg = isVerified ? "Member KYC has been verified and approved." : "Member KYC set to PENDING.";
        return Ok(ApiResponse<CustomerDto>.Ok(dto, msg));
    }
}

public class UploadDocumentRequest
{
    public string DocumentType { get; set; } = string.Empty;
    public string? DocumentNumber { get; set; }
    public string? Base64Data { get; set; }
    public string? FileName { get; set; }
}
