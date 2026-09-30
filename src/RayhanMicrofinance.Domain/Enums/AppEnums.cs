namespace RayhanMicrofinance.Domain.Enums;

public enum UserRole
{
    SuperAdmin = 1,
    Admin = 2,
    BranchManager = 3,
    AreaManager = 4,
    FieldOfficer = 5,
    Cashier = 6,
    Accountant = 7,
    RecoveryOfficer = 8,
    Auditor = 9,
    ReadOnlyUser = 10
}

public enum Gender
{
    Male = 1,
    Female = 2,
    Other = 3
}

public enum MaritalStatus
{
    Single = 1,
    Married = 2,
    Divorced = 3,
    Widowed = 4
}

public enum KycDocumentType
{
    Aadhaar = 1,
    PAN = 2,
    VoterID = 3,
    Passport = 4,
    DrivingLicense = 5,
    Other = 6
}

public enum DayOfWeekEnum
{
    Sunday = 0,
    Monday = 1,
    Tuesday = 2,
    Wednesday = 3,
    Thursday = 4,
    Friday = 5,
    Saturday = 6
}

public enum GroupStatus
{
    Active = 1,
    Inactive = 2,
    Suspended = 3,
    Closed = 4
}

public enum LoanType
{
    GroupLoan = 1,
    IndividualLoan = 2,
    ProductLoan = 3,
    GoldLoan = 4,
    BusinessLoan = 5,
    AgricultureLoan = 6,
    EmergencyLoan = 7
}

public enum InterestCalculationMethod
{
    Flat = 1,
    ReducingBalance = 2
}

public enum RepaymentFrequency
{
    Daily = 1,
    Weekly = 2,
    BiWeekly = 3,
    Monthly = 4
}

public enum LoanStatus
{
    Draft = 1,
    Applied = 2,
    UnderVerification = 3,
    Approved = 4,
    Rejected = 5,
    Disbursed = 6,
    Active = 7,
    Closed = 8,
    Rescheduled = 9,
    WrittenOff = 10
}

public enum EmiStatus
{
    Pending = 1,
    Paid = 2,
    PartiallyPaid = 3,
    Overdue = 4,
    Waived = 5
}

public enum SavingsType
{
    DailySavings = 1,
    WeeklySavings = 2,
    MonthlySavings = 3,
    FixedDeposit = 4,
    RecurringDeposit = 5
}

public enum SavingsAccountStatus
{
    Active = 1,
    Dormant = 2,
    Closed = 3
}

public enum PaymentMode
{
    Cash = 1,
    BankTransfer = 2,
    Cheque = 3,
    UPI = 4,
    QRPayment = 5
}

public enum VoucherType
{
    Journal = 1,
    Payment = 2,
    Receipt = 3,
    Contra = 4
}

public enum AccountClassification
{
    Asset = 1,
    Liability = 2,
    Equity = 3,
    Revenue = 4,
    Expense = 5
}

public enum AttendanceStatus
{
    Present = 1,
    Absent = 2,
    HalfDay = 3,
    Leave = 4,
    Holiday = 5
}

public enum NotificationChannel
{
    InApp = 1,
    SMS = 2,
    Email = 3,
    WhatsApp = 4
}
