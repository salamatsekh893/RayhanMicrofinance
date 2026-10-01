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
[Route("api/collections")]
public class CollectionController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IAccountingService _accounting;
    private readonly ICurrentUserService _currentUser;

    public CollectionController(
        ApplicationDbContext db,
        IAccountingService accounting,
        ICurrentUserService currentUser)
    {
        _db = db;
        _accounting = accounting;
        _currentUser = currentUser;
    }

    [HttpPost("collect")]
    public async Task<ActionResult<ApiResponse<LoanCollection>>> CollectPayment([FromBody] CreateLoanCollectionDto req)
    {
        if (req.Amount <= 0)
        {
            return BadRequest(ApiResponse<LoanCollection>.Fail("Collection amount must be greater than zero."));
        }

        var loan = await _db.LoanApplications
            .Include(l => l.Customer)
            .Include(l => l.EmiSchedules)
            .FirstOrDefaultAsync(l => l.Id == req.LoanApplicationId);

        if (loan == null) return NotFound(ApiResponse<LoanCollection>.Fail("Loan not found."));

        if (loan.Status != LoanStatus.Active)
        {
            return BadRequest(ApiResponse<LoanCollection>.Fail("Collections can only be made on Active loans."));
        }

        var receiptCount = await _db.LoanCollections.CountAsync(c => c.BranchId == loan.BranchId);
        var receiptNumber = $"RCP-{loan.BranchId:D2}-{DateTime.UtcNow.Year % 100}{(receiptCount + 1):D6}";

        // Allocate Collection to unpaid EMI schedules (Oldest pending first)
        var pendingEmis = loan.EmiSchedules
            .Where(e => e.Status != EmiStatus.Paid)
            .OrderBy(e => e.InstallmentNumber)
            .ToList();

        decimal remainingToAllocate = req.Amount;
        decimal totalAllocatedPrincipal = 0;
        decimal totalAllocatedInterest = 0;
        decimal totalAllocatedPenalty = 0;

        foreach (var emi in pendingEmis)
        {
            if (remainingToAllocate <= 0) break;

            // 1. Pay penalty first if any
            if (emi.PenaltyAmount > emi.PaidPenalty)
            {
                var duePenalty = emi.PenaltyAmount - emi.PaidPenalty;
                var payPen = Math.Min(remainingToAllocate, duePenalty);
                emi.PaidPenalty += payPen;
                totalAllocatedPenalty += payPen;
                remainingToAllocate -= payPen;
            }

            // 2. Pay interest
            if (remainingToAllocate > 0 && emi.InterestAmount > emi.PaidInterest)
            {
                var dueInt = emi.InterestAmount - emi.PaidInterest;
                var payInt = Math.Min(remainingToAllocate, dueInt);
                emi.PaidInterest += payInt;
                totalAllocatedInterest += payInt;
                remainingToAllocate -= payInt;
            }

            // 3. Pay principal
            if (remainingToAllocate > 0 && emi.PrincipalAmount > emi.PaidPrincipal)
            {
                var duePrin = emi.PrincipalAmount - emi.PaidPrincipal;
                var payPrin = Math.Min(remainingToAllocate, duePrin);
                emi.PaidPrincipal += payPrin;
                totalAllocatedPrincipal += payPrin;
                remainingToAllocate -= payPrin;
            }

            // Check if EMI is fully paid
            if (emi.PaidPrincipal >= emi.PrincipalAmount && emi.PaidInterest >= emi.InterestAmount)
            {
                emi.Status = EmiStatus.Paid;
                emi.PaidDate = req.CollectionDate != default ? req.CollectionDate : DateTime.UtcNow;
                emi.PaymentReference = receiptNumber;
            }
            else if (emi.PaidPrincipal > 0 || emi.PaidInterest > 0)
            {
                emi.Status = EmiStatus.PartiallyPaid;
            }
        }

        // Any excess amount reduces outstanding principal (Advance payment)
        if (remainingToAllocate > 0)
        {
            totalAllocatedPrincipal += remainingToAllocate;
            remainingToAllocate = 0;
        }

        // Update Loan Aggregate balances
        loan.TotalPaid += req.Amount;
        loan.OutstandingPrincipal = Math.Max(0, loan.OutstandingPrincipal - totalAllocatedPrincipal);
        loan.OutstandingInterest = Math.Max(0, loan.OutstandingInterest - totalAllocatedInterest);

        if (loan.OutstandingPrincipal <= 0 && loan.OutstandingInterest <= 0)
        {
            loan.Status = LoanStatus.Closed;
            loan.ClosedDate = DateTime.UtcNow;
        }

        // Create Collection Record
        var collection = new LoanCollection
        {
            ReceiptNumber = receiptNumber,
            LoanApplicationId = loan.Id,
            CustomerId = loan.CustomerId,
            BranchId = loan.BranchId,
            FieldOfficerId = _currentUser.EmployeeId,
            CollectionDate = req.CollectionDate != default ? req.CollectionDate : DateTime.UtcNow,
            TotalAmountPaid = req.Amount,
            PrincipalPortion = totalAllocatedPrincipal,
            InterestPortion = totalAllocatedInterest,
            PenaltyPortion = totalAllocatedPenalty,
            PaymentMode = req.PaymentMode,
            TransactionReference = req.ReferenceNumber,
            Remarks = req.Remarks,
            CollectionLatitude = req.Latitude,
            CollectionLongitude = req.Longitude,
            IsVerified = true,
            VerifiedBy = _currentUser.Username ?? "Field Officer"
        };
        _db.LoanCollections.Add(collection);

        // Update Branch Cash/Bank
        var branch = await _db.Branches.FindAsync(loan.BranchId);
        if (branch != null)
        {
            if (req.PaymentMode == PaymentMode.Cash)
                branch.CurrentCashBalance += req.Amount;
            else
                branch.CurrentBankBalance += req.Amount;
        }

        // Post Accounting Voucher
        await _accounting.PostCollectionVoucherAsync(collection);

        await _db.SaveChangesAsync();

        return Ok(ApiResponse<LoanCollection>.Ok(collection, $"Payment of {req.Amount:C} collected. Receipt: {receiptNumber}"));
    }

    [HttpGet("sheet")]
    public async Task<ActionResult<ApiResponse<List<CollectionSheetItemDto>>>> GetCollectionSheet(
        [FromQuery] int centerId,
        [FromQuery] DateTime? date)
    {
        var center = await _db.Centers.FindAsync(centerId);
        if (center == null) return NotFound(ApiResponse<List<CollectionSheetItemDto>>.Fail("Center not found."));

        if (!_currentUser.IsAdminOrSuperAdmin && _currentUser.BranchId.HasValue && center.BranchId != _currentUser.BranchId.Value)
        {
            return Forbid();
        }

        if (_currentUser.IsFieldOfficer && _currentUser.EmployeeId.HasValue && center.FieldOfficerId.HasValue && center.FieldOfficerId.Value != _currentUser.EmployeeId.Value)
        {
            return Forbid();
        }

        var targetDate = date ?? DateTime.UtcNow.Date;

        var loans = await _db.LoanApplications
            .Include(l => l.Customer)
            .Include(l => l.Group)
            .Include(l => l.EmiSchedules)
            .Where(l => l.CenterId == centerId && l.Status == LoanStatus.Active)
            .ToListAsync();

        var sheet = new List<CollectionSheetItemDto>();

        foreach (var l in loans)
        {
            // Find current due or first unpaid installment
            var pendingEmi = l.EmiSchedules
                .Where(e => e.Status != EmiStatus.Paid)
                .OrderBy(e => e.InstallmentNumber)
                .FirstOrDefault();

            if (pendingEmi == null) continue;

            var dueAmount = pendingEmi.PrincipalAmount + pendingEmi.InterestAmount - pendingEmi.PaidPrincipal - pendingEmi.PaidInterest;

            // Overdue amount from previous installments
            var overdueAmount = l.EmiSchedules
                .Where(e => e.Status != EmiStatus.Paid && e.InstallmentNumber < pendingEmi.InstallmentNumber)
                .Sum(e => e.PrincipalAmount + e.InterestAmount - e.PaidPrincipal - e.PaidInterest);

            sheet.Add(new CollectionSheetItemDto
            {
                LoanId = l.Id,
                LoanAccountNumber = l.LoanAccountNumber,
                CustomerId = l.CustomerId,
                CustomerName = l.Customer?.FullName ?? "Unknown",
                Phone = l.Customer?.Phone ?? "",
                GroupName = l.Group?.GroupName ?? "Individual",
                DueInstallmentNumber = pendingEmi.InstallmentNumber,
                DueAmount = dueAmount,
                OverdueAmount = overdueAmount,
                CollectedAmount = dueAmount + overdueAmount, // default to full due
                SavingsDeposit = 50m // default daily/weekly savings
            });
        }

        return Ok(ApiResponse<List<CollectionSheetItemDto>>.Ok(sheet));
    }

    [HttpPost("batch-submit")]
    public async Task<ActionResult<ApiResponse<int>>> SubmitBatchCollection([FromBody] BatchCollectionDto req)
    {
        int processedCount = 0;

        foreach (var item in req.Items.Where(i => i.CollectedAmount > 0))
        {
            await CollectPayment(new CreateLoanCollectionDto
            {
                LoanApplicationId = item.LoanId,
                Amount = item.CollectedAmount,
                PaymentMode = item.PaymentMode,
                CollectionDate = req.CollectionDate,
                Remarks = "Center Batch Collection"
            });
            processedCount++;
        }

        return Ok(ApiResponse<int>.Ok(processedCount, $"{processedCount} collections recorded successfully."));
    }
}
