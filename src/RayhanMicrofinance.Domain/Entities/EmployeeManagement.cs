using RayhanMicrofinance.Domain.Common;
using RayhanMicrofinance.Domain.Enums;

namespace RayhanMicrofinance.Domain.Entities;

public class Department : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ICollection<Designation> Designations { get; set; } = new List<Designation>();
}

public class Designation : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public Department? Department { get; set; }
    public int HierarchyLevel { get; set; } = 1;
}

public class Employee : BaseEntity
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public Gender Gender { get; set; } = Gender.Male;
    public DateTime DateOfBirth { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }

    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    public int DepartmentId { get; set; }
    public Department? Department { get; set; }

    public int DesignationId { get; set; }
    public Designation? Designation { get; set; }

    public UserRole Role { get; set; }
    public DateTime JoiningDate { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal HraAllowance { get; set; }
    public decimal MedicalAllowance { get; set; }
    public decimal SpecialAllowance { get; set; }

    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankName { get; set; }
    public string? IfscCode { get; set; }
    public string? PanNumber { get; set; }
    public string? AadhaarNumber { get; set; }

    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    public ICollection<SalarySlip> SalarySlips { get; set; } = new List<SalarySlip>();
}

public class Attendance : BaseEntity
{
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan? InTime { get; set; }
    public TimeSpan? OutTime { get; set; }
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
    public string? Remarks { get; set; }
}

public class SalarySlip : BaseEntity
{
    public string SlipNumber { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal Allowances { get; set; }
    public decimal Incentives { get; set; }
    public decimal Deductions { get; set; }
    public decimal NetSalary { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? PaymentDate { get; set; }
    public PaymentMode PaymentMode { get; set; } = PaymentMode.BankTransfer;
    public string? PaymentReference { get; set; }
}

public class EmployeeTransfer : BaseEntity
{
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public int FromBranchId { get; set; }
    public Branch? FromBranch { get; set; }
    public int ToBranchId { get; set; }
    public Branch? ToBranch { get; set; }
    public DateTime TransferDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? ApprovedBy { get; set; }
}

public class EmployeeIncentive : BaseEntity
{
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime IncentiveDate { get; set; }
    public bool IsPaid { get; set; }
}
