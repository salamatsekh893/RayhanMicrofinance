# RPS Rayhan Tech - Enterprise Microfinance ERP System

An enterprise-grade, multi-branch, multi-user Microfinance ERP built with **ASP.NET Core (.NET 9 / .NET 10)**, **C#**, **Clean Architecture**, **Entity Framework Core**, **SQL Server / SQLite**, and a responsive **Tailwind CSS** modern dashboard.

---

## 🏛️ System Architecture (Clean Architecture)

```
D:\RAYHAN_NET.MICROFINNCE\
├── src/
│   ├── RayhanMicrofinance.Domain/         # Core Entities, Enums, Value Objects
│   ├── RayhanMicrofinance.Application/    # DTOs, Business Interfaces, EMI Calculators, Models
│   ├── RayhanMicrofinance.Infrastructure/ # EF Core DbContext, Auto-Accounting Engine, JWT, Seeder
│   └── RayhanMicrofinance.Web/            # ASP.NET Core REST API & Tailwind CSS Single-Page ERP UI
├── RayhanMicrofinance.slnx
└── README.md
```

---

## 🚀 Key Modules Implemented (Per PRD)

1. **Dashboard & Analytics:**
   - Real-time KPIs: Today's Collection, Today's Disbursement, Active Portfolio, Due Collection, Overdue Loans & Risk, Vault Cash Balance, Bank Balance, Month Net Profit.
   - Interactive 6-Month Disbursement vs Collection Chart (Chart.js).
   - Multi-Branch performance ranking & recovery percentages.

2. **Company & Multi-Branch Management:**
   - Single Company enterprise profile.
   - Multi-Branch support (Head Office + Area/Branch offices).
   - Real-time branch selector with consolidated or per-branch views.

3. **Customer & KYC Management:**
   - Member registration with Guardian, Phone, Family details, Occupation, Annual Income.
   - KYC Verification (Aadhaar, PAN, Voter ID, Passport).
   - Nominee & Guarantor details.
   - GPS Location capture & Biometrics/Photo references.

4. **Centers & JLG Group Management:**
   - Center creation with meeting schedules (Day of week, Meeting time, Place, Field Officer).
   - Joint Liability Groups (JLG 5-member model), group leaders, and member assignments.

5. **Loan Management Lifecycle:**
   - Loan Schemes (Group JLG, Individual Micro-business, Agriculture, Emergency).
   - Calculation engines: **Flat Rate** and **Reducing Balance (Amortization)**.
   - Repayment frequencies: Daily, Weekly, Bi-Weekly, Monthly.
   - Complete workflow: **Application ➔ Verification ➔ Approval ➔ Disbursement ➔ Collection ➔ Passbook**.
   - Automatic EMI schedule generation on disbursement.

6. **Daily & Field Collection Management:**
   - Center-wise **Field Collection Sheet Generator** for field officers.
   - Instant 1-click **Batch Collection Submission**.
   - Waterfall payment allocation: Penalty ➔ Interest ➔ Principal ➔ Advance.
   - Thermal & A4 **Printable Collection Receipts**.

7. **Savings & Deposits:**
   - Daily Savings, Recurring Deposit (RD), and Fixed Deposit (FD) accounts.
   - Deposit and Withdrawal processing with instant passbook balance update.

8. **Automated Double-Entry Accounting:**
   - Chart of Accounts (Assets, Liabilities, Equity, Revenues, Expenses).
   - Automated journal vouchers generated on loan disbursement, collection, savings, expenses.
   - Live **General Ledger Trial Balance** (100% Debit-Credit balanced).
   - Live **Profit & Loss Statement** (Operating revenues vs expenses).
   - Live **Balance Sheet** (Assets = Liabilities + Equity).
   - Cash Book and Daily Cashier Day Book.

9. **Security & Role-Based Access Control (RBAC):**
   - JWT Bearer Authentication with HMAC-SHA256.
   - Roles: Super Admin, Branch Manager, Field Officer, Accountant, Cashier, Auditor.
   - BCrypt cryptographic password hashing.

---

## 🔑 Default Login Credentials

| Role | Username | Password |
| :--- | :--- | :--- |
| **Super Admin** | `admin` | `AdminPassword123!` |
| **Branch Manager** | `manager` | `Manager123!` |
| **Field Officer** | `fieldofficer` | `Field123!` |

*(The web login screen also features 1-Click Quick Login buttons for convenience.)*

---

## 💻 How to Run Locally

1. Open PowerShell or Command Prompt in `D:\RAYHAN_NET.MICROFINNCE`.
2. Run the application:
   ```powershell
   dotnet run --project src/RayhanMicrofinance.Web/RayhanMicrofinance.Web.csproj --launch-profile http
   ```
3. Open your browser and navigate to:
   👉 **`http://localhost:5126`**

---

## 🗄️ Database Switching: SQLite (Dev) vs SQL Server (Production)

In `src/RayhanMicrofinance.Web/appsettings.json`:

* **For SQLite (Local fast development):**
  ```json
  "DatabaseProvider": "Sqlite"
  ```
* **For Microsoft SQL Server (Production):**
  ```json
  "DatabaseProvider": "SqlServer",
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=RayhanMicrofinanceDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;"
  }
  ```

---

## 🖥️ How to Deploy to Windows Server (IIS)

1. **Publish the Project:**
   ```powershell
   dotnet publish src/RayhanMicrofinance.Web/RayhanMicrofinance.Web.csproj -c Release -o C:\publish\microfinance
   ```
2. **On your Windows Server:**
   - Install **.NET 9 / .NET 10 Hosting Bundle**.
   - Install **SQL Server Express** and create `RayhanMicrofinanceDb`.
   - Open **IIS (Internet Information Services)**.
   - Add a new Website pointing physical path to `C:\publish\microfinance`.
   - Set Application Pool to **No Managed Code**.
   - Browse to your domain or server IP address!
