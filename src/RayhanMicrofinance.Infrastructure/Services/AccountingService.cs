using Microsoft.EntityFrameworkCore;
using RayhanMicrofinance.Application.Interfaces;
using RayhanMicrofinance.Domain.Entities;
using RayhanMicrofinance.Domain.Enums;
using RayhanMicrofinance.Infrastructure.Data;

namespace RayhanMicrofinance.Infrastructure.Services;

public class AccountingService : IAccountingService
{
    private readonly ApplicationDbContext _db;

    public AccountingService(ApplicationDbContext db)
    {
        _db = db;
    }

    private async Task<ChartOfAccount?> GetAccountByCodeAsync(string code, CancellationToken ct)
    {
        return await _db.ChartOfAccounts.FirstOrDefaultAsync(a => a.AccountCode == code, ct);
    }

    public async Task<Voucher?> PostDisbursementVoucherAsync(LoanApplication loan, CancellationToken ct = default)
    {
        var cashOrBankCode = loan.DisbursementMode == PaymentMode.Cash ? "1010" : "1020";
        var cashAccount = await GetAccountByCodeAsync(cashOrBankCode, ct);
        var loanPortfolioAccount = await GetAccountByCodeAsync("1030", ct);
        var feeIncomeAccount = await GetAccountByCodeAsync("4020", ct);

        if (cashAccount == null || loanPortfolioAccount == null) return null;

        var voucher = new Voucher
        {
            VoucherNumber = $"V-DISB-{loan.LoanAccountNumber}",
            VoucherType = VoucherType.Payment,
            VoucherDate = loan.DisbursedDate ?? DateTime.UtcNow,
            BranchId = loan.BranchId,
            ReferenceNumber = loan.LoanAccountNumber,
            Narration = $"Loan Disbursement for Loan #{loan.LoanAccountNumber} ({loan.LoanType})",
            TotalAmount = loan.DisbursedAmount,
            IsPosted = true,
            PreparedBy = "System"
        };

        // Debit Loan Portfolio (Asset increases)
        voucher.Details.Add(new VoucherDetail
        {
            ChartOfAccountId = loanPortfolioAccount.Id,
            DebitAmount = loan.DisbursedAmount,
            CreditAmount = 0,
            LineNarration = "Principal Disbursed"
        });

        // Credit Cash/Bank (Asset decreases)
        voucher.Details.Add(new VoucherDetail
        {
            ChartOfAccountId = cashAccount.Id,
            DebitAmount = 0,
            CreditAmount = loan.DisbursedAmount,
            LineNarration = $"Disbursed via {loan.DisbursementMode}"
        });

        _db.Vouchers.Add(voucher);

        // Update Account Balances
        loanPortfolioAccount.CurrentBalance += loan.DisbursedAmount;
        cashAccount.CurrentBalance -= loan.DisbursedAmount;

        return voucher;
    }

