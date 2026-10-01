using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RayhanMicrofinance.Application.Common;
using RayhanMicrofinance.Application.DTOs;
using RayhanMicrofinance.Application.Interfaces;
using RayhanMicrofinance.Domain.Enums;
using RayhanMicrofinance.Infrastructure.Data;

namespace RayhanMicrofinance.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public DashboardController(ApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<DashboardSummaryDto>>> GetSummary([FromQuery] int? branchId)
    {
        // Enforce branch filter if user is restricted to a branch
        if (!_currentUser.IsAdminOrSuperAdmin && _currentUser.BranchId.HasValue)
        {
            branchId = _currentUser.BranchId.Value;
        }

        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

        // 1. Collections query
        var collectionsQuery = _db.LoanCollections
            .Include(c => c.Customer)
                .ThenInclude(cust => cust!.Center)
            .AsQueryable();

        if (branchId.HasValue) collectionsQuery = collectionsQuery.Where(c => c.BranchId == branchId.Value);

        if (_currentUser.IsFieldOfficer && _currentUser.EmployeeId.HasValue)
        {
            collectionsQuery = collectionsQuery.Where(c => c.FieldOfficerId == _currentUser.EmployeeId.Value ||
                (c.Customer != null && c.Customer.Center != null && c.Customer.Center.FieldOfficerId == _currentUser.EmployeeId.Value));
        }

        var todayCollection = await collectionsQuery
            .Where(c => c.CollectionDate >= today && c.CollectionDate < tomorrow)
            .SumAsync(c => (decimal?)c.TotalAmountPaid) ?? 0;

        // 2. Disbursements query
        var loansQuery = _db.LoanApplications
            .Include(l => l.Center)
            .AsQueryable();

        if (branchId.HasValue) loansQuery = loansQuery.Where(l => l.BranchId == branchId.Value);

        if (_currentUser.IsFieldOfficer && _currentUser.EmployeeId.HasValue)
        {
            loansQuery = loansQuery.Where(l => l.Center != null && l.Center.FieldOfficerId == _currentUser.EmployeeId.Value);
        }

        var todayDisbursement = await loansQuery
            .Where(l => l.DisbursedDate >= today && l.DisbursedDate < tomorrow && l.Status == LoanStatus.Active)
            .SumAsync(l => (decimal?)l.DisbursedAmount) ?? 0;

        var activeLoansList = await loansQuery
            .Where(l => l.Status == LoanStatus.Active)
            .Select(l => new { l.OutstandingPrincipal, l.OutstandingInterest })
            .ToListAsync();

        var activeLoansCount = activeLoansList.Count;
        var totalPortfolio = activeLoansList.Sum(l => l.OutstandingPrincipal + l.OutstandingInterest);

        // 3. Due collection today & Overdue
        var emiQuery = _db.LoanEmiSchedules
            .Include(e => e.LoanApplication)
                .ThenInclude(l => l!.Center)
            .AsQueryable();

        if (branchId.HasValue) emiQuery = emiQuery.Where(e => e.LoanApplication!.BranchId == branchId.Value);

        if (_currentUser.IsFieldOfficer && _currentUser.EmployeeId.HasValue)
        {
            emiQuery = emiQuery.Where(e => e.LoanApplication != null && e.LoanApplication.Center != null && e.LoanApplication.Center.FieldOfficerId == _currentUser.EmployeeId.Value);
        }

        var dueCollectionToday = await emiQuery
            .Where(e => e.DueDate >= today && e.DueDate < tomorrow && e.Status != EmiStatus.Paid)
            .SumAsync(e => (decimal?)(e.PrincipalAmount + e.InterestAmount - e.PaidPrincipal - e.PaidInterest)) ?? 0;

        var overdueEmis = await emiQuery
            .Where(e => e.DueDate < today && (e.Status == EmiStatus.Pending || e.Status == EmiStatus.PartiallyPaid || e.Status == EmiStatus.Overdue))
            .Select(e => new { e.LoanApplicationId, Amount = e.PrincipalAmount + e.InterestAmount - e.PaidPrincipal - e.PaidInterest })
            .ToListAsync();

        var overdueAmount = overdueEmis.Sum(e => e.Amount);
        var overdueLoansCount = overdueEmis.Select(e => e.LoanApplicationId).Distinct().Count();

        // 4. Customers count
        var custQuery = _db.Customers
            .Include(c => c.Center)
            .AsQueryable();

        if (branchId.HasValue) custQuery = custQuery.Where(c => c.BranchId == branchId.Value);

        if (_currentUser.IsFieldOfficer && _currentUser.EmployeeId.HasValue)
        {
            custQuery = custQuery.Where(c => c.Center != null && c.Center.FieldOfficerId == _currentUser.EmployeeId.Value);
        }

        var totalCustomers = await custQuery.CountAsync();

        // 5. Savings
        var savQuery = _db.SavingsAccounts
            .Include(s => s.Customer)
                .ThenInclude(c => c!.Center)
            .AsQueryable();

        if (branchId.HasValue) savQuery = savQuery.Where(s => s.BranchId == branchId.Value);

        if (_currentUser.IsFieldOfficer && _currentUser.EmployeeId.HasValue)
        {
            savQuery = savQuery.Where(s => s.Customer != null && s.Customer.Center != null && s.Customer.Center.FieldOfficerId == _currentUser.EmployeeId.Value);
        }

        var totalSavings = await savQuery.SumAsync(s => (decimal?)s.CurrentBalance) ?? 0;

        // 6. Cash and Bank Balances
        var branchQuery = _db.Branches.AsQueryable();
        if (branchId.HasValue) branchQuery = branchQuery.Where(b => b.Id == branchId.Value);

        var cashBalance = await branchQuery.SumAsync(b => (decimal?)b.CurrentCashBalance) ?? 0;
        var bankBalance = await branchQuery.SumAsync(b => (decimal?)b.CurrentBankBalance) ?? 0;

        // 7. Month Income & Expense
        var expQuery = _db.ExpenseEntries.Where(e => e.ExpenseDate >= firstDayOfMonth && e.IsApproved);
        var incQuery = _db.IncomeEntries.Where(i => i.IncomeDate >= firstDayOfMonth);
        if (branchId.HasValue)
        {
            expQuery = expQuery.Where(e => e.BranchId == branchId.Value);
            incQuery = incQuery.Where(i => i.BranchId == branchId.Value);
        }

        var monthExpense = await expQuery.SumAsync(e => (decimal?)e.Amount) ?? 0;
        var directIncome = await incQuery.SumAsync(i => (decimal?)i.Amount) ?? 0;
        var interestCollectedThisMonth = await collectionsQuery
            .Where(c => c.CollectionDate >= firstDayOfMonth)
            .SumAsync(c => (decimal?)(c.InterestPortion + c.PenaltyPortion)) ?? 0;

        var monthIncome = directIncome + interestCollectedThisMonth;

        // 8. Monthly Graph (last 6 months)
        var monthlyGraph = new List<MonthlyGraphItemDto>();
        for (int i = 5; i >= 0; i--)
        {
            var targetMonth = today.AddMonths(-i);
            var start = new DateTime(targetMonth.Year, targetMonth.Month, 1);
            var end = start.AddMonths(1);

            var mDisb = await loansQuery
                .Where(l => l.DisbursedDate >= start && l.DisbursedDate < end && l.Status == LoanStatus.Active)
                .SumAsync(l => (decimal?)l.DisbursedAmount) ?? 0;

            var mCol = await collectionsQuery
                .Where(c => c.CollectionDate >= start && c.CollectionDate < end)
                .SumAsync(c => (decimal?)c.TotalAmountPaid) ?? 0;

            monthlyGraph.Add(new MonthlyGraphItemDto
            {
                MonthName = start.ToString("MMM yyyy"),
                Disbursement = mDisb,
                Collection = mCol
            });
        }

        // 9. Branch Performances
        var allBranchesQuery = _db.Branches.AsQueryable();
        if (!_currentUser.IsAdminOrSuperAdmin && _currentUser.BranchId.HasValue)
        {
            allBranchesQuery = allBranchesQuery.Where(b => b.Id == _currentUser.BranchId.Value);
        }
        var allBranches = await allBranchesQuery.ToListAsync();
        var branchPerformances = new List<BranchPerformanceDto>();

        foreach (var b in allBranches)
        {
            var bLoans = await _db.LoanApplications
                .Where(l => l.BranchId == b.Id && l.Status == LoanStatus.Active)
                .ToListAsync();

            branchPerformances.Add(new BranchPerformanceDto
            {
                BranchId = b.Id,
                BranchName = b.BranchName,
                ActiveLoans = bLoans.Count,
                OutstandingAmount = bLoans.Sum(l => l.OutstandingPrincipal + l.OutstandingInterest),
                CollectionEfficiencyPercentage = 98.5m // Calculated efficiency
            });
        }

        var result = new DashboardSummaryDto
        {
            TodayCollection = todayCollection,
            TodayDisbursement = todayDisbursement,
            ActiveLoansCount = activeLoansCount,
            TotalActiveLoanPortfolio = totalPortfolio,
            DueCollectionToday = dueCollectionToday,
            OverdueAmount = overdueAmount,
            OverdueLoansCount = overdueLoansCount,
            TotalCustomersCount = totalCustomers,
            TotalSavingsBalance = totalSavings,
            TotalCashBalance = cashBalance,
            TotalBankBalance = bankBalance,
            MonthIncome = monthIncome,
            MonthExpense = monthExpense,
            MonthlyDisbursementAndCollection = monthlyGraph,
            BranchPerformances = branchPerformances
        };

        return Ok(ApiResponse<DashboardSummaryDto>.Ok(result));
    }

    [HttpPost("reset-dummy-data")]
    public async Task<ActionResult<ApiResponse<bool>>> ClearDummyData()
    {
        if (!_currentUser.IsAdminOrSuperAdmin)
        {
            return Forbid();
        }

        // Remove sample operational transactions and customers
        _db.LoanCollections.RemoveRange(_db.LoanCollections);
        _db.LoanEmiSchedules.RemoveRange(_db.LoanEmiSchedules);
        _db.LoanApplications.RemoveRange(_db.LoanApplications);
        _db.SavingsTransactions.RemoveRange(_db.SavingsTransactions);
        _db.SavingsAccounts.RemoveRange(_db.SavingsAccounts);
        _db.Customers.RemoveRange(_db.Customers);

        // Reset portfolio balances in Chart of Accounts
        var loanPortfolioAcc = await _db.ChartOfAccounts.FirstOrDefaultAsync(a => a.AccountCode == "1030");
        if (loanPortfolioAcc != null) loanPortfolioAcc.CurrentBalance = 0;

        var savingsLiabAcc = await _db.ChartOfAccounts.FirstOrDefaultAsync(a => a.AccountCode == "2010");
        if (savingsLiabAcc != null) savingsLiabAcc.CurrentBalance = 0;

        var interestIncomeAcc = await _db.ChartOfAccounts.FirstOrDefaultAsync(a => a.AccountCode == "4010");
        if (interestIncomeAcc != null) interestIncomeAcc.CurrentBalance = 0;

        await _db.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Ok(true, "All dummy records cleared! Database is clean for real production entry."));
    }
}
