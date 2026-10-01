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

            // 6.5 Staff / Employees
            var empManager1 = new Employee
            {
                EmployeeCode = "EMP-01-001",
                FirstName = "Md.",
                LastName = "Rayhan",
                BranchId = hoBranch.Id,
                DepartmentId = opDept.Id,
                DesignationId = desigBm.Id,
                Role = UserRole.BranchManager,
                JoiningDate = DateTime.UtcNow.AddYears(-2),
                BasicSalary = 45000,
                Email = "manager@rayhantech.com",
                Phone = "+91 98765 00002"
            };

            var empFo1 = new Employee
            {
                EmployeeCode = "EMP-01-002",
                FirstName = "Rahul",
                LastName = "Sen",
                BranchId = hoBranch.Id,
                DepartmentId = opDept.Id,
                DesignationId = desigFo.Id,
                Role = UserRole.FieldOfficer,
                JoiningDate = DateTime.UtcNow.AddYears(-1),
                BasicSalary = 22000,
                Email = "rahul@rayhantech.com",
                Phone = "+91 98765 00003"
            };

            var empManager2 = new Employee
            {
                EmployeeCode = "EMP-02-001",
                FirstName = "Sourav",
                LastName = "Das",
                BranchId = barasatBranch.Id,
                DepartmentId = opDept.Id,
                DesignationId = desigBm.Id,
                Role = UserRole.BranchManager,
                JoiningDate = DateTime.UtcNow.AddYears(-2),
                BasicSalary = 40000,
                Email = "sourav@rayhantech.com",
                Phone = "+91 98765 00004"
            };

            var empFo2 = new Employee
            {
                EmployeeCode = "EMP-02-002",
                FirstName = "Bikash",
                LastName = "Ghosh",
                BranchId = barasatBranch.Id,
                DepartmentId = opDept.Id,
                DesignationId = desigFo.Id,
                Role = UserRole.FieldOfficer,
                JoiningDate = DateTime.UtcNow.AddMonths(-8),
                BasicSalary = 20000,
                Email = "bikash@rayhantech.com",
                Phone = "+91 98765 00005"
            };

            context.Employees.AddRange(empManager1, empFo1, empManager2, empFo2);
            await context.SaveChangesAsync();

            // 7. Users with RBAC and Employee Linkages
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
                    EmployeeId = empManager1.Id,
                    IsActive = true
                },
                new()
                {
                    Username = "manager_barasat",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Manager123!"),
                    FullName = "Sourav Das (Barasat Manager)",
                    Email = "sourav@rayhantech.com",
                    Phone = "+91 98765 00004",
                    Role = UserRole.BranchManager,
                    BranchId = barasatBranch.Id,
                    EmployeeId = empManager2.Id,
                    IsActive = true
                },
                new()
                {
                    Username = "fieldofficer",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Field123!"),
                    FullName = "Rahul Sen (Field Officer - HO)",
                    Email = "rahul@rayhantech.com",
                    Phone = "+91 98765 00003",
                    Role = UserRole.FieldOfficer,
                    BranchId = hoBranch.Id,
                    EmployeeId = empFo1.Id,
                    IsActive = true
                },
                new()
                {
                    Username = "fieldofficer2",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Field123!"),
                    FullName = "Bikash Ghosh (Field Officer - Barasat)",
                    Email = "bikash@rayhantech.com",
                    Phone = "+91 98765 00005",
                    Role = UserRole.FieldOfficer,
                    BranchId = barasatBranch.Id,
                    EmployeeId = empFo2.Id,
                    IsActive = true
                }
            };
            context.Users.AddRange(users);
            await context.SaveChangesAsync();

            // 8. Centers & Groups
            var center1 = new Center
            {
                BranchId = hoBranch.Id,
                FieldOfficerId = empFo1.Id,
                CenterCode = "CTR-101",
                CenterName = "Bidhannagar Mahila Center",
                MeetingDay = DayOfWeekEnum.Monday,
                MeetingTime = new TimeSpan(10, 0, 0),
                MeetingPlace = "Community Hall, Ward 2",
                VillageOrTown = "Salt Lake",
                Latitude = 22.5800,
                Longitude = 88.4200
            };

            var center2 = new Center
            {
                BranchId = barasatBranch.Id,
                FieldOfficerId = empFo2.Id,
                CenterCode = "CTR-201",
                CenterName = "Barasat Nabapally Center",
                MeetingDay = DayOfWeekEnum.Wednesday,
                MeetingTime = new TimeSpan(11, 0, 0),
                MeetingPlace = "Panchayat Office Hall",
                VillageOrTown = "Barasat",
                Latitude = 22.7200,
                Longitude = 88.4800
            };
            context.Centers.AddRange(center1, center2);
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

            var group2 = new LoanGroup
            {
                BranchId = barasatBranch.Id,
                CenterId = center2.Id,
                GroupCode = "GRP-201-A",
                GroupName = "Saraswati Mahila Group",
                Status = GroupStatus.Active,
                FormedDate = DateTime.UtcNow.AddMonths(-2)
            };
            context.LoanGroups.AddRange(group1, group2);
            await context.SaveChangesAsync();

            // 9. Sample Customers (HO & Barasat)
            var cust1 = new Customer
            {
                CustomerCode = "CUST-HO-260001",
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
                CustomerCode = "CUST-HO-260002",
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
                IsKycVerified = false,
                NomineeName = "Mustafa Mondal",
                NomineeRelation = "Spouse",
                NomineePhone = "9831122337",
                Latitude = 22.5810,
                Longitude = 88.4215
            };

            var cust3 = new Customer
            {
                CustomerCode = "CUST-BR-260001",
                BranchId = barasatBranch.Id,
                CenterId = center2.Id,
                GroupId = group2.Id,
                FirstName = "Anjali",
                LastName = "Mondal",
                GuardianName = "Tapas Mondal",
                RelationWithGuardian = "Spouse",
                Gender = Gender.Female,
                DateOfBirth = new DateTime(1994, 3, 10),
                MaritalStatus = MaritalStatus.Married,
                Phone = "9831122338",
                Address = "Vill-Nabapally, PO-Barasat",
                City = "Barasat",
                State = "West Bengal",
                Pincode = "700124",
                Occupation = "Handicrafts & Jute Bags",
                MonthlyIncome = 17000,
                AadhaarNumber = "9988 7766 5544",
                VoterIdNumber = "WB/02/456/789012",
                IsKycVerified = true,
                NomineeName = "Tapas Mondal",
                NomineeRelation = "Spouse",
                NomineePhone = "9831122339",
                Latitude = 22.7210,
                Longitude = 88.4815
            };

            context.Customers.AddRange(cust1, cust2, cust3);
            await context.SaveChangesAsync();

            // Set group leaders
            group1.GroupLeaderId = cust1.Id;
            group2.GroupLeaderId = cust3.Id;
            await context.SaveChangesAsync();

            // 10. Sample Loan Applications with EMI Schedules
            var loanScheme = schemes.First();

            // Loan 1 (Kolkata HO)
            var loan1 = new LoanApplication
            {
                LoanAccountNumber = "LN-2026-00001",
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

            // Loan 2 (Barasat Branch)
            var loan2 = new LoanApplication
            {
                LoanAccountNumber = "LN-2026-00002",
                CustomerId = cust3.Id,
                BranchId = barasatBranch.Id,
                CenterId = center2.Id,
                GroupId = group2.Id,
                LoanSchemeId = loanScheme.Id,
                LoanType = LoanType.GroupLoan,
                RequestedAmount = 25000,
                ApprovedAmount = 25000,
                DisbursedAmount = 25000,
                InterestRatePerAnnum = 18.0m,
                CalculationMethod = InterestCalculationMethod.Flat,
                Frequency = RepaymentFrequency.Weekly,
                TenureInMonths = 12,
                TotalInstallments = 52,
                TotalInterest = 4500,
                TotalPayable = 29500,
                TotalPaid = 2835, // 5 installments paid
                OutstandingPrincipal = 22596,
                OutstandingInterest = 4069,
                ProcessingFee = 250,
                InsuranceFee = 250,
                Status = LoanStatus.Active,
                ApplicationDate = DateTime.UtcNow.AddMonths(-1),
                ApprovedDate = DateTime.UtcNow.AddMonths(-1),
                ApprovedBy = "manager_barasat",
                DisbursedDate = DateTime.UtcNow.AddMonths(-1),
                DisbursedBy = "manager_barasat",
                DisbursementMode = PaymentMode.Cash,
                FirstEmiDate = DateTime.UtcNow.AddMonths(-1).AddDays(7),
                MaturityDate = DateTime.UtcNow.AddMonths(11),
                PurposeOfLoan = "Jute craft machinery and raw materials"
            };

            context.LoanApplications.AddRange(loan1, loan2);
            await context.SaveChangesAsync();

            // Create EMI Schedule for Loan 1
            decimal weeklyPrincipal1 = Math.Round(30000m / 52m, 2);
            decimal weeklyInterest1 = Math.Round(5400m / 52m, 2);
            var startDate1 = loan1.DisbursedDate!.Value;

            for (int i = 1; i <= 52; i++)
            {
                var dueDate = startDate1.AddDays(i * 7);
                var isPaid = i <= 8;
                var emi = new LoanEmiSchedule
                {
                    LoanApplicationId = loan1.Id,
                    InstallmentNumber = i,
                    DueDate = dueDate,
                    PrincipalAmount = weeklyPrincipal1,
                    InterestAmount = weeklyInterest1,
                    PaidPrincipal = isPaid ? weeklyPrincipal1 : 0,
                    PaidInterest = isPaid ? weeklyInterest1 : 0,
                    PaidPenalty = 0,
                    Status = isPaid ? EmiStatus.Paid : (dueDate < DateTime.UtcNow ? EmiStatus.Overdue : EmiStatus.Pending),
                    PaidDate = isPaid ? dueDate : null,
                    PaymentReference = isPaid ? $"REC-PAY-{i:D4}" : null
                };
                context.LoanEmiSchedules.Add(emi);
            }

            // Create EMI Schedule for Loan 2
            decimal weeklyPrincipal2 = Math.Round(25000m / 52m, 2);
            decimal weeklyInterest2 = Math.Round(4500m / 52m, 2);
            var startDate2 = loan2.DisbursedDate!.Value;

            for (int i = 1; i <= 52; i++)
            {
                var dueDate = startDate2.AddDays(i * 7);
                var isPaid = i <= 5;
                var emi = new LoanEmiSchedule
                {
                    LoanApplicationId = loan2.Id,
                    InstallmentNumber = i,
                    DueDate = dueDate,
                    PrincipalAmount = weeklyPrincipal2,
                    InterestAmount = weeklyInterest2,
                    PaidPrincipal = isPaid ? weeklyPrincipal2 : 0,
                    PaidInterest = isPaid ? weeklyInterest2 : 0,
                    PaidPenalty = 0,
                    Status = isPaid ? EmiStatus.Paid : (dueDate < DateTime.UtcNow ? EmiStatus.Overdue : EmiStatus.Pending),
                    PaidDate = isPaid ? dueDate : null,
                    PaymentReference = isPaid ? $"REC-BR-PAY-{i:D4}" : null
                };
                context.LoanEmiSchedules.Add(emi);
            }
            await context.SaveChangesAsync();

            // 11. Sample Savings Accounts
            var savingsAcc1 = new SavingsAccount
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

            var savingsAcc2 = new SavingsAccount
            {
                AccountNumber = "SB-2026-0002",
                CustomerId = cust3.Id,
                BranchId = barasatBranch.Id,
                SavingsSchemeId = savingsSchemes.First().Id,
                CurrentBalance = 1500,
                TotalDeposited = 1500,
                TotalWithdrawn = 0,
                Status = SavingsAccountStatus.Active,
                OpenedDate = DateTime.UtcNow.AddMonths(-1)
            };

            context.SavingsAccounts.AddRange(savingsAcc1, savingsAcc2);
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
