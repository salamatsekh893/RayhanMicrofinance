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
public class AccountingController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public AccountingController(ApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [HttpGet("chart-of-accounts")]
    public async Task<ActionResult<ApiResponse<List<ChartOfAccountDto>>>> GetChartOfAccounts()
    {
        var accounts = await _db.ChartOfAccounts
            .OrderBy(a => a.AccountCode)
            .Select(a => new ChartOfAccountDto
            {
                Id = a.Id,
                AccountCode = a.AccountCode,
                AccountName = a.AccountName,
                Classification = a.Classification,
                ParentAccountId = a.ParentAccountId,
                IsHeader = a.IsHeader,
                CurrentBalance = a.CurrentBalance
            })
            .ToListAsync();

        return Ok(ApiResponse<List<ChartOfAccountDto>>.Ok(accounts));
    }

    [HttpGet("vouchers")]
    public async Task<ActionResult<ApiResponse<List<Voucher>>>> GetVouchers([FromQuery] int? branchId, [FromQuery] VoucherType? type)
    {
        if (!_currentUser.IsSuperAdmin && _currentUser.BranchId.HasValue)
        {
            branchId = _currentUser.BranchId.Value;
        }

        var query = _db.Vouchers
            .Include(v => v.Details)
                .ThenInclude(d => d.ChartOfAccount)
            .Include(v => v.Branch)
            .AsNoTracking()
            .AsQueryable();

        if (branchId.HasValue) query = query.Where(v => v.BranchId == branchId.Value);
        if (type.HasValue) query = query.Where(v => v.VoucherType == type.Value);

        var list = await query
            .OrderByDescending(v => v.VoucherDate)
            .Take(50)
            .ToListAsync();

        return Ok(ApiResponse<List<Voucher>>.Ok(list));
    }

    [HttpPost("vouchers")]
    public async Task<ActionResult<ApiResponse<Voucher>>> CreateCustomVoucher([FromBody] CreateVoucherDto req)
    {
        var totalDebit = req.Lines.Sum(l => l.DebitAmount);
        var totalCredit = req.Lines.Sum(l => l.CreditAmount);

        if (totalDebit != totalCredit || totalDebit <= 0)
        {
            return BadRequest(ApiResponse<Voucher>.Fail("Debit and Credit totals must be equal and greater than 0."));
        }

        var branchId = req.BranchId;
        if (!_currentUser.IsSuperAdmin && _currentUser.BranchId.HasValue)
        {
            branchId = _currentUser.BranchId.Value;
        }

        var count = await _db.Vouchers.CountAsync(v => v.BranchId == branchId);
        var vNo = $"V-{req.VoucherType.ToString().ToUpper()}-{branchId:D2}-{(count + 1):D5}";

        var voucher = new Voucher
        {
            VoucherNumber = vNo,
            VoucherType = req.VoucherType,
            VoucherDate = req.VoucherDate,
            BranchId = branchId,
            ReferenceNumber = req.ReferenceNumber,
            Narration = req.Narration,
            TotalAmount = totalDebit,
            IsPosted = true,
            PreparedBy = _currentUser.Username ?? "Accountant"
        };

        foreach (var line in req.Lines)
        {
            var acc = await _db.ChartOfAccounts.FindAsync(line.ChartOfAccountId);
            if (acc != null)
            {
                // Asset / Expense increases with Debit; Liability / Equity / Revenue increases with Credit
                if (acc.Classification == AccountClassification.Asset || acc.Classification == AccountClassification.Expense)
                {
                    acc.CurrentBalance += (line.DebitAmount - line.CreditAmount);
                }
                else
                {
                    acc.CurrentBalance += (line.CreditAmount - line.DebitAmount);
                }
            }

            voucher.Details.Add(new VoucherDetail
            {
                ChartOfAccountId = line.ChartOfAccountId,
                DebitAmount = line.DebitAmount,
                CreditAmount = line.CreditAmount,
                LineNarration = line.LineNarration
            });
        }

        _db.Vouchers.Add(voucher);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<Voucher>.Ok(voucher, "Voucher posted successfully."));
    }

    [HttpGet("trial-balance")]
    public async Task<ActionResult<ApiResponse<object>>> GetTrialBalance([FromQuery] int? branchId)
    {
        var accounts = await _db.ChartOfAccounts
            .Include(a => a.VoucherDetails)
            .OrderBy(a => a.AccountCode)
            .ToListAsync();

        var rows = accounts.Select(a =>
        {
            var debits = a.VoucherDetails.Sum(d => d.DebitAmount);
            var credits = a.VoucherDetails.Sum(d => d.CreditAmount);
            var balance = a.CurrentBalance;

            decimal debitBal = 0;
            decimal creditBal = 0;

            if (a.Classification == AccountClassification.Asset || a.Classification == AccountClassification.Expense)
            {
                debitBal = balance >= 0 ? balance : 0;
                creditBal = balance < 0 ? -balance : 0;
            }
            else
            {
                creditBal = balance >= 0 ? balance : 0;
                debitBal = balance < 0 ? -balance : 0;
            }

            return new
            {
                a.AccountCode,
                a.AccountName,
                Classification = a.Classification.ToString(),
                TotalDebit = debits,
                TotalCredit = credits,
                DebitBalance = debitBal,
                CreditBalance = creditBal
            };
        }).ToList();

        var totalDebitBalance = rows.Sum(r => r.DebitBalance);
        var totalCreditBalance = rows.Sum(r => r.CreditBalance);

        return Ok(ApiResponse<object>.Ok(new
        {
            Rows = rows,
            TotalDebit = totalDebitBalance,
            TotalCredit = totalCreditBalance,
            IsBalanced = totalDebitBalance == totalCreditBalance
        }));
    }

    [HttpGet("profit-loss")]
    public async Task<ActionResult<ApiResponse<object>>> GetProfitAndLoss([FromQuery] int? branchId)
    {
        var revenues = await _db.ChartOfAccounts
            .Where(a => a.Classification == AccountClassification.Revenue)
            .Select(a => new { a.AccountCode, a.AccountName, Amount = a.CurrentBalance })
            .ToListAsync();

        var expenses = await _db.ChartOfAccounts
            .Where(a => a.Classification == AccountClassification.Expense)
            .Select(a => new { a.AccountCode, a.AccountName, Amount = a.CurrentBalance })
            .ToListAsync();

        var totalRevenue = revenues.Sum(r => r.Amount);
        var totalExpense = expenses.Sum(e => e.Amount);
        var netProfit = totalRevenue - totalExpense;

        return Ok(ApiResponse<object>.Ok(new
        {
            Revenues = revenues,
            TotalRevenue = totalRevenue,
            Expenses = expenses,
            TotalExpense = totalExpense,
            NetProfit = netProfit
        }));
    }

    [HttpGet("balance-sheet")]
    public async Task<ActionResult<ApiResponse<object>>> GetBalanceSheet([FromQuery] int? branchId)
    {
        var assets = await _db.ChartOfAccounts
            .Where(a => a.Classification == AccountClassification.Asset)
            .Select(a => new { a.AccountCode, a.AccountName, Amount = a.CurrentBalance })
            .ToListAsync();

        var liabilities = await _db.ChartOfAccounts
            .Where(a => a.Classification == AccountClassification.Liability)
            .Select(a => new { a.AccountCode, a.AccountName, Amount = a.CurrentBalance })
            .ToListAsync();

        var equities = await _db.ChartOfAccounts
            .Where(a => a.Classification == AccountClassification.Equity)
            .Select(a => new { a.AccountCode, a.AccountName, Amount = a.CurrentBalance })
            .ToListAsync();

        var totalAssets = assets.Sum(a => a.Amount);
        var totalLiabilities = liabilities.Sum(l => l.Amount);
        var totalEquities = equities.Sum(e => e.Amount);

        return Ok(ApiResponse<object>.Ok(new
        {
            Assets = assets,
            TotalAssets = totalAssets,
            Liabilities = liabilities,
            TotalLiabilities = totalLiabilities,
            Equities = equities,
            TotalEquities = totalEquities,
            TotalLiabilitiesAndEquity = totalLiabilities + totalEquities
        }));
    }

    [HttpGet("day-book")]
    public async Task<ActionResult<ApiResponse<object>>> GetDayBook([FromQuery] DateTime? date, [FromQuery] int? branchId)
    {
        var targetDate = date ?? DateTime.UtcNow.Date;
        var nextDay = targetDate.AddDays(1);

        var vouchers = await _db.Vouchers
            .Include(v => v.Details)
                .ThenInclude(d => d.ChartOfAccount)
            .Where(v => v.VoucherDate >= targetDate && v.VoucherDate < nextDay)
            .OrderBy(v => v.VoucherDate)
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(vouchers));
    }
}
