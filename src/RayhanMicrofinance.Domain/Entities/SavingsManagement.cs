using RayhanMicrofinance.Domain.Common;
using RayhanMicrofinance.Domain.Enums;

namespace RayhanMicrofinance.Domain.Entities;

public class SavingsScheme : BaseEntity
{
    public string SchemeCode { get; set; } = string.Empty;
    public string SchemeName { get; set; } = string.Empty;
    public SavingsType SavingsType { get; set; } = SavingsType.DailySavings;
    public decimal InterestRatePerAnnum { get; set; } = 4.0m;
    public decimal MinDepositAmount { get; set; } = 50;
    public int MaturityInMonths { get; set; } = 12;
    public int LockinPeriodMonths { get; set; } = 1;
    public string? Description { get; set; }
}

public class SavingsAccount : BaseEntity
{
    public string AccountNumber { get; set; } = string.Empty;

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    public int SavingsSchemeId { get; set; }
    public SavingsScheme? SavingsScheme { get; set; }

    public decimal CurrentBalance { get; set; } = 0;
    public decimal TotalDeposited { get; set; } = 0;
    public decimal TotalWithdrawn { get; set; } = 0;
    public decimal TotalInterestEarned { get; set; } = 0;

    public DateTime OpenedDate { get; set; } = DateTime.UtcNow;
    public DateTime? MaturityDate { get; set; }
    public DateTime? ClosedDate { get; set; }

    public SavingsAccountStatus Status { get; set; } = SavingsAccountStatus.Active;

    public ICollection<SavingsTransaction> Transactions { get; set; } = new List<SavingsTransaction>();
}

public class SavingsTransaction : BaseEntity
{
    public string TransactionNumber { get; set; } = string.Empty;

    public int SavingsAccountId { get; set; }
    public SavingsAccount? SavingsAccount { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public string Type { get; set; } = "Deposit"; // Deposit, Withdrawal, InterestCredit
    public decimal Amount { get; set; }
    public decimal BalanceAfterTransaction { get; set; }

    public PaymentMode PaymentMode { get; set; } = PaymentMode.Cash;
    public string? ReferenceNumber { get; set; }
    public string? CollectedBy { get; set; }
    public string? Remarks { get; set; }
}