    public async Task<Voucher?> PostCollectionVoucherAsync(LoanCollection collection, CancellationToken ct = default)
    {
        var cashOrBankCode = collection.PaymentMode == PaymentMode.Cash ? "1010" : "1020";
        var cashAccount = await GetAccountByCodeAsync(cashOrBankCode, ct);
        var loanPortfolioAccount = await GetAccountByCodeAsync("1030", ct);
        var interestIncomeAccount = await GetAccountByCodeAsync("4010", ct);
        var penaltyIncomeAccount = await GetAccountByCodeAsync("4030", ct);

        if (cashAccount == null || loanPortfolioAccount == null) return null;

        var voucher = new Voucher
        {
            VoucherNumber = $"V-COL-{collection.ReceiptNumber}",
            VoucherType = VoucherType.Receipt,
            VoucherDate = collection.CollectionDate,
            BranchId = collection.BranchId,
            ReferenceNumber = collection.ReceiptNumber,
            Narration = $"Loan Collection against Receipt #{collection.ReceiptNumber}",
            TotalAmount = collection.TotalAmountPaid,
            IsPosted = true,
            PreparedBy = "System"
        };

        // Debit Cash/Bank (Asset increases)
        voucher.Details.Add(new VoucherDetail
        {
            ChartOfAccountId = cashAccount.Id,
            DebitAmount = collection.TotalAmountPaid,
            CreditAmount = 0,
            LineNarration = "Total Received"
        });
        cashAccount.CurrentBalance += collection.TotalAmountPaid;

        // Credit Principal (Asset decreases)
        if (collection.PrincipalPortion > 0)
        {
            voucher.Details.Add(new VoucherDetail
            {
                ChartOfAccountId = loanPortfolioAccount.Id,
                DebitAmount = 0,
                CreditAmount = collection.PrincipalPortion,
                LineNarration = "Principal Repaid"
            });
            loanPortfolioAccount.CurrentBalance -= collection.PrincipalPortion;
        }

        // Credit Interest Income (Revenue increases)
        if (collection.InterestPortion > 0 && interestIncomeAccount != null)
        {
            voucher.Details.Add(new VoucherDetail
            {
                ChartOfAccountId = interestIncomeAccount.Id,
                DebitAmount = 0,
                CreditAmount = collection.InterestPortion,
                LineNarration = "Interest Income Earned"
            });
            interestIncomeAccount.CurrentBalance += collection.InterestPortion;
        }

        // Credit Penalty Income (Revenue increases)
        if (collection.PenaltyPortion > 0 && penaltyIncomeAccount != null)
        {
            voucher.Details.Add(new VoucherDetail
            {
                ChartOfAccountId = penaltyIncomeAccount.Id,
                DebitAmount = 0,
                CreditAmount = collection.PenaltyPortion,
                LineNarration = "Penalty/Late Fine Collected"
            });
            penaltyIncomeAccount.CurrentBalance += collection.PenaltyPortion;
        }

        _db.Vouchers.Add(voucher);
        return voucher;
    }

    public async Task<Voucher?> PostSavingsTransactionVoucherAsync(SavingsTransaction transaction, CancellationToken ct = default)
    {
        var cashOrBankCode = transaction.PaymentMode == PaymentMode.Cash ? "1010" : "1020";
        var cashAccount = await GetAccountByCodeAsync(cashOrBankCode, ct);
        var savingsLiabilityAccount = await GetAccountByCodeAsync("2010", ct);

        if (cashAccount == null || savingsLiabilityAccount == null) return null;

        var isDeposit = transaction.Type.Equals("Deposit", StringComparison.OrdinalIgnoreCase);

        var voucher = new Voucher
        {
            VoucherNumber = $"V-SAV-{transaction.TransactionNumber}",
            VoucherType = isDeposit ? VoucherType.Receipt : VoucherType.Payment,
            VoucherDate = transaction.TransactionDate,
            BranchId = transaction.BranchId,
            ReferenceNumber = transaction.TransactionNumber,
            Narration = $"Savings {transaction.Type} #{transaction.TransactionNumber}",
            TotalAmount = transaction.Amount,
            IsPosted = true,
            PreparedBy = "System"
        };

        if (isDeposit)
        {
            // Debit Cash (Asset increases)
            voucher.Details.Add(new VoucherDetail
            {
                ChartOfAccountId = cashAccount.Id,
                DebitAmount = transaction.Amount,
                CreditAmount = 0,
                LineNarration = "Savings Deposited"
            });
            cashAccount.CurrentBalance += transaction.Amount;

            // Credit Member Savings (Liability increases)
            voucher.Details.Add(new VoucherDetail
            {
                ChartOfAccountId = savingsLiabilityAccount.Id,
                DebitAmount = 0,
                CreditAmount = transaction.Amount,
                LineNarration = "Member Savings Account Credited"
            });
            savingsLiabilityAccount.CurrentBalance += transaction.Amount;
        }
        else
        {
            // Debit Member Savings (Liability decreases)
            voucher.Details.Add(new VoucherDetail
            {
                ChartOfAccountId = savingsLiabilityAccount.Id,
                DebitAmount = transaction.Amount,
                CreditAmount = 0,
                LineNarration = "Member Savings Withdrawn"
            });
            savingsLiabilityAccount.CurrentBalance -= transaction.Amount;

            // Credit Cash (Asset decreases)
            voucher.Details.Add(new VoucherDetail
            {
                ChartOfAccountId = cashAccount.Id,
                DebitAmount = 0,
                CreditAmount = transaction.Amount,
                LineNarration = "Cash Paid Out"
            });
            cashAccount.CurrentBalance -= transaction.Amount;
        }

        _db.Vouchers.Add(voucher);
        return voucher;
    }

