using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using RayhanMicrofinance.Domain.Entities;
using RayhanMicrofinance.Domain.Enums;

namespace RayhanMicrofinance.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        // 1. Company
        if (!await context.Companies.AnyAsync())
        {
            var company = new Company
            {
                Name = "RPS Rayhan Tech Microfinance",
                Code = "RPS-MF",
                RegistrationNumber = "U65999WB2026PTC123456",
                TaxNumber = "19AABCR1234F1Z5",
                Address = "Tech Park, Sector V, Salt Lake",
                City = "Kolkata",
                State = "West Bengal",
                Pincode = "700091",
                Country = "India",
                Phone = "+91 98765 43210",
                Email = "info@rayhantech.com",
                Website = "https://rayhantech.com",
                CurrencySymbol = "₹",
                CurrencyCode = "INR",
                Timezone = "India Standard Time"
            };
            context.Companies.Add(company);
            await context.SaveChangesAsync();

            // 2. Branches
            var hoBranch = new Branch
            {
                CompanyId = company.Id,
                BranchCode = "HO-001",
                BranchName = "Kolkata Head Office",
                AreaCode = "AREA-01",
                AreaName = "Greater Kolkata",
                Address = "Salt Lake Sector V",
                City = "Kolkata",
                State = "West Bengal",
                Pincode = "700091",
                Phone = "+91 98765 43211",
                Email = "ho@rayhantech.com",
                ManagerName = "Md. Rayhan",
                IsHeadOffice = true,
                CurrentCashBalance = 500000m,
                CurrentBankBalance = 2500000m
            };

            var barasatBranch = new Branch
            {
                CompanyId = company.Id,
                BranchCode = "BR-002",
                BranchName = "Barasat Branch",
                AreaCode = "AREA-01",
                AreaName = "North 24 Parganas",
                Address = "Champadali More",
                City = "Barasat",
                State = "West Bengal",
                Pincode = "700124",
                Phone = "+91 98765 43212",
                Email = "barasat@rayhantech.com",
                ManagerName = "Sourav Das",
                IsHeadOffice = false,
                CurrentCashBalance = 200000m,
                CurrentBankBalance = 1000000m
            };

            context.Branches.AddRange(hoBranch, barasatBranch);
            await context.SaveChangesAsync();

            // 3. Chart of Accounts
            var coa = new List<ChartOfAccount>
            {
                new() { AccountCode = "1010", AccountName = "Cash in Hand", Classification = AccountClassification.Asset, IsSystemAccount = true, CurrentBalance = 700000m },
                new() { AccountCode = "1020", AccountName = "Bank Current Account", Classification = AccountClassification.Asset, IsSystemAccount = true, CurrentBalance = 3500000m },
                new() { AccountCode = "1030", AccountName = "Loan Portfolio Principal (Asset)", Classification = AccountClassification.Asset, IsSystemAccount = true, CurrentBalance = 0 },
                new() { AccountCode = "2010", AccountName = "Member Savings Liability", Classification = AccountClassification.Liability, IsSystemAccount = true, CurrentBalance = 0 },
                new() { AccountCode = "3010", AccountName = "Equity & Retained Capital", Classification = AccountClassification.Equity, IsSystemAccount = true, CurrentBalance = 4200000m },
                new() { AccountCode = "4010", AccountName = "Interest Income on Loans", Classification = AccountClassification.Revenue, IsSystemAccount = true, CurrentBalance = 0 },
                new() { AccountCode = "4020", AccountName = "Loan Processing Fee Income", Classification = AccountClassification.Revenue, IsSystemAccount = true, CurrentBalance = 0 },
                new() { AccountCode = "4030", AccountName = "Late Fine & Penalty Income", Classification = AccountClassification.Revenue, IsSystemAccount = true, CurrentBalance = 0 },
                new() { AccountCode = "4040", AccountName = "Other Operating Income", Classification = AccountClassification.Revenue, IsSystemAccount = true, CurrentBalance = 0 },
                new() { AccountCode = "5010", AccountName = "Staff Salary & Allowances", Classification = AccountClassification.Expense, IsSystemAccount = true, CurrentBalance = 0 },
                new() { AccountCode = "5020", AccountName = "Office Rent & Utilities", Classification = AccountClassification.Expense, IsSystemAccount = true, CurrentBalance = 0 },
                new() { AccountCode = "5030", AccountName = "Field Conveyance & Travel", Classification = AccountClassification.Expense, IsSystemAccount = true, CurrentBalance = 0 },
                new() { AccountCode = "5040", AccountName = "Stationery & IT Expenses", Classification = AccountClassification.Expense, IsSystemAccount = true, CurrentBalance = 0 }
            };
            context.ChartOfAccounts.AddRange(coa);
            await context.SaveChangesAsync();

            // 4. Loan Schemes
            var schemes = new List<LoanScheme>
            {
                new()
                {
                    SchemeCode = "SCH-JLG-01",
                    SchemeName = "Mahila Samriddhi Group Loan (JLG)",
                    LoanType = LoanType.GroupLoan,
                    MinAmount = 10000,
                    MaxAmount = 60000,
                    DefaultAmount = 30000,
                    MinTenureMonths = 6,
                    MaxTenureMonths = 24,
                    DefaultTenureMonths = 12,
                    InterestRatePerAnnum = 18.0m,
                    InterestCalculationMethod = InterestCalculationMethod.Flat,
                    RepaymentFrequency = RepaymentFrequency.Weekly,
                    ProcessingFeePercentage = 1.0m,
                    InsuranceFeePercentage = 1.0m,
                    LateFinePercentagePerDay = 0.05m,
                    GracePeriodDays = 3,
                    Description = "Joint Liability Group loan for rural & semi-urban women entrepreneurs"
                },
                new()
                {
                    SchemeCode = "SCH-IND-02",
                    SchemeName = "Unnati Business Micro Loan (Individual)",
                    LoanType = LoanType.IndividualLoan,
                    MinAmount = 25000,
                    MaxAmount = 150000,
                    DefaultAmount = 50000,
                    MinTenureMonths = 6,
                    MaxTenureMonths = 36,
                    DefaultTenureMonths = 18,
                    InterestRatePerAnnum = 20.0m,
                    InterestCalculationMethod = InterestCalculationMethod.ReducingBalance,
                    RepaymentFrequency = RepaymentFrequency.Monthly,
                    ProcessingFeePercentage = 1.5m,
                    InsuranceFeePercentage = 1.0m,
                    LateFinePercentagePerDay = 0.1m,
                    GracePeriodDays = 5,
                    Description = "Individual business expansion loan for micro-enterprises"
                },
                new()
                {
                    SchemeCode = "SCH-AGR-03",
                    SchemeName = "Krishi Vikas Agriculture Loan",
                    LoanType = LoanType.AgricultureLoan,
                    MinAmount = 15000,
                    MaxAmount = 80000,
                    DefaultAmount = 35000,
                    MinTenureMonths = 6,
                    MaxTenureMonths = 12,
                    DefaultTenureMonths = 12,
                    InterestRatePerAnnum = 15.0m,
                    InterestCalculationMethod = InterestCalculationMethod.Flat,
                    RepaymentFrequency = RepaymentFrequency.Monthly,
                    ProcessingFeePercentage = 1.0m,
                    InsuranceFeePercentage = 0.5m,
                    LateFinePercentagePerDay = 0.05m,
                    GracePeriodDays = 7,
                    Description = "Seasonal agricultural production and equipment support"
                },
                new()
                {
                    SchemeCode = "SCH-EMG-04",
                    SchemeName = "Apatkalin Emergency Loan",
                    LoanType = LoanType.EmergencyLoan,
                    MinAmount = 5000,
                    MaxAmount = 20000,
                    DefaultAmount = 10000,
                    MinTenureMonths = 3,
                    MaxTenureMonths = 6,
                    DefaultTenureMonths = 6,
                    InterestRatePerAnnum = 12.0m,
                    InterestCalculationMethod = InterestCalculationMethod.Flat,
                    RepaymentFrequency = RepaymentFrequency.Weekly,
                    ProcessingFeePercentage = 0.5m,
                    InsuranceFeePercentage = 0m,
                    LateFinePercentagePerDay = 0.05m,
                    GracePeriodDays = 2,
                    Description = "Immediate medical or family emergency assistance loan"
                }
            };
            context.LoanSchemes.AddRange(schemes);
            await context.SaveChangesAsync();

            // 5. Savings Schemes
            var savingsSchemes = new List<SavingsScheme>
            {
                new() { SchemeCode = "SAV-REG-01", SchemeName = "Dainik Sanchay (Daily Savings)", SavingsType = SavingsType.DailySavings, InterestRatePerAnnum = 4.5m, MinDepositAmount = 50, MaturityInMonths = 12 },
                new() { SchemeCode = "SAV-RD-02", SchemeName = "Masik Recurring Deposit (RD)", SavingsType = SavingsType.RecurringDeposit, InterestRatePerAnnum = 7.5m, MinDepositAmount = 500, MaturityInMonths = 24 },
                new() { SchemeCode = "SAV-FD-03", SchemeName = "Suraksha Fixed Deposit (FD)", SavingsType = SavingsType.FixedDeposit, InterestRatePerAnnum = 8.5m, MinDepositAmount = 5000, MaturityInMonths = 36 }
            };
            context.SavingsSchemes.AddRange(savingsSchemes);
            await context.SaveChangesAsync();

            // 6. Departments & Designations
            var opDept = new Department { Name = "Operations", Description = "Field & Branch Microfinance Operations" };
            var accDept = new Department { Name = "Accounts & Finance", Description = "Accounting, Treasury & Audit" };
            context.Departments.AddRange(opDept, accDept);
            await context.SaveChangesAsync();

            var desigBm = new Designation { Title = "Branch Manager", DepartmentId = opDept.Id, HierarchyLevel = 2 };
            var desigFo = new Designation { Title = "Field Officer", DepartmentId = opDept.Id, HierarchyLevel = 4 };
            var desigAcc = new Designation { Title = "Accountant", DepartmentId = accDept.Id, HierarchyLevel = 3 };
            context.Designations.AddRange(desigBm, desigFo, desigAcc);
            await context.SaveChangesAsync();

            // 7. Users
            var users = new List<User>
            {
                new()
                {
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("AdminPassword123!"),
                    FullName = "System Super Administrator",
                    Email = "admin@rayhantech.com",
                    Phone = "+91 98765 00001",
                    Role = UserRole.SuperAdmin,
                    BranchId = hoBranch.Id,
                    IsActive = true
                },
                new()
                {
                    Username = "manager",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Manager123!"),
                    FullName = "Md. Rayhan (HO Manager)",
                    Email = "manager@rayhantech.com",
                    Phone = "+91 98765 00002",
                    Role = UserRole.BranchManager,
                    BranchId = hoBranch.Id,
                    IsActive = true
                },
                new()
                {
                    Username = "fieldofficer",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Field123!"),
                    FullName = "Rahul Sen (Field Officer)",
                    Email = "rahul@rayhantech.com",
                    Phone = "+91 98765 00003",
                    Role = UserRole.FieldOfficer,
                    BranchId = hoBranch.Id,
                    IsActive = true
                }
            };
            context.Users.AddRange(users);
            await context.SaveChangesAsync();

            // 8. Centers & Groups
            var center1 = new Center
            {
                BranchId = hoBranch.Id,
                CenterCode = "CTR-101",
                CenterName = "Bidhannagar Mahila Center",
                MeetingDay = DayOfWeekEnum.Monday,
                MeetingTime = new TimeSpan(10, 0, 0),
                MeetingPlace = "Community Hall, Ward 2",
                VillageOrTown = "Salt Lake",
                Latitude = 22.5800,
                Longitude = 88.4200
            };
            context.Centers.Add(center1);
            await context.SaveChangesAsync();

            var group1 = new LoanGroup
            {
                BranchId = hoBranch.Id,
                CenterId = center1.Id,
                GroupCode = "GRP-101-A",
                GroupName = "Maa Tara JLG Group",
                Status = GroupStatus.Active,
                FormedDate = DateTime.UtcNow.AddMonths(-3)
            };
            context.LoanGroups.Add(group1);
            await context.SaveChangesAsync();

            // 9. Sample Customers
            var cust1 = new Customer
            {
                CustomerCode = "CUST-2026-0001",
                BranchId = hoBranch.Id,
                CenterId = center1.Id,
                GroupId = group1.Id,
                FirstName = "Ananya",
                LastName = "Khatun",
                GuardianName = "Rahim Ali",
                RelationWithGuardian = "Spouse",
                Gender = Gender.Female,
                DateOfBirth = new DateTime(1992, 5, 14),
                MaritalStatus = MaritalStatus.Married,
                Phone = "9831122334",
                Address = "House 12, Ward 2, Bidhannagar",
                City = "Kolkata",
                State = "West Bengal",
                Pincode = "700091",
                Occupation = "Dairy & Poultry Farming",
                MonthlyIncome = 18000,
                AadhaarNumber = "4512 8890 1234",
                PanNumber = "ABCDE1234F",
                VoterIdNumber = "WB/01/123/456789",
                IsKycVerified = true,
                NomineeName = "Rahim Ali",
                NomineeRelation = "Spouse",
                NomineePhone = "9831122335",
                Latitude = 22.5805,
                Longitude = 88.4210
            };

            var cust2 = new Customer
            {
                CustomerCode = "CUST-2026-0002",
                BranchId = hoBranch.Id,
                CenterId = center1.Id,
                GroupId = group1.Id,
                FirstName = "Fatema",
                LastName = "Bibi",
                GuardianName = "Mustafa Mondal",
                RelationWithGuardian = "Spouse",
                Gender = Gender.Female,
                DateOfBirth = new DateTime(1989, 8, 22),
                MaritalStatus = MaritalStatus.Married,
                Phone = "9831122336",
                Address = "House 15, Ward 2, Bidhannagar",
                City = "Kolkata",
                State = "West Bengal",
                Pincode = "700091",
                Occupation = "Tailoring & Garments",
                MonthlyIncome = 16000,
                AadhaarNumber = "7823 4455 9012",
                VoterIdNumber = "WB/01/123/456790",
                IsKycVerified = true,
                NomineeName = "Mustafa Mondal",
                NomineeRelation = "Spouse",
                NomineePhone = "9831122337",
                Latitude = 22.5810,
                Longitude = 88.4215
            };

            context.Customers.AddRange(cust1, cust2);
            await context.SaveChangesAsync();

            // Set group leader
            group1.GroupLeaderId = cust1.Id;
            await context.SaveChangesAsync();

            // 10. Sample Loan Application with EMI Schedule
            var loanScheme = schemes.First();
            var loan = new LoanApplication
            {
                LoanAccountNumber = "LN-2026-0001",
                CustomerId = cust1.Id,
                BranchId = hoBranch.Id,
                CenterId = center1.Id,
                GroupId = group1.Id,
                LoanSchemeId = loanScheme.Id,
                LoanType = LoanType.GroupLoan,
                RequestedAmount = 30000,
                ApprovedAmount = 30000,
                DisbursedAmount = 30000,
                InterestRatePerAnnum = 18.0m,
                CalculationMethod = InterestCalculationMethod.Flat,
                Frequency = RepaymentFrequency.Weekly,
                TenureInMonths = 12,
                TotalInstallments = 52,
                TotalInterest = 5400,
                TotalPayable = 35400,
                TotalPaid = 5448, // 8 installments paid
                OutstandingPrincipal = 25384,
                OutstandingInterest = 4568,
                ProcessingFee = 300,
                InsuranceFee = 300,
                Status = LoanStatus.Active,
                ApplicationDate = DateTime.UtcNow.AddMonths(-2),
                ApprovedDate = DateTime.UtcNow.AddMonths(-2),
                ApprovedBy = "admin",
                DisbursedDate = DateTime.UtcNow.AddMonths(-2),
                DisbursedBy = "admin",
                DisbursementMode = PaymentMode.Cash,
                FirstEmiDate = DateTime.UtcNow.AddMonths(-2).AddDays(7),
                MaturityDate = DateTime.UtcNow.AddMonths(10),
                PurposeOfLoan = "Purchase of sewing machines and textile fabrics"
            };
            context.LoanApplications.Add(loan);
            await context.SaveChangesAsync();

            // Create EMI Schedule
            decimal weeklyPrincipal = Math.Round(30000m / 52m, 2);
            decimal weeklyInterest = Math.Round(5400m / 52m, 2);
            var startDate = loan.DisbursedDate!.Value;

            for (int i = 1; i <= 52; i++)
            {
                var dueDate = startDate.AddDays(i * 7);
                var isPaid = i <= 8;
                var emi = new LoanEmiSchedule
                {
                    LoanApplicationId = loan.Id,
                    InstallmentNumber = i,
                    DueDate = dueDate,
                    PrincipalAmount = weeklyPrincipal,
                    InterestAmount = weeklyInterest,
                    PaidPrincipal = isPaid ? weeklyPrincipal : 0,
                    PaidInterest = isPaid ? weeklyInterest : 0,
                    PaidPenalty = 0,
                    Status = isPaid ? EmiStatus.Paid : (dueDate < DateTime.UtcNow ? EmiStatus.Overdue : EmiStatus.Pending),
                    PaidDate = isPaid ? dueDate : null,
                    PaymentReference = isPaid ? $"REC-PAY-{i:D4}" : null
                };
                context.LoanEmiSchedules.Add(emi);
            }
            await context.SaveChangesAsync();

            // 11. Sample Savings Account
            var savingsAcc = new SavingsAccount
            {
                AccountNumber = "SB-2026-0001",
                CustomerId = cust1.Id,
                BranchId = hoBranch.Id,
                SavingsSchemeId = savingsSchemes.First().Id,
                CurrentBalance = 2400,
                TotalDeposited = 2400,
                TotalWithdrawn = 0,
                Status = SavingsAccountStatus.Active,
                OpenedDate = DateTime.UtcNow.AddMonths(-2)
            };
            context.SavingsAccounts.Add(savingsAcc);
            await context.SaveChangesAsync();

            // 12. System Settings
            var settings = new List<SystemSetting>
            {
                new() { SettingKey = "CompanyName", SettingValue = "RPS Rayhan Tech Microfinance", SettingGroup = "Company" },
                new() { SettingKey = "AutoGenerateEmiOnDisbursement", SettingValue = "true", SettingGroup = "Loan" },
                new() { SettingKey = "AutoPostAccountingVouchers", SettingValue = "true", SettingGroup = "Accounting" },
                new() { SettingKey = "AllowPartialPayments", SettingValue = "true", SettingGroup = "Collection" },
                new() { SettingKey = "GracePeriodDaysDefault", SettingValue = "3", SettingGroup = "Loan" },
                new() { SettingKey = "SmsNotificationsEnabled", SettingValue = "false", SettingGroup = "Notification" },
                new() { SettingKey = "WhatsAppNotificationsEnabled", SettingValue = "false", SettingGroup = "Notification" }
            };
            context.SystemSettings.AddRange(settings);
            await context.SaveChangesAsync();
        }
    }
}
