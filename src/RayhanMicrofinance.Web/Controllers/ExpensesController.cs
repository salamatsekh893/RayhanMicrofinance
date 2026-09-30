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
public class ExpensesController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IAccountingService _accounting;
    private readonly ICurrentUserService _currentUser;

    public ExpensesController(ApplicationDbContext db, IAccountingService accounting, ICurrentUserService currentUser)
    {
        _db = db;
        _accounting = accounting;
        _currentUser = currentUser;
    }

    [HttpGet("categories")]
    public async Task<ActionResult<ApiResponse<List<ExpenseCategory>>>> GetCategories()
    {
        var list = await _db.ExpenseCategories.ToListAsync();
        if (list.Count == 0)
        {
            list = new List<ExpenseCategory>
            {
                new() { Name = "Office Rent", Description = "Branch office rental" },
                new() { Name = "Electricity & Utilities", Description = "Electricity, Water, Internet" },
                new() { Name = "Field Conveyance & Fuel", Description = "Field Officer travel & bike fuel" },
                new() { Name = "Printing & Stationery", Description = "Passbooks, loan forms, receipts" },
                new() { Name = "Staff Welfare & Tea", Description = "Refreshments & staff welfare" }
            };
            _db.ExpenseCategories.AddRange(list);
            await _db.SaveChangesAsync();
        }
        return Ok(ApiResponse<List<ExpenseCategory>>.Ok(list));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ExpenseDto>>>> GetExpenses([FromQuery] int? branchId)
    {
        if (!_currentUser.IsSuperAdmin && _currentUser.BranchId.HasValue)
        {
            branchId = _currentUser.BranchId.Value;
        }

        var query = _db.ExpenseEntries
            .Include(e => e.ExpenseCategory)
            .Include(e => e.Branch)
            .AsNoTracking()
            .AsQueryable();

        if (branchId.HasValue) query = query.Where(e => e.BranchId == branchId.Value);

        var list = await query
            .OrderByDescending(e => e.ExpenseDate)
            .Take(50)
            .Select(e => new ExpenseDto
            {
                Id = e.Id,
                ExpenseNumber = e.ExpenseNumber,
                ExpenseCategoryId = e.ExpenseCategoryId,
                ExpenseCategoryName = e.ExpenseCategory != null ? e.ExpenseCategory.Name : null,
                BranchId = e.BranchId,
                BranchName = e.Branch != null ? e.Branch.BranchName : null,
                Amount = e.Amount,
                PaymentMode = e.PaymentMode,
                PaidTo = e.PaidTo,
                ExpenseDate = e.ExpenseDate,
                Description = e.Description,
                IsApproved = e.IsApproved
            })
            .ToListAsync();

        return Ok(ApiResponse<List<ExpenseDto>>.Ok(list));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ExpenseDto>>> RecordExpense([FromBody] CreateExpenseDto req)
    {
        var branchId = req.BranchId;
        if (!_currentUser.IsSuperAdmin && _currentUser.BranchId.HasValue)
        {
            branchId = _currentUser.BranchId.Value;
        }

        var count = await _db.ExpenseEntries.CountAsync(e => e.BranchId == branchId);
        var expNo = $"EXP-{branchId:D2}-{(count + 1):D5}";

        var expense = new ExpenseEntry
        {
            ExpenseNumber = expNo,
            ExpenseCategoryId = req.ExpenseCategoryId,
            BranchId = branchId,
            Amount = req.Amount,
            PaymentMode = req.PaymentMode,
            PaidTo = req.PaidTo,
            ExpenseDate = req.ExpenseDate != default ? req.ExpenseDate : DateTime.UtcNow,
            ReferenceNumber = req.ReferenceNumber,
            Description = req.Description,
            IsApproved = true,
            ApprovedBy = _currentUser.Username ?? "Manager"
        };

        _db.ExpenseEntries.Add(expense);

        // Update branch cash
        var branch = await _db.Branches.FindAsync(branchId);
        if (branch != null)
        {
            if (req.PaymentMode == PaymentMode.Cash)
                branch.CurrentCashBalance -= req.Amount;
            else
                branch.CurrentBankBalance -= req.Amount;
        }

        // Post accounting voucher
        await _accounting.PostExpenseVoucherAsync(expense);

        await _db.SaveChangesAsync();

        return Ok(ApiResponse<ExpenseDto>.Ok(new ExpenseDto
        {
            Id = expense.Id,
            ExpenseNumber = expense.ExpenseNumber,
            Amount = expense.Amount
        }, "Expense recorded successfully."));
    }
}
