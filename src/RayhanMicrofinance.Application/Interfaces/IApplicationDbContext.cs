using Microsoft.EntityFrameworkCore;
using RayhanMicrofinance.Domain.Entities;

namespace RayhanMicrofinance.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Company> Companies { get; }
    DbSet<Branch> Branches { get; }
    DbSet<FinancialYear> FinancialYears { get; }
    DbSet<Holiday> Holidays { get; }

    DbSet<Department> Departments { get; }
    DbSet<Designation> Designations { get; }
    DbSet<Employee> Employees { get; }
    DbSet<Attendance> Attendances { get; }
    DbSet<SalarySlip> SalarySlips { get; }
    DbSet<EmployeeTransfer> EmployeeTransfers { get; }
    DbSet<EmployeeIncentive> EmployeeIncentives { get; }

    DbSet<Center> Centers { get; }
    DbSet<LoanGroup> LoanGroups { get; }
    DbSet<Customer> Customers { get; }

    DbSet<LoanScheme> LoanSchemes { get; }
    DbSet<LoanApplication> LoanApplications { get; }
    DbSet<LoanEmiSchedule> LoanEmiSchedules { get; }
    DbSet<LoanCollection> LoanCollections { get; }
    DbSet<LoanRescheduleHistory> LoanRescheduleHistories { get; }
    DbSet<LoanWriteOffHistory> LoanWriteOffHistories { get; }

    DbSet<SavingsScheme> SavingsSchemes { get; }
    DbSet<SavingsAccount> SavingsAccounts { get; }
    DbSet<SavingsTransaction> SavingsTransactions { get; }

    DbSet<ProductCategory> ProductCategories { get; }
    DbSet<Supplier> Suppliers { get; }
    DbSet<Product> Products { get; }
    DbSet<ProductPurchase> ProductPurchases { get; }
    DbSet<ProductStockTransfer> ProductStockTransfers { get; }

    DbSet<ChartOfAccount> ChartOfAccounts { get; }
    DbSet<Voucher> Vouchers { get; }
    DbSet<VoucherDetail> VoucherDetails { get; }
    DbSet<ExpenseCategory> ExpenseCategories { get; }
    DbSet<ExpenseEntry> ExpenseEntries { get; }
    DbSet<IncomeCategory> IncomeCategories { get; }
    DbSet<IncomeEntry> IncomeEntries { get; }

    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<LoginHistory> LoginHistories { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<SystemSetting> SystemSettings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
