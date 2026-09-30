using RayhanMicrofinance.Domain.Enums;

namespace RayhanMicrofinance.Application.DTOs;

public class LoanSchemeDto
{
    public int Id { get; set; }
    public string SchemeCode { get; set; } = string.Empty;
    public string SchemeName { get; set; } = string.Empty;
    public LoanType LoanType { get; set; }
    public string LoanTypeName => LoanType.ToString();
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public decimal DefaultAmount { get; set; }
    public int MinTenureMonths { get; set; }
    public int MaxTenureMonths { get; set; }
    public int DefaultTenureMonths { get; set; }
    public decimal InterestRatePerAnnum { get; set; }
    public InterestCalculationMethod InterestCalculationMethod { get; set; }
    public RepaymentFrequency RepaymentFrequency { get; set; }
    public decimal ProcessingFeePercentage { get; set; }
    public decimal InsuranceFeePercentage { get; set; }
    public decimal LateFinePercentagePerDay { get; set; }
    public int GracePeriodDays { get; set; }
}

public class CreateLoanSchemeDto
{
    public string SchemeName { get; set; } = string.Empty;
    public LoanType LoanType { get; set; } = LoanType.GroupLoan;
    public decimal MinAmount { get; set; } = 5000;
    public decimal MaxAmount { get; set; } = 100000;
    public decimal DefaultAmount { get; set; } = 30000;
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

public class LoanApplicationDto
{
    public int Id { get; set; }
    public string LoanAccountNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public int BranchId { get; set; }
    public string? BranchName { get; set; }
    public int? CenterId { get; set; }
    public string? CenterName { get; set; }
    public int? GroupId { get; set; }
    public string? GroupName { get; set; }
    public int LoanSchemeId { get; set; }
    public string? LoanSchemeName { get; set; }

    public LoanType LoanType { get; set; }
    public decimal RequestedAmount { get; set; }
    public decimal ApprovedAmount { get; set; }
    public decimal DisbursedAmount { get; set; }
    public decimal InterestRatePerAnnum { get; set; }
    public InterestCalculationMethod CalculationMethod { get; set; }
    public RepaymentFrequency Frequency { get; set; }
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

    public LoanStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public DateTime ApplicationDate { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public DateTime? DisbursedDate { get; set; }
    public DateTime? FirstEmiDate { get; set; }
    public DateTime? MaturityDate { get; set; }
    public string PurposeOfLoan { get; set; } = string.Empty;
}

public class CreateLoanApplicationDto
{
    public int CustomerId { get; set; }
    public int BranchId { get; set; }
    public int? CenterId { get; set; }
    public int? GroupId { get; set; }
    public int LoanSchemeId { get; set; }
    public decimal RequestedAmount { get; set; }
    public int TenureInMonths { get; set; }
    public string PurposeOfLoan { get; set; } = string.Empty;
}

public class ApproveLoanDto
{
    public int LoanId { get; set; }
    public decimal ApprovedAmount { get; set; }
    public string? Remarks { get; set; }
}

public class DisburseLoanDto
{
    public int LoanId { get; set; }
    public decimal DisbursedAmount { get; set; }
    public PaymentMode DisbursementMode { get; set; } = PaymentMode.Cash;
    public DateTime DisbursementDate { get; set; } = DateTime.UtcNow;
    public DateTime FirstEmiDate { get; set; }
    public string? DisbursementReference { get; set; }
}

public class LoanEmiScheduleDto
{
    public int Id { get; set; }
    public int InstallmentNumber { get; set; }
    public DateTime DueDate { get; set; }
    public decimal PrincipalAmount { get; set; }
    public decimal InterestAmount { get; set; }
    public decimal TotalEmiAmount { get; set; }
    public decimal PaidPrincipal { get; set; }
    public decimal PaidInterest { get; set; }
    public decimal PaidPenalty { get; set; }
    public decimal TotalPaidAmount { get; set; }
    public decimal OutstandingPrincipal { get; set; }
    public decimal OutstandingInterest { get; set; }
    public decimal TotalOutstanding { get; set; }
    public decimal PenaltyAmount { get; set; }
    public EmiStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public DateTime? PaidDate { get; set; }
}

public class CreateLoanCollectionDto
{
    public int LoanApplicationId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMode PaymentMode { get; set; } = PaymentMode.Cash;
    public DateTime CollectionDate { get; set; } = DateTime.UtcNow;
    public string? ReferenceNumber { get; set; }
    public string? Remarks { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

public class CollectionSheetItemDto
{
    public int LoanId { get; set; }
    public string LoanAccountNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public int DueInstallmentNumber { get; set; }
    public decimal DueAmount { get; set; }
    public decimal OverdueAmount { get; set; }
    public decimal TotalReceivable => DueAmount + OverdueAmount;
    public decimal CollectedAmount { get; set; }
    public decimal SavingsDeposit { get; set; }
    public PaymentMode PaymentMode { get; set; } = PaymentMode.Cash;
}

public class BatchCollectionDto
{
    public int BranchId { get; set; }
    public int CenterId { get; set; }
    public DateTime CollectionDate { get; set; } = DateTime.UtcNow;
    public List<CollectionSheetItemDto> Items { get; set; } = new List<CollectionSheetItemDto>();
}
