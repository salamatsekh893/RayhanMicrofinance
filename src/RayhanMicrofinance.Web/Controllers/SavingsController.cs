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
public class SavingsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IAccountingService _accounting;
    private readonly ICurrentUserService _currentUser;

    public SavingsController(
        ApplicationDbContext db,
        IAccountingService accounting,
        ICurrentUserService currentUser)
    {
        _db = db;
        _accounting = accounting;
        _currentUser = currentUser;
    }

    [HttpGet("schemes")]
    public async Task<ActionResult<ApiResponse<List<SavingsSchemeDto>>>> GetSchemes()
    {
        var schemes = await _db.SavingsSchemes
            .Where(s => s.IsActive)
            .Select(s => new SavingsSchemeDto
            {
                Id = s.Id,
                SchemeCode = s.SchemeCode,
                SchemeName = s.SchemeName,
                SavingsType = s.SavingsType,
                InterestRatePerAnnum = s.InterestRatePerAnnum,
                MinDepositAmount = s.MinDepositAmount,
                MaturityInMonths = s.MaturityInMonths
            })
            .ToListAsync();

        return Ok(ApiResponse<List<SavingsSchemeDto>>.Ok(schemes));
    }

    [HttpGet("accounts")]
    public async Task<ActionResult<ApiResponse<List<SavingsAccountDto>>>> GetAccounts([FromQuery] int? customerId, [FromQuery] int? branchId)
    {
        if (!_currentUser.IsAdminOrSuperAdmin && _currentUser.BranchId.HasValue)
        {
            branchId = _currentUser.BranchId.Value;
        }

        var query = _db.SavingsAccounts
            .Include(s => s.Customer)
                .ThenInclude(c => c!.Center)
            .Include(s => s.Branch)
            .Include(s => s.SavingsScheme)
            .AsNoTracking()
            .AsQueryable();

        if (customerId.HasValue) query = query.Where(s => s.CustomerId == customerId.Value);
        if (branchId.HasValue) query = query.Where(s => s.BranchId == branchId.Value);

        if (_currentUser.IsFieldOfficer && _currentUser.EmployeeId.HasValue)
        {
            query = query.Where(s => s.Customer != null && s.Customer.Center != null && s.Customer.Center.FieldOfficerId == _currentUser.EmployeeId.Value);
        }

        var list = await query
            .Select(s => new SavingsAccountDto
            {
                Id = s.Id,
                AccountNumber = s.AccountNumber,
                CustomerId = s.CustomerId,
                CustomerName = s.Customer != null ? s.Customer.FullName : null,
                BranchId = s.BranchId,
                BranchName = s.Branch != null ? s.Branch.BranchName : null,
                SavingsSchemeId = s.SavingsSchemeId,
                SavingsSchemeName = s.SavingsScheme != null ? s.SavingsScheme.SchemeName : null,
                CurrentBalance = s.CurrentBalance,
                TotalDeposited = s.TotalDeposited,
                TotalWithdrawn = s.TotalWithdrawn,
                Status = s.Status,
                OpenedDate = s.OpenedDate
            })
            .ToListAsync();

        return Ok(ApiResponse<List<SavingsAccountDto>>.Ok(list));
    }

    [HttpPost("open-account")]
    public async Task<ActionResult<ApiResponse<SavingsAccountDto>>> OpenAccount([FromBody] CreateSavingsAccountDto req)
    {
        var customer = await _db.Customers.FindAsync(req.CustomerId);
        if (customer == null) return BadRequest(ApiResponse<SavingsAccountDto>.Fail("Customer not found"));

        var scheme = await _db.SavingsSchemes.FindAsync(req.SavingsSchemeId);
        if (scheme == null) return BadRequest(ApiResponse<SavingsAccountDto>.Fail("Scheme not found"));

        var branchId = customer.BranchId;
        var count = await _db.SavingsAccounts.CountAsync(s => s.BranchId == branchId);
        var accNo = $"SB-{branchId:D2}-{DateTime.UtcNow.Year % 100}{(count + 1):D5}";

        var account = new SavingsAccount
        {
            AccountNumber = accNo,
            CustomerId = customer.Id,
            BranchId = branchId,
            SavingsSchemeId = scheme.Id,
            CurrentBalance = req.InitialDeposit,
            TotalDeposited = req.InitialDeposit,
            TotalWithdrawn = 0,
            Status = SavingsAccountStatus.Active,
            OpenedDate = DateTime.UtcNow,
            MaturityDate = DateTime.UtcNow.AddMonths(scheme.MaturityInMonths)
        };

        _db.SavingsAccounts.Add(account);
        await _db.SaveChangesAsync();

        if (req.InitialDeposit > 0)
        {
            var initialTrx = new SavingsTransaction
            {
                TransactionNumber = $"TRX-SB-{account.Id}-{DateTime.UtcNow.Ticks % 1000000}",
                SavingsAccountId = account.Id,
                CustomerId = customer.Id,
                BranchId = branchId,
                Amount = req.InitialDeposit,
                BalanceAfterTransaction = req.InitialDeposit,
                Type = "Deposit",
                PaymentMode = req.PaymentMode,
                TransactionDate = DateTime.UtcNow,
                Remarks = "Opening Deposit"
            };
            _db.SavingsTransactions.Add(initialTrx);

            await _accounting.PostSavingsTransactionVoucherAsync(initialTrx);
            await _db.SaveChangesAsync();
        }

        return Ok(ApiResponse<SavingsAccountDto>.Ok(new SavingsAccountDto
        {
            Id = account.Id,
            AccountNumber = account.AccountNumber,
            CurrentBalance = account.CurrentBalance
        }, "Savings account opened successfully."));
    }

    [HttpPost("transaction")]
    public async Task<ActionResult<ApiResponse<SavingsTransaction>>> ProcessTransaction([FromBody] SavingsTransactionRequestDto req)
    {
        var account = await _db.SavingsAccounts.FindAsync(req.SavingsAccountId);
        if (account == null) return NotFound(ApiResponse<SavingsTransaction>.Fail("Savings account not found"));

        if (account.Status != SavingsAccountStatus.Active)
        {
            return BadRequest(ApiResponse<SavingsTransaction>.Fail("Account is not active."));
        }

        var isDeposit = req.Type.Equals("Deposit", StringComparison.OrdinalIgnoreCase);

        if (!isDeposit && account.CurrentBalance < req.Amount)
        {
            return BadRequest(ApiResponse<SavingsTransaction>.Fail("Insufficient savings account balance."));
        }

        if (isDeposit)
        {
            account.CurrentBalance += req.Amount;
            account.TotalDeposited += req.Amount;
        }
        else
        {
            account.CurrentBalance -= req.Amount;
            account.TotalWithdrawn += req.Amount;
        }

        var trx = new SavingsTransaction
        {
            TransactionNumber = $"TRX-SB-{account.Id}-{DateTime.UtcNow.Ticks % 1000000}",
            SavingsAccountId = account.Id,
            CustomerId = account.CustomerId,
            BranchId = account.BranchId,
            Amount = req.Amount,
            BalanceAfterTransaction = account.CurrentBalance,
            Type = isDeposit ? "Deposit" : "Withdrawal",
            PaymentMode = req.PaymentMode,
            ReferenceNumber = req.ReferenceNumber,
            Remarks = req.Remarks,
            TransactionDate = DateTime.UtcNow,
            CollectedBy = _currentUser.Username ?? "Cashier"
        };

        _db.SavingsTransactions.Add(trx);

        // Update branch cash
        var branch = await _db.Branches.FindAsync(account.BranchId);
        if (branch != null)
        {
            if (isDeposit)
                branch.CurrentCashBalance += req.Amount;
            else
                branch.CurrentCashBalance -= req.Amount;
        }

        // Post accounting voucher
        await _accounting.PostSavingsTransactionVoucherAsync(trx);

        await _db.SaveChangesAsync();

        return Ok(ApiResponse<SavingsTransaction>.Ok(trx, $"Savings {trx.Type} of {req.Amount:C} processed. Balance: {account.CurrentBalance:C}"));
    }
}
