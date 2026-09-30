using RayhanMicrofinance.Domain.Common;
using RayhanMicrofinance.Domain.Enums;

namespace RayhanMicrofinance.Domain.Entities;

public class LoanScheme : BaseEntity
{
    public string SchemeCode { get; set; } = string.Empty;
    public string SchemeName { get; set; } = string.Empty;
    public LoanType LoanType { get; set; } = LoanType.GroupLoan;

    public decimal MinAmount { get; set; } = 5000;
    public decimal MaxAmount { get; set; } = 100000;
    public decimal DefaultAmount { get; set; } = 25000;

    public int MinTenureMonths { get; set; } = 6;
    public int MaxTenureMonths { get; set; } = 24;
    public int DefaultTenureMonths { get; set; } = 12;

    public decimal InterestRatePerAnnum { get; set; } = 18.0m;
    public InterestCalculationMethod InterestCalculationMethod { get; set; } = InterestCalculationMethod.Flat;
    public RepaymentFrequency RepaymentFrequency { get; set; } = RepaymentFrequency.Weekly;

    public decimal ProcessingFeePercentage { get; set; } = 1.0m;
    public decimal InsuranceFeePercentage { get; set; } = 1.0m;
    public decimal LateFinePercentagePerDay { get; set; } = 0.1m;
    public int GracePeriodDays { get; set; } = 3;

    public string? Description { get; set; }
}

public class LoanApplication : BaseEntity
{
    public string LoanAccountNumber { get; set; } = string.Empty;

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    public int? CenterId { get; set; }
    public Center? Center { get; set; }

    public int? GroupId { get; set; }
    public LoanGroup? Group { get; set; }

    public int LoanSchemeId { get; set; }
    public LoanScheme? LoanScheme { get; set; }

    public LoanType LoanType { get; set; } = LoanType.GroupLoan;
    public decimal RequestedAmount { get; set; }
    public decimal ApprovedAmount { get; set; }
    public decimal DisbursedAmount { get; set; }

    public decimal InterestRatePerAnnum { get; set; }
    public InterestCalculationMethod CalculationMethod { get; set; } = InterestCalculationMethod.Flat;
    public RepaymentFrequency Frequency { get; set; } = RepaymentFrequency.Weekly;
    public int TenureInMonths { get; set; }
    public int TotalInstallments { get; set; }

    public decimal TotalInterest { get; set; }
    public decimal TotalPayable { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal OutstandingPrincipal { get; set; }
    public decimal OutstandingInterest { get; set; }
    public decimal TotalOutstanding => OutstandingPrincipal + OutstandingInterest;

    public decimal ProcessingFee { get; set; }
    public decimal InsuranceFee { get; set; }
    public decimal TotalPenaltyCollected { get; set; }

    public LoanStatus Status { get; set; } = LoanStatus.Applied;
    public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;

    public DateTime? VerifiedDate { get; set; }
    public string? VerifiedBy { get; set; }

    public DateTime? ApprovedDate { get; set; }
    public string? ApprovedBy { get; set; }
    public string? ApprovalRemarks { get; set; }

    public DateTime? DisbursedDate { get; set; }
    public string? DisbursedBy { get; set; }
    public PaymentMode DisbursementMode { get; set; } = PaymentMode.Cash;
    public string? DisbursementBankAccountId { get; set; }
    public string? DisbursementReference { get; set; }

    public DateTime? FirstEmiDate { get; set; }
    public DateTime? MaturityDate { get; set; }
    public DateTime? ClosedDate { get; set; }

    public string PurposeOfLoan { get; set; } = string.Empty;

    public ICollection<LoanEmiSchedule> EmiSchedules { get; set; } = new List<LoanEmiSchedule>();
    public ICollection<LoanCollection> Collections { get; set; } = new List<LoanCollection>();
}

public class LoanEmiSchedule : BaseEntity
{
    public int LoanApplicationId { get; set; }
    public LoanApplication? LoanApplication { get; set; }

    public int InstallmentNumber { get; set; }
    public DateTime DueDate { get; set; }

    public decimal PrincipalAmount { get; set; }
    public decimal InterestAmount { get; set; }
    public decimal TotalEmiAmount => PrincipalAmount + InterestAmount;

    public decimal PaidPrincipal { get; set; }
    public decimal PaidInterest { get; set; }
    public decimal PaidPenalty { get; set; }
    public decimal TotalPaidAmount => PaidPrincipal + PaidInterest + PaidPenalty;

    public decimal OutstandingPrincipal => PrincipalAmount - PaidPrincipal;
    public decimal OutstandingInterest => InterestAmount - PaidInterest;
    public decimal TotalOutstanding => OutstandingPrincipal + OutstandingInterest;

    public decimal PenaltyAmount { get; set; }
    public EmiStatus Status { get; set; } = EmiStatus.Pending;
    public DateTime? PaidDate { get; set; }
    public string? PaymentReference { get; set; }
}

public class LoanCollection : BaseEntity
{
    public string ReceiptNumber { get; set; } = string.Empty;

    public int LoanApplicationId { get; set; }
    public LoanApplication? LoanApplication { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    public int? FieldOfficerId { get; set; }
    public Employee? FieldOfficer { get; set; }

    public DateTime CollectionDate { get; set; } = DateTime.UtcNow;
    public decimal TotalAmountPaid { get; set; }

    public decimal PrincipalPortion { get; set; }
    public decimal InterestPortion { get; set; }
    public decimal PenaltyPortion { get; set; }
    public decimal AdvancePortion { get; set; }

    public PaymentMode PaymentMode { get; set; } = PaymentMode.Cash;
    public string? TransactionReference { get; set; }
    public string? Remarks { get; set; }

    public bool IsVerified { get; set; } = true;
    public string? VerifiedBy { get; set; }
    public double? CollectionLatitude { get; set; }
    public double? CollectionLongitude { get; set; }
}

public class LoanRescheduleHistory : BaseEntity
{
    public int LoanApplicationId { get; set; }
    public LoanApplication? LoanApplication { get; set; }

    public DateTime RescheduleDate { get; set; }
    public decimal PreviousOutstanding { get; set; }
    public int NewTenureInMonths { get; set; }
    public decimal NewInterestRate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string ApprovedBy { get; set; } = string.Empty;
}

public class LoanWriteOffHistory : BaseEntity
{
    public int LoanApplicationId { get; set; }
    public LoanApplication? LoanApplication { get; set; }

    public DateTime WriteOffDate { get; set; }
    public decimal WrittenOffPrincipal { get; set; }
    public decimal WrittenOffInterest { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string ApprovedBy { get; set; } = string.Empty;
}