    public async Task<Voucher?> PostExpenseVoucherAsync(ExpenseEntry expense, CancellationToken ct = default)
    {
        var cashOrBankCode = expense.PaymentMode == PaymentMode.Cash ? "1010" : "1020";
        var cashAccount = await GetAccountByCodeAsync(cashOrBankCode, ct);
        var expenseCategory = await _db.ExpenseCategories.FindAsync(new object[] { expense.ExpenseCategoryId }, ct);
        var expenseAccount = expenseCategory?.ChartOfAccountId != null
            ? await _db.ChartOfAccounts.FindAsync(new object[] { expenseCategory.ChartOfAccountId.Value }, ct)
            : await GetAccountByCodeAsync("5010", ct);

        if (cashAccount == null || expenseAccount == null) return null;

        var voucher = new Voucher
        {
            VoucherNumber = $"V-EXP-{expense.ExpenseNumber}",
            VoucherType = VoucherType.Payment,
            VoucherDate = expense.ExpenseDate,
            BranchId = expense.BranchId,
            ReferenceNumber = expense.ExpenseNumber,
            Narration = $"Expense: {expense.PaidTo} - {expense.Description}",
            TotalAmount = expense.Amount,
            IsPosted = true,
            PreparedBy = "System"
        };

        // Debit Expense (Expense increases)
        voucher.Details.Add(new VoucherDetail
        {
            ChartOfAccountId = expenseAccount.Id,
            DebitAmount = expense.Amount,
            CreditAmount = 0,
            LineNarration = expense.Description
        });
        expenseAccount.CurrentBalance += expense.Amount;

        // Credit Cash/Bank (Asset decreases)
        voucher.Details.Add(new VoucherDetail
        {
            ChartOfAccountId = cashAccount.Id,
            DebitAmount = 0,
            CreditAmount = expense.Amount,
            LineNarration = $"Paid via {expense.PaymentMode}"
        });
        cashAccount.CurrentBalance -= expense.Amount;

        _db.Vouchers.Add(voucher);
        return voucher;
    }

    public async Task<Voucher?> PostIncomeVoucherAsync(IncomeEntry income, CancellationToken ct = default)
    {
        var cashOrBankCode = income.PaymentMode == PaymentMode.Cash ? "1010" : "1020";
        var cashAccount = await GetAccountByCodeAsync(cashOrBankCode, ct);
        var incomeCategory = await _db.IncomeCategories.FindAsync(new object[] { income.IncomeCategoryId }, ct);
        var incomeAccount = incomeCategory?.ChartOfAccountId != null
            ? await _db.ChartOfAccounts.FindAsync(new object[] { incomeCategory.ChartOfAccountId.Value }, ct)
            : await GetAccountByCodeAsync("4040", ct);

        if (cashAccount == null || incomeAccount == null) return null;

        var voucher = new Voucher
        {
            VoucherNumber = $"V-INC-{income.IncomeNumber}",
            VoucherType = VoucherType.Receipt,
            VoucherDate = income.IncomeDate,
            BranchId = income.BranchId,
            ReferenceNumber = income.IncomeNumber,
            Narration = $"Income: {income.ReceivedFrom} - {income.Description}",
            TotalAmount = income.Amount,
            IsPosted = true,
            PreparedBy = "System"
        };

        // Debit Cash/Bank (Asset increases)
        voucher.Details.Add(new VoucherDetail
        {
            ChartOfAccountId = cashAccount.Id,
            DebitAmount = income.Amount,
            CreditAmount = 0,
            LineNarration = $"Received via {income.PaymentMode}"
        });
        cashAccount.CurrentBalance += income.Amount;

        // Credit Income (Revenue increases)
        voucher.Details.Add(new VoucherDetail
        {
            ChartOfAccountId = incomeAccount.Id,
            DebitAmount = 0,
            CreditAmount = income.Amount,
            LineNarration = income.Description
        });
        incomeAccount.CurrentBalance += income.Amount;

        _db.Vouchers.Add(voucher);
        return voucher;
    }
}
