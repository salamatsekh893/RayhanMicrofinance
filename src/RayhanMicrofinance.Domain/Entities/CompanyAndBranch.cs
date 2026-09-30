using RayhanMicrofinance.Domain.Common;

namespace RayhanMicrofinance.Domain.Entities;

public class Company : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = "RPS-MF";
    public string RegistrationNumber { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Pincode { get; set; } = string.Empty;
    public string Country { get; set; } = "India";
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string CurrencySymbol { get; set; } = "₹";
    public string CurrencyCode { get; set; } = "INR";
    public string? LogoUrl { get; set; }
    public string Timezone { get; set; } = "India Standard Time";

    public ICollection<Branch> Branches { get; set; } = new List<Branch>();
}

public class Branch : BaseEntity
{
    public int CompanyId { get; set; }
    public Company? Company { get; set; }

    public string BranchCode { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string AreaCode { get; set; } = string.Empty;
    public string AreaName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Pincode { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? ManagerName { get; set; }
    public bool IsHeadOffice { get; set; } = false;
    public decimal CurrentCashBalance { get; set; } = 0;
    public decimal CurrentBankBalance { get; set; } = 0;

    public ICollection<Center> Centers { get; set; } = new List<Center>();
    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}

public class FinancialYear : BaseEntity
{
    public string Title { get; set; } = string.Empty; // e.g. "2026-2027"
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsCurrent { get; set; }
    public bool IsClosed { get; set; }
}

public class Holiday : BaseEntity
{
    public int? BranchId { get; set; }
    public Branch? Branch { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime HolidayDate { get; set; }
    public string? Description { get; set; }
}
