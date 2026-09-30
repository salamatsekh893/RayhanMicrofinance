using RayhanMicrofinance.Domain.Enums;

namespace RayhanMicrofinance.Application.DTOs;

public class SavingsSchemeDto
{
    public int Id { get; set; }
    public string SchemeCode { get; set; } = string.Empty;
    public string SchemeName { get; set; } = string.Empty;
    public SavingsType SavingsType { get; set; }
    public string SavingsTypeName => SavingsType.ToString();
    public decimal InterestRatePerAnnum { get; set; }
    public decimal MinDepositAmount { get; set; }
    public int MaturityInMonths { get; set; }
}

public class SavingsAccountDto
{
    public int Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int BranchId { get; set; }
    public string? BranchName { get; set; }
    public int SavingsSchemeId { get; set; }
    public string? SavingsSchemeName { get; set; }
    public decimal CurrentBalance { get; set; }
    public decimal TotalDeposited { get; set; }
    public decimal TotalWithdrawn { get; set; }
    public SavingsAccountStatus Status { get; set; }
    public DateTime OpenedDate { get; set; }
}

public class CreateSavingsAccountDto
{
    public int CustomerId { get; set; }
    public int BranchId { get; set; }
    public int SavingsSchemeId { get; set; }
    public decimal InitialDeposit { get; set; }
    public PaymentMode PaymentMode { get; set; } = PaymentMode.Cash;
}

public class SavingsTransactionRequestDto
{
    public int SavingsAccountId { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; } = "Deposit"; // Deposit or Withdrawal
    public PaymentMode PaymentMode { get; set; } = PaymentMode.Cash;
    public string? ReferenceNumber { get; set; }
    public string? Remarks { get; set; }
}

public class ChartOfAccountDto
{
    public int Id { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public AccountClassification Classification { get; set; }
    public string ClassificationName => Classification.ToString();
    public int? ParentAccountId { get; set; }
    public bool IsHeader { get; set; }
    public decimal CurrentBalance { get; set; }
}

public class CreateVoucherDto
{
    public VoucherType VoucherType { get; set; }
    public int BranchId { get; set; }
    public DateTime VoucherDate { get; set; } = DateTime.UtcNow;
    public string? ReferenceNumber { get; set; }
    public string Narration { get; set; } = string.Empty;
    public List<VoucherLineDto> Lines { get; set; } = new List<VoucherLineDto>();
}

public class VoucherLineDto
{
    public int ChartOfAccountId { get; set; }
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public string? LineNarration { get; set; }
}

public class ExpenseDto
{
    public int Id { get; set; }
    public string ExpenseNumber { get; set; } = string.Empty;
    public int ExpenseCategoryId { get; set; }
    public string? ExpenseCategoryName { get; set; }
    public int BranchId { get; set; }
    public string? BranchName { get; set; }
    public decimal Amount { get; set; }
    public PaymentMode PaymentMode { get; set; }
    public string PaidTo { get; set; } = string.Empty;
    public DateTime ExpenseDate { get; set; }
    public string? Description { get; set; }
    public bool IsApproved { get; set; }
}

public class CreateExpenseDto
{
    public int ExpenseCategoryId { get; set; }
    public int BranchId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMode PaymentMode { get; set; } = PaymentMode.Cash;
    public string PaidTo { get; set; } = string.Empty;
    public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
    public string? ReferenceNumber { get; set; }
    public string? Description { get; set; }
}

public class DashboardSummaryDto
{
    public decimal TodayCollection { get; set; }
    public decimal TodayDisbursement { get; set; }
    public int ActiveLoansCount { get; set; }
    public decimal TotalActiveLoanPortfolio { get; set; }
    public decimal DueCollectionToday { get; set; }
    public decimal OverdueAmount { get; set; }
    public int OverdueLoansCount { get; set; }
    public int TotalCustomersCount { get; set; }
    public decimal TotalSavingsBalance { get; set; }
    public decimal TotalCashBalance { get; set; }
    public decimal TotalBankBalance { get; set; }
    public decimal MonthIncome { get; set; }
    public decimal MonthExpense { get; set; }
    public decimal NetProfit => MonthIncome - MonthExpense;

    public List<MonthlyGraphItemDto> MonthlyDisbursementAndCollection { get; set; } = new List<MonthlyGraphItemDto>();
    public List<BranchPerformanceDto> BranchPerformances { get; set; } = new List<BranchPerformanceDto>();
}

public class MonthlyGraphItemDto
{
    public string MonthName { get; set; } = string.Empty;
    public decimal Disbursement { get; set; }
    public decimal Collection { get; set; }
}

public class BranchPerformanceDto
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int ActiveLoans { get; set; }
    public decimal OutstandingAmount { get; set; }
    public decimal CollectionEfficiencyPercentage { get; set; }
}
