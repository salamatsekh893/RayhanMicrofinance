using RayhanMicrofinance.Domain.Entities;
using RayhanMicrofinance.Domain.Enums;

namespace RayhanMicrofinance.Application.Interfaces;

public class EmiScheduleItem
{
    public int InstallmentNumber { get; set; }
    public DateTime DueDate { get; set; }
    public decimal PrincipalAmount { get; set; }
    public decimal InterestAmount { get; set; }
    public decimal TotalEmiAmount => PrincipalAmount + InterestAmount;
    public decimal RemainingPrincipal { get; set; }
}

public interface ILoanCalculationService
{
    List<EmiScheduleItem> GenerateSchedule(
        decimal principalAmount,
        decimal annualInterestRate,
        int tenureInMonths,
        RepaymentFrequency frequency,
        InterestCalculationMethod calculationMethod,
        DateTime firstDisbursementDate);
}

public interface IAccountingService
{
    Task<Voucher?> PostDisbursementVoucherAsync(LoanApplication loan, CancellationToken ct = default);
    Task<Voucher?> PostCollectionVoucherAsync(LoanCollection collection, CancellationToken ct = default);
    Task<Voucher?> PostSavingsTransactionVoucherAsync(SavingsTransaction transaction, CancellationToken ct = default);
    Task<Voucher?> PostExpenseVoucherAsync(ExpenseEntry expense, CancellationToken ct = default);
    Task<Voucher?> PostIncomeVoucherAsync(IncomeEntry income, CancellationToken ct = default);
}
