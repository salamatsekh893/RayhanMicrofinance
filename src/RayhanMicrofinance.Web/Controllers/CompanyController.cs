using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RayhanMicrofinance.Application.Common;
using RayhanMicrofinance.Application.Interfaces;
using RayhanMicrofinance.Domain.Entities;
using RayhanMicrofinance.Infrastructure.Data;

namespace RayhanMicrofinance.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompanyController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CompanyController(ApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<Company>>> GetCompany()
    {
        var company = await _db.Companies.Include(c => c.Branches).FirstOrDefaultAsync();
        if (company == null)
        {
            company = new Company
            {
                Name = "My Microfinance Company",
                Code = "MF-01",
                Country = "India",
                CurrencySymbol = "₹",
                CurrencyCode = "INR"
            };
            _db.Companies.Add(company);
            await _db.SaveChangesAsync();
        }

        return Ok(ApiResponse<Company>.Ok(company));
    }

    [HttpPut]
    public async Task<ActionResult<ApiResponse<Company>>> UpdateCompany([FromBody] Company model)
    {
        if (!_currentUser.IsAdminOrSuperAdmin)
        {
            return Forbid();
        }

        var company = await _db.Companies.FirstOrDefaultAsync();
        if (company == null)
        {
            company = new Company();
            _db.Companies.Add(company);
        }

        company.Name = string.IsNullOrWhiteSpace(model.Name) ? company.Name : model.Name.Trim();
        company.Code = string.IsNullOrWhiteSpace(model.Code) ? company.Code : model.Code.Trim();
        company.RegistrationNumber = model.RegistrationNumber ?? string.Empty;
        company.TaxNumber = model.TaxNumber ?? string.Empty;
        company.Address = model.Address ?? string.Empty;
        company.City = model.City ?? string.Empty;
        company.State = model.State ?? string.Empty;
        company.Pincode = model.Pincode ?? string.Empty;
        company.Country = string.IsNullOrWhiteSpace(model.Country) ? "India" : model.Country.Trim();
        company.Phone = model.Phone ?? string.Empty;
        company.Email = model.Email ?? string.Empty;
        company.Website = model.Website ?? string.Empty;
        company.CurrencySymbol = string.IsNullOrWhiteSpace(model.CurrencySymbol) ? "₹" : model.CurrencySymbol.Trim();
        company.CurrencyCode = string.IsNullOrWhiteSpace(model.CurrencyCode) ? "INR" : model.CurrencyCode.Trim();
        company.UpdatedAt = DateTime.UtcNow;
        company.UpdatedBy = _currentUser.Username;

        // Automatically sync Head Office branch details
        var headOffice = await _db.Branches.FirstOrDefaultAsync(b => b.IsHeadOffice);
        if (headOffice != null)
        {
            if (!string.IsNullOrWhiteSpace(company.City))
            {
                headOffice.BranchName = $"{company.City.Trim().ToUpper()} HEAD OFFICE";
                headOffice.City = company.City;
            }
            if (!string.IsNullOrWhiteSpace(company.Address)) headOffice.Address = company.Address;
            if (!string.IsNullOrWhiteSpace(company.State)) headOffice.State = company.State;
            if (!string.IsNullOrWhiteSpace(company.Pincode)) headOffice.Pincode = company.Pincode;
            if (!string.IsNullOrWhiteSpace(company.Phone)) headOffice.Phone = company.Phone;
            if (!string.IsNullOrWhiteSpace(company.Email)) headOffice.Email = company.Email;
        }

        await _db.SaveChangesAsync();
        return Ok(ApiResponse<Company>.Ok(company, "Company profile updated successfully!"));
    }

    [HttpPost("logo")]
    public async Task<ActionResult<ApiResponse<string>>> UploadLogo([FromBody] CompanyLogoRequest req)
    {
        if (!_currentUser.IsAdminOrSuperAdmin)
        {
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(req.Base64Data))
        {
            return BadRequest(ApiResponse<string>.Fail("No image data provided."));
        }

        var company = await _db.Companies.FirstOrDefaultAsync();
        if (company == null)
        {
            company = new Company();
            _db.Companies.Add(company);
        }

        try
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "company");
            Directory.CreateDirectory(uploadsFolder);

            var ext = ".png";
            var base64Clean = req.Base64Data;
            if (req.Base64Data.Contains(","))
            {
                var parts = req.Base64Data.Split(',');
                base64Clean = parts[1];
                if (parts[0].Contains("jpeg") || parts[0].Contains("jpg")) ext = ".jpg";
                else if (parts[0].Contains("webp")) ext = ".webp";
                else if (parts[0].Contains("svg")) ext = ".svg";
            }

            var fileName = $"company_logo_{DateTime.UtcNow.Ticks}{ext}";
            var filePath = Path.Combine(uploadsFolder, fileName);
            var bytes = Convert.FromBase64String(base64Clean);
            await System.IO.File.WriteAllBytesAsync(filePath, bytes);

            company.LogoUrl = $"/uploads/company/{fileName}";
            company.UpdatedAt = DateTime.UtcNow;
            company.UpdatedBy = _currentUser.Username;
            await _db.SaveChangesAsync();

            return Ok(ApiResponse<string>.Ok(company.LogoUrl, "Company logo uploaded successfully!"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.Fail($"Failed to upload logo: {ex.Message}"));
        }
    }
}

public class CompanyLogoRequest
{
    public string Base64Data { get; set; } = string.Empty;
}
