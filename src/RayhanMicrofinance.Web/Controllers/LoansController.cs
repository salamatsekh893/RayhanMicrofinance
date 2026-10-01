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
public class LoansController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ILoanCalculationService _loanCalc;
    private readonly IAccountingService _accounting;
    private readonly ICurrentUserService _currentUser;

    public LoansController(
        ApplicationDbContext db,
        ILoanCalculationService loanCalc,
        IAccountingService accounting,
        ICurrentUserService currentUser)
    {
        _db = db;
        _loanCalc = loanCalc;
        _accounting = accounting;
        _currentUser = currentUser;
    }

    [HttpGet("schemes")]
    public async Task<ActionResult<ApiResponse<List<LoanSchemeDto>>>> GetSchemes()
    {
        var schemes = await _db.LoanSchemes
            .Where(s => s.IsActive)
            .Select(s => new LoanSchemeDto
            {
                Id = s.Id,
                SchemeCode = s.SchemeCode,
                SchemeName = s.SchemeName,
                LoanType = s.LoanType,
                MinAmount = s.MinAmount,
                MaxAmount = s.MaxAmount,
                DefaultAmount = s.DefaultAmount,
                MinTenureMonths = s.MinTenureMonths,
                MaxTenureMonths = s.MaxTenureMonths,
                DefaultTenureMonths = s.DefaultTenureMonths,
                InterestRatePerAnnum = s.InterestRatePerAnnum,
                InterestCalculationMethod = s.InterestCalculationMethod,
                RepaymentFrequency = s.RepaymentFrequency,
                ProcessingFeePercentage = s.ProcessingFeePercentage,
                InsuranceFeePercentage = s.InsuranceFeePercentage,
                LateFinePercentagePerDay = s.LateFinePercentagePerDay,
                GracePeriodDays = s.GracePeriodDays
            })
            .ToListAsync();

        return Ok(ApiResponse<List<LoanSchemeDto>>.Ok(schemes));
    }

    [HttpPost("schemes")]
    public async Task<ActionResult<ApiResponse<LoanSchemeDto>>> CreateScheme([FromBody] CreateLoanSchemeDto req)
    {
        var count = await _db.LoanSchemes.CountAsync();
        var scheme = new LoanScheme
        {
            SchemeCode = $"SCH-{(count + 1):D2}",
            SchemeName = req.SchemeName,
            LoanType = req.LoanType,
            MinAmount = req.MinAmount,
            MaxAmount = req.MaxAmount,
            DefaultAmount = req.DefaultAmount,
            MinTenureMonths = req.MinTenureMonths,
            MaxTenureMonths = req.MaxTenureMonths,
            DefaultTenureMonths = req.DefaultTenureMonths,
            InterestRatePerAnnum = req.InterestRatePerAnnum,
            InterestCalculationMethod = req.InterestCalculationMethod,
            RepaymentFrequency = req.RepaymentFrequency,
            ProcessingFeePercentage = req.ProcessingFeePercentage,
            InsuranceFeePercentage = req.InsuranceFeePercentage,
            LateFinePercentagePerDay = req.LateFinePercentagePerDay,
            GracePeriodDays = req.GracePeriodDays,
            Description = req.Description
        };

        _db.LoanSchemes.Add(scheme);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<LoanSchemeDto>.Ok(new LoanSchemeDto
        {
            Id = scheme.Id,
            SchemeCode = scheme.SchemeCode,
            SchemeName = scheme.SchemeName
        }, "Loan Scheme created successfully"));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<LoanApplicationDto>>>> GetLoans(
        [FromQuery] LoanStatus? status,
        [FromQuery] int? branchId,
        [FromQuery] int? customerId,
        [FromQuery] string? search,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 15)
    {
        if (!_currentUser.IsAdminOrSuperAdmin && _currentUser.BranchId.HasValue)
        {
            branchId = _currentUser.BranchId.Value;
        }

        var query = _db.LoanApplications
            .Include(l => l.Customer)
            .Include(l => l.Branch)
            .Include(l => l.Center)
            .Include(l => l.Group)
            .Include(l => l.LoanScheme)
            .AsNoTracking()
            .AsQueryable();

        if (status.HasValue) query = query.Where(l => l.Status == status.Value);
        if (branchId.HasValue) query = query.Where(l => l.BranchId == branchId.Value);

        if (_currentUser.IsFieldOfficer && _currentUser.EmployeeId.HasValue)
        {
            query = query.Where(l => l.Center != null && l.Center.FieldOfficerId == _currentUser.EmployeeId.Value);
        }

        if (customerId.HasValue) query = query.Where(l => l.CustomerId == customerId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(l =>
                l.LoanAccountNumber.ToLower().Contains(s) ||
                (l.Customer != null && (l.Customer.FirstName.ToLower().Contains(s) || l.Customer.LastName.ToLower().Contains(s) || l.Customer.Phone.Contains(s))));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(l => l.Id)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LoanApplicationDto
            {
                Id = l.Id,
                LoanAccountNumber = l.LoanAccountNumber,
                CustomerId = l.CustomerId,
                CustomerName = l.Customer != null ? l.Customer.FullName : null,
                CustomerPhone = l.Customer != null ? l.Customer.Phone : null,
                BranchId = l.BranchId,
                BranchName = l.Branch != null ? l.Branch.BranchName : null,
                CenterId = l.CenterId,
                CenterName = l.Center != null ? l.Center.CenterName : null,
                GroupId = l.GroupId,
                GroupName = l.Group != null ? l.Group.GroupName : null,
                LoanSchemeId = l.LoanSchemeId,
                LoanSchemeName = l.LoanScheme != null ? l.LoanScheme.SchemeName : null,
                LoanType = l.LoanType,
                RequestedAmount = l.RequestedAmount,
                ApprovedAmount = l.ApprovedAmount,
                DisbursedAmount = l.DisbursedAmount,
                InterestRatePerAnnum = l.InterestRatePerAnnum,
                CalculationMethod = l.CalculationMethod,
                Frequency = l.Frequency,
                TenureInMonths = l.TenureInMonths,
                TotalInstallments = l.TotalInstallments,
                TotalInterest = l.TotalInterest,
                TotalPayable = l.TotalPayable,
                TotalPaid = l.TotalPaid,
                OutstandingPrincipal = l.OutstandingPrincipal,
                OutstandingInterest = l.OutstandingInterest,
                ProcessingFee = l.ProcessingFee,
                InsuranceFee = l.InsuranceFee,
                Status = l.Status,
                ApplicationDate = l.ApplicationDate,
                ApprovedDate = l.ApprovedDate,
                DisbursedDate = l.DisbursedDate,
                FirstEmiDate = l.FirstEmiDate,
                MaturityDate = l.MaturityDate,
                PurposeOfLoan = l.PurposeOfLoan
            })
            .ToListAsync();

        return Ok(ApiResponse<PagedResult<LoanApplicationDto>>.Ok(new PagedResult<LoanApplicationDto>
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalCount = total,
            Items = items
        }));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<LoanApplicationDto>>> GetLoanById(int id)
    {
        var l = await _db.LoanApplications
            .Include(x => x.Customer)
            .Include(x => x.Branch)
            .Include(x => x.Center)
            .Include(x => x.Group)
            .Include(x => x.LoanScheme)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (l == null) return NotFound(ApiResponse<LoanApplicationDto>.Fail("Loan not found"));

        var dto = new LoanApplicationDto
        {
            Id = l.Id,
            LoanAccountNumber = l.LoanAccountNumber,
            CustomerId = l.CustomerId,
            CustomerName = l.Customer?.FullName,
            CustomerPhone = l.Customer?.Phone,
            BranchId = l.BranchId,
            BranchName = l.Branch?.BranchName,
            CenterId = l.CenterId,
            CenterName = l.Center?.CenterName,
            GroupId = l.GroupId,
            GroupName = l.Group?.GroupName,
            LoanSchemeId = l.LoanSchemeId,
            LoanSchemeName = l.LoanScheme?.SchemeName,
            LoanType = l.LoanType,
            RequestedAmount = l.RequestedAmount,
            ApprovedAmount = l.ApprovedAmount,
            DisbursedAmount = l.DisbursedAmount,
            InterestRatePerAnnum = l.InterestRatePerAnnum,
            CalculationMethod = l.CalculationMethod,
            Frequency = l.Frequency,
            TenureInMonths = l.TenureInMonths,
            TotalInstallments = l.TotalInstallments,
            TotalInterest = l.TotalInterest,
            TotalPayable = l.TotalPayable,
            TotalPaid = l.TotalPaid,
            OutstandingPrincipal = l.OutstandingPrincipal,
            OutstandingInterest = l.OutstandingInterest,
            ProcessingFee = l.ProcessingFee,
            InsuranceFee = l.InsuranceFee,
            Status = l.Status,
            ApplicationDate = l.ApplicationDate,
            ApprovedDate = l.ApprovedDate,
            DisbursedDate = l.DisbursedDate,
            FirstEmiDate = l.FirstEmiDate,
            MaturityDate = l.MaturityDate,
            PurposeOfLoan = l.PurposeOfLoan
        };

        return Ok(ApiResponse<LoanApplicationDto>.Ok(dto));
    }

    [HttpGet("{id}/schedule")]
    public async Task<ActionResult<ApiResponse<List<LoanEmiScheduleDto>>>> GetLoanSchedule(int id)
    {
        var schedules = await _db.LoanEmiSchedules
            .Where(s => s.LoanApplicationId == id)
            .OrderBy(s => s.InstallmentNumber)
            .Select(s => new LoanEmiScheduleDto
            {
                Id = s.Id,
                InstallmentNumber = s.InstallmentNumber,
                DueDate = s.DueDate,
                PrincipalAmount = s.PrincipalAmount,
                InterestAmount = s.InterestAmount,
                TotalEmiAmount = s.PrincipalAmount + s.InterestAmount,
                PaidPrincipal = s.PaidPrincipal,
                PaidInterest = s.PaidInterest,
                PaidPenalty = s.PaidPenalty,
                TotalPaidAmount = s.PaidPrincipal + s.PaidInterest + s.PaidPenalty,
                OutstandingPrincipal = s.PrincipalAmount - s.PaidPrincipal,
                OutstandingInterest = s.InterestAmount - s.PaidInterest,
                TotalOutstanding = (s.PrincipalAmount - s.PaidPrincipal) + (s.InterestAmount - s.PaidInterest),
                PenaltyAmount = s.PenaltyAmount,
                Status = s.Status,
                PaidDate = s.PaidDate
            })
            .ToListAsync();

        return Ok(ApiResponse<List<LoanEmiScheduleDto>>.Ok(schedules));
    }

    [HttpPost("preview-schedule")]
    public ActionResult<ApiResponse<List<EmiScheduleItem>>> PreviewSchedule(
        [FromQuery] decimal amount,
        [FromQuery] decimal rate,
        [FromQuery] int tenureMonths,
        [FromQuery] RepaymentFrequency frequency,
        [FromQuery] InterestCalculationMethod method)
    {
        var schedule = _loanCalc.GenerateSchedule(amount, rate, tenureMonths, frequency, method, DateTime.UtcNow);
        return Ok(ApiResponse<List<EmiScheduleItem>>.Ok(schedule));
    }

    [HttpPost("apply")]
    public async Task<ActionResult<ApiResponse<LoanApplicationDto>>> ApplyForLoan([FromBody] CreateLoanApplicationDto req)
    {
        var customer = await _db.Customers.FindAsync(req.CustomerId);
        if (customer == null) return BadRequest(ApiResponse<LoanApplicationDto>.Fail("Customer not found."));

        if (!_currentUser.IsAdminOrSuperAdmin && _currentUser.BranchId.HasValue && customer.BranchId != _currentUser.BranchId.Value)
        {
            return Forbid();
        }

        if (_currentUser.IsFieldOfficer && _currentUser.EmployeeId.HasValue && customer.CenterId.HasValue)
        {
            var center = await _db.Centers.FindAsync(customer.CenterId.Value);
            if (center != null && center.FieldOfficerId.HasValue && center.FieldOfficerId.Value != _currentUser.EmployeeId.Value)
            {
                return BadRequest(ApiResponse<LoanApplicationDto>.Fail("You can only apply for loans on behalf of members in your assigned centers."));
            }
        }

        var scheme = await _db.LoanSchemes.FindAsync(req.LoanSchemeId);
        if (scheme == null) return BadRequest(ApiResponse<LoanApplicationDto>.Fail("Loan scheme not found."));

        if (req.RequestedAmount < scheme.MinAmount || req.RequestedAmount > scheme.MaxAmount)
        {
            return BadRequest(ApiResponse<LoanApplicationDto>.Fail($"Requested amount must be between {scheme.MinAmount} and {scheme.MaxAmount}."));
        }

        var branchId = customer.BranchId;
        var count = await _db.LoanApplications.CountAsync(l => l.BranchId == branchId);
        var loanAccNo = $"LN-{DateTime.UtcNow.Year}-{(count + 1):D5}";

        var loan = new LoanApplication
        {
            LoanAccountNumber = loanAccNo,
            CustomerId = customer.Id,
            BranchId = branchId,
            CenterId = req.CenterId ?? customer.CenterId,
            GroupId = req.GroupId ?? customer.GroupId,
            LoanSchemeId = scheme.Id,
            LoanType = scheme.LoanType,
            RequestedAmount = req.RequestedAmount,
            ApprovedAmount = 0,
            DisbursedAmount = 0,
            InterestRatePerAnnum = scheme.InterestRatePerAnnum,
            CalculationMethod = scheme.InterestCalculationMethod,
            Frequency = scheme.RepaymentFrequency,
            TenureInMonths = req.TenureInMonths > 0 ? req.TenureInMonths : scheme.DefaultTenureMonths,
            Status = LoanStatus.Applied,
            ApplicationDate = DateTime.UtcNow,
            PurposeOfLoan = req.PurposeOfLoan
        };

        _db.LoanApplications.Add(loan);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<LoanApplicationDto>.Ok(new LoanApplicationDto
        {
            Id = loan.Id,
            LoanAccountNumber = loan.LoanAccountNumber,
            Status = loan.Status
        }, "Loan application submitted successfully."));
    }

    [HttpPost("{id}/approve")]
    public async Task<ActionResult<ApiResponse<bool>>> ApproveLoan(int id, [FromBody] ApproveLoanDto req)
    {
        if (_currentUser.IsFieldOfficer)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<bool>.Fail("Field Officers cannot approve loans. Requires Branch Manager or Admin approval."));
        }

        var loan = await _db.LoanApplications.FindAsync(id);
        if (loan == null) return NotFound(ApiResponse<bool>.Fail("Loan not found"));

        if (!_currentUser.IsAdminOrSuperAdmin && _currentUser.BranchId.HasValue && loan.BranchId != _currentUser.BranchId.Value)
        {
            return Forbid();
        }

        if (loan.Status != LoanStatus.Applied && loan.Status != LoanStatus.UnderVerification)
        {
            return BadRequest(ApiResponse<bool>.Fail("Loan cannot be approved in its current status."));
        }

        loan.ApprovedAmount = req.ApprovedAmount > 0 ? req.ApprovedAmount : loan.RequestedAmount;
        loan.Status = LoanStatus.Approved;
        loan.ApprovedDate = DateTime.UtcNow;
        loan.ApprovedBy = _currentUser.Username ?? "Admin";
        loan.ApprovalRemarks = req.Remarks;

        await _db.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true, $"Loan approved for {loan.ApprovedAmount:C}"));
    }

    [HttpPost("{id}/disburse")]
    public async Task<ActionResult<ApiResponse<bool>>> DisburseLoan(int id, [FromBody] DisburseLoanDto req)
    {
        if (_currentUser.IsFieldOfficer)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<bool>.Fail("Field Officers cannot disburse loans. Cashier or Branch Manager required."));
        }

        var loan = await _db.LoanApplications
            .Include(l => l.Customer)
            .Include(l => l.LoanScheme)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (loan == null) return NotFound(ApiResponse<bool>.Fail("Loan not found"));

        if (!_currentUser.IsAdminOrSuperAdmin && _currentUser.BranchId.HasValue && loan.BranchId != _currentUser.BranchId.Value)
        {
            return Forbid();
        }

        if (loan.Status != LoanStatus.Approved)
        {
            return BadRequest(ApiResponse<bool>.Fail("Loan must be approved before disbursement."));
        }

        var disburseAmount = req.DisbursedAmount > 0 ? req.DisbursedAmount : loan.ApprovedAmount;
        loan.DisbursedAmount = disburseAmount;
        loan.DisbursedDate = req.DisbursementDate != default ? req.DisbursementDate : DateTime.UtcNow;
        loan.DisbursedBy = _currentUser.Username ?? "Admin";
        loan.DisbursementMode = req.DisbursementMode;
        loan.FirstEmiDate = req.FirstEmiDate != default ? req.FirstEmiDate : loan.DisbursedDate.Value.AddDays(7);
        loan.Status = LoanStatus.Active;

        // Calculate Fees
        if (loan.LoanScheme != null)
        {
            loan.ProcessingFee = Math.Round(disburseAmount * (loan.LoanScheme.ProcessingFeePercentage / 100m), 2);
            loan.InsuranceFee = Math.Round(disburseAmount * (loan.LoanScheme.InsuranceFeePercentage / 100m), 2);
        }

        // Generate EMI Schedule
        var scheduleItems = _loanCalc.GenerateSchedule(
            disburseAmount,
            loan.InterestRatePerAnnum,
            loan.TenureInMonths,
            loan.Frequency,
            loan.CalculationMethod,
            loan.DisbursedDate.Value);

        loan.TotalInstallments = scheduleItems.Count;
        loan.TotalInterest = scheduleItems.Sum(s => s.InterestAmount);
        loan.TotalPayable = disburseAmount + loan.TotalInterest;
        loan.OutstandingPrincipal = disburseAmount;
        loan.OutstandingInterest = loan.TotalInterest;
        loan.TotalPaid = 0;
        loan.MaturityDate = scheduleItems.LastOrDefault()?.DueDate;

        // Save EMI Schedule entities
        foreach (var item in scheduleItems)
        {
            loan.EmiSchedules.Add(new LoanEmiSchedule
            {
                LoanApplicationId = loan.Id,
                InstallmentNumber = item.InstallmentNumber,
                DueDate = item.DueDate,
                PrincipalAmount = item.PrincipalAmount,
                InterestAmount = item.InterestAmount,
                Status = EmiStatus.Pending
            });
        }

        // Post Accounting Voucher automatically!
        await _accounting.PostDisbursementVoucherAsync(loan);

        // Update Branch Cash/Bank Balance
        var branch = await _db.Branches.FindAsync(loan.BranchId);
        if (branch != null)
        {
            if (loan.DisbursementMode == PaymentMode.Cash)
                branch.CurrentCashBalance -= disburseAmount;
            else
                branch.CurrentBankBalance -= disburseAmount;
        }

        await _db.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true, "Loan disbursed successfully and EMI schedule generated."));
    }
}
