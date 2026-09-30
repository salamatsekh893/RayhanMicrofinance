using RayhanMicrofinance.Application.Interfaces;
using RayhanMicrofinance.Domain.Enums;

namespace RayhanMicrofinance.Application.Services;

public class LoanCalculationService : ILoanCalculationService
{
    public List<EmiScheduleItem> GenerateSchedule(
        decimal principalAmount,
        decimal annualInterestRate,
        int tenureInMonths,
        RepaymentFrequency frequency,
        InterestCalculationMethod calculationMethod,
        DateTime firstDisbursementDate)
    {
        var schedule = new List<EmiScheduleItem>();
        if (principalAmount <= 0 || tenureInMonths <= 0) return schedule;

        int totalInstallments;
        int periodsPerYear;
        Func<DateTime, int, DateTime> dateIncrementer;

        switch (frequency)
        {
            case RepaymentFrequency.Daily:
                totalInstallments = tenureInMonths * 26; // approx working days per month (excluding Sundays)
                periodsPerYear = 312;
                dateIncrementer = (date, i) => AddWorkingDays(date, i);
                break;
            case RepaymentFrequency.Weekly:
                totalInstallments = (int)Math.Round((tenureInMonths * 52.0) / 12.0);
                periodsPerYear = 52;
                dateIncrementer = (date, i) => date.AddDays(i * 7);
                break;
            case RepaymentFrequency.BiWeekly:
                totalInstallments = (int)Math.Round((tenureInMonths * 26.0) / 12.0);
                periodsPerYear = 26;
                dateIncrementer = (date, i) => date.AddDays(i * 14);
                break;
            case RepaymentFrequency.Monthly:
            default:
                totalInstallments = tenureInMonths;
                periodsPerYear = 12;
                dateIncrementer = (date, i) => date.AddMonths(i);
                break;
        }

        if (totalInstallments <= 0) totalInstallments = 1;

        if (calculationMethod == InterestCalculationMethod.Flat)
        {
            // Flat Method: Total Interest = Principal * (Rate/100) * (TenureMonths/12)
            decimal totalInterest = Math.Round(principalAmount * (annualInterestRate / 100m) * (tenureInMonths / 12.0m), 2);
            decimal perInstallmentPrincipal = Math.Round(principalAmount / totalInstallments, 2);
            decimal perInstallmentInterest = Math.Round(totalInterest / totalInstallments, 2);

            decimal runningPrincipal = principalAmount;
            decimal principalSum = 0;
            decimal interestSum = 0;

            for (int i = 1; i <= totalInstallments; i++)
            {
                var dueDate = dateIncrementer(firstDisbursementDate, i);
                decimal p = perInstallmentPrincipal;
                decimal intr = perInstallmentInterest;

                // Adjust rounding in the last installment
                if (i == totalInstallments)
                {
                    p = principalAmount - principalSum;
                    intr = totalInterest - interestSum;
                }
                else
                {
                    principalSum += p;
                    interestSum += intr;
                }

                runningPrincipal -= p;
                schedule.Add(new EmiScheduleItem
                {
                    InstallmentNumber = i,
                    DueDate = dueDate,
                    PrincipalAmount = p,
                    InterestAmount = intr,
                    RemainingPrincipal = Math.Max(0, runningPrincipal)
                });
            }
        }
        else
        {
            // Reducing Balance Method (Amortization)
            double ratePerPeriod = (double)(annualInterestRate / 100m) / periodsPerYear;
            double pDouble = (double)principalAmount;
            double emiDouble;

            if (ratePerPeriod > 0)
            {
                emiDouble = pDouble * ratePerPeriod * Math.Pow(1 + ratePerPeriod, totalInstallments) /
                            (Math.Pow(1 + ratePerPeriod, totalInstallments) - 1);
            }
            else
            {
                emiDouble = pDouble / totalInstallments;
            }

            decimal emi = Math.Round((decimal)emiDouble, 2);
            decimal remainingBalance = principalAmount;

            for (int i = 1; i <= totalInstallments; i++)
            {
                var dueDate = dateIncrementer(firstDisbursementDate, i);
                decimal interest = Math.Round(remainingBalance * (annualInterestRate / 100m) / periodsPerYear, 2);
                decimal principal = emi - interest;

                if (i == totalInstallments || principal > remainingBalance)
                {
                    principal = remainingBalance;
                    emi = principal + interest;
                }

                remainingBalance -= principal;

                schedule.Add(new EmiScheduleItem
                {
                    InstallmentNumber = i,
                    DueDate = dueDate,
                    PrincipalAmount = Math.Max(0, principal),
                    InterestAmount = Math.Max(0, interest),
                    RemainingPrincipal = Math.Max(0, remainingBalance)
                });

                if (remainingBalance <= 0) break;
            }
        }

        return schedule;
    }

    private static DateTime AddWorkingDays(DateTime startDate, int days)
    {
        DateTime current = startDate;
        int added = 0;
        while (added < days)
        {
            current = current.AddDays(1);
            if (current.DayOfWeek != DayOfWeek.Sunday)
            {
                added++;
            }
        }
        return current;
    }
}
