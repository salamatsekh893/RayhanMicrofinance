using RayhanMicrofinance.Domain.Common;
using RayhanMicrofinance.Domain.Enums;

namespace RayhanMicrofinance.Domain.Entities;

public class ChartOfAccount : BaseEntity
{
    public string AccountCode { get; set; } = string.Empty; // e.g. "1010", "2010"
    public string AccountName { get; set; } = string.Empty;
    public AccountClassification Classification { get; set; } = AccountClassification.Asset;

    public int? ParentAccountId { get; set; }
    public ChartOfAccount? ParentAccount { get; set; }

    public bool IsHeader { get; set; } = false;
    public bool IsSystemAccount { get; set; } = false; // Cannot be deleted
    public decimal CurrentBalance { get; set; } = 0;

    public int? BranchId { get; set; } // Nullable if company-wide
    public Branch? Branch { get; set; }

    public ICollection<ChartOfAccount> SubAccounts { get; set; } = new List<ChartOfAccount>();
    public ICollection<VoucherDetail> VoucherDetails { get; set; } = new List<VoucherDetail>();
}

public class Voucher : BaseEntity
{
    public string VoucherNumber { get; set; } = string.Empty;
    public VoucherType VoucherType { get; set; } = VoucherType.Journal;
    public DateTime VoucherDate { get; set; } = DateTime.UtcNow;

    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    public string? ReferenceNumber { get; set; }
    public string Narration { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }

    public bool IsPosted { get; set; } = true;
    public string? PreparedBy { get; set; }
    public string? ApprovedBy { get; set; }

    public ICollection<VoucherDetail> Details { get; set; } = new List<VoucherDetail>();
}

public class VoucherDetail : BaseEntity
{
    public int VoucherId { get; set; }
    public Voucher? Voucher { get; set; }

    public int ChartOfAccountId { get; set; }
    public ChartOfAccount? ChartOfAccount { get; set; }

    public decimal DebitAmount { get; set; } = 0;
    public decimal CreditAmount { get; set; } = 0;
    public string? LineNarration { get; set; }
}

public class ExpenseCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ChartOfAccountId { get; set; }
    public ChartOfAccount? ChartOfAccount { get; set; }
}

public class ExpenseEntry : BaseEntity
{
    public string ExpenseNumber { get; set; } = string.Empty;
    public int ExpenseCategoryId { get; set; }
    public ExpenseCategory? ExpenseCategory { get; set; }

    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    public decimal Amount { get; set; }
    public PaymentMode PaymentMode { get; set; } = PaymentMode.Cash;
    public string PaidTo { get; set; } = string.Empty;
    public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
    public string? ReferenceNumber { get; set; }
    public string? Description { get; set; }

    public bool IsApproved { get; set; } = true;
    public string? ApprovedBy { get; set; }

    public int? VoucherId { get; set; }
    public Voucher? Voucher { get; set; }
}

public class IncomeCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ChartOfAccountId { get; set; }
    public ChartOfAccount? ChartOfAccount { get; set; }
}

public class IncomeEntry : BaseEntity
{
    public string IncomeNumber { get; set; } = string.Empty;
    public int IncomeCategoryId { get; set; }
    public IncomeCategory? IncomeCategory { get; set; }

    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    public decimal Amount { get; set; }
    public PaymentMode PaymentMode { get; set; } = PaymentMode.Cash;
    public string ReceivedFrom { get; set; } = string.Empty;
    public DateTime IncomeDate { get; set; } = DateTime.UtcNow;
    public string? ReferenceNumber { get; set; }
    public string? Description { get; set; }

    public int? VoucherId { get; set; }
    public Voucher? Voucher { get; set; }
}
