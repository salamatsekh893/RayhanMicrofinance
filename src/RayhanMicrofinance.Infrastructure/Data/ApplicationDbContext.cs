using Microsoft.EntityFrameworkCore;
using RayhanMicrofinance.Application.Interfaces;
using RayhanMicrofinance.Domain.Common;
using RayhanMicrofinance.Domain.Entities;

namespace RayhanMicrofinance.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentUserService? _currentUserService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService? currentUserService = null) : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<FinancialYear> FinancialYears => Set<FinancialYear>();
    public DbSet<Holiday> Holidays => Set<Holiday>();

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Designation> Designations => Set<Designation>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<SalarySlip> SalarySlips => Set<SalarySlip>();
    public DbSet<EmployeeTransfer> EmployeeTransfers => Set<EmployeeTransfer>();
    public DbSet<EmployeeIncentive> EmployeeIncentives => Set<EmployeeIncentive>();

    public DbSet<Center> Centers => Set<Center>();
    public DbSet<LoanGroup> LoanGroups => Set<LoanGroup>();
    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<LoanScheme> LoanSchemes => Set<LoanScheme>();
    public DbSet<LoanApplication> LoanApplications => Set<LoanApplication>();
    public DbSet<LoanEmiSchedule> LoanEmiSchedules => Set<LoanEmiSchedule>();
    public DbSet<LoanCollection> LoanCollections => Set<LoanCollection>();
    public DbSet<LoanRescheduleHistory> LoanRescheduleHistories => Set<LoanRescheduleHistory>();
    public DbSet<LoanWriteOffHistory> LoanWriteOffHistories => Set<LoanWriteOffHistory>();

    public DbSet<SavingsScheme> SavingsSchemes => Set<SavingsScheme>();
    public DbSet<SavingsAccount> SavingsAccounts => Set<SavingsAccount>();
    public DbSet<SavingsTransaction> SavingsTransactions => Set<SavingsTransaction>();

    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductPurchase> ProductPurchases => Set<ProductPurchase>();
    public DbSet<ProductStockTransfer> ProductStockTransfers => Set<ProductStockTransfer>();

    public DbSet<ChartOfAccount> ChartOfAccounts => Set<ChartOfAccount>();
    public DbSet<Voucher> Vouchers => Set<Voucher>();
    public DbSet<VoucherDetail> VoucherDetails => Set<VoucherDetail>();
    public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();
    public DbSet<ExpenseEntry> ExpenseEntries => Set<ExpenseEntry>();
    public DbSet<IncomeCategory> IncomeCategories => Set<IncomeCategory>();
    public DbSet<IncomeEntry> IncomeEntries => Set<IncomeEntry>();

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<LoginHistory> LoginHistories => Set<LoginHistory>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Global Decimal Precision (18, 2)
        foreach (var property in modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetColumnType("decimal(18,2)");
        }

        // Configure Relationships to avoid multiple cascade paths in SQL Server
        modelBuilder.Entity<Customer>()
            .HasOne(c => c.Branch)
            .WithMany(b => b.Customers)
            .HasForeignKey(c => c.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Customer>()
            .HasOne(c => c.Center)
            .WithMany(ctr => ctr.Customers)
            .HasForeignKey(c => c.CenterId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Customer>()
            .HasOne(c => c.Group)
            .WithMany(g => g.Members)
            .HasForeignKey(c => c.GroupId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LoanGroup>()
            .HasOne(g => g.GroupLeader)
            .WithMany()
            .HasForeignKey(g => g.GroupLeaderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LoanGroup>()
            .HasOne(g => g.Branch)
            .WithMany()
            .HasForeignKey(g => g.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LoanApplication>()
            .HasOne(l => l.Customer)
            .WithMany(c => c.LoanApplications)
            .HasForeignKey(l => l.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LoanApplication>()
            .HasOne(l => l.Branch)
            .WithMany()
            .HasForeignKey(l => l.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LoanCollection>()
            .HasOne(c => c.LoanApplication)
            .WithMany(l => l.Collections)
            .HasForeignKey(c => c.LoanApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LoanCollection>()
            .HasOne(c => c.Customer)
            .WithMany()
            .HasForeignKey(c => c.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SavingsAccount>()
            .HasOne(s => s.Customer)
            .WithMany(c => c.SavingsAccounts)
            .HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SavingsTransaction>()
            .HasOne(t => t.Customer)
            .WithMany()
            .HasForeignKey(t => t.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EmployeeTransfer>()
            .HasOne(t => t.FromBranch)
            .WithMany()
            .HasForeignKey(t => t.FromBranchId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EmployeeTransfer>()
            .HasOne(t => t.ToBranch)
            .WithMany()
            .HasForeignKey(t => t.ToBranchId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var currentUsername = _currentUserService?.Username ?? "System";

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.CreatedBy = currentUsername;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedBy = currentUsername;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
