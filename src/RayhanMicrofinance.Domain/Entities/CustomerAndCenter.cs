using RayhanMicrofinance.Domain.Common;
using RayhanMicrofinance.Domain.Enums;

namespace RayhanMicrofinance.Domain.Entities;

public class Center : BaseEntity
{
    public string CenterCode { get; set; } = string.Empty;
    public string CenterName { get; set; } = string.Empty;

    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    public int? FieldOfficerId { get; set; }
    public Employee? FieldOfficer { get; set; }

    public DayOfWeekEnum MeetingDay { get; set; } = DayOfWeekEnum.Monday;
    public TimeSpan MeetingTime { get; set; } = new TimeSpan(9, 0, 0);
    public string MeetingPlace { get; set; } = string.Empty;
    public string VillageOrTown { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public ICollection<LoanGroup> Groups { get; set; } = new List<LoanGroup>();
    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
}

public class LoanGroup : BaseEntity
{
    public string GroupCode { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;

    public int CenterId { get; set; }
    public Center? Center { get; set; }

    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    public int? GroupLeaderId { get; set; }
    public Customer? GroupLeader { get; set; }

    public GroupStatus Status { get; set; } = GroupStatus.Active;
    public DateTime FormedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedDate { get; set; }
    public string? CloseReason { get; set; }

    public ICollection<Customer> Members { get; set; } = new List<Customer>();
}

public class Customer : BaseEntity
{
    public string CustomerCode { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    public int? CenterId { get; set; }
    public Center? Center { get; set; }

    public int? GroupId { get; set; }
    public LoanGroup? Group { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();

    public string GuardianName { get; set; } = string.Empty;
    public string RelationWithGuardian { get; set; } = "Spouse"; // Husband/Father/Spouse
    public Gender Gender { get; set; } = Gender.Female;
    public DateTime DateOfBirth { get; set; }
    public MaritalStatus MaritalStatus { get; set; } = MaritalStatus.Married;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }

    // Address Details
    public string Address { get; set; } = string.Empty;
    public string Landmark { get; set; } = string.Empty;
    public string VillageOrTown { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Pincode { get; set; } = string.Empty;

    // Occupation & Income
    public string Occupation { get; set; } = string.Empty;
    public decimal MonthlyIncome { get; set; }
    public int FamilyMemberCount { get; set; } = 4;
    public decimal TotalFamilyIncome { get; set; }

    // KYC
    public KycDocumentType PrimaryKycType { get; set; } = KycDocumentType.Aadhaar;
    public string AadhaarNumber { get; set; } = string.Empty;
    public string? PanNumber { get; set; }
    public string? VoterIdNumber { get; set; }
    public string? PassportNumber { get; set; }
    public bool IsKycVerified { get; set; } = false;

    // Media & Biometrics / Proofs (File paths or URLs)
    public string? PhotoUrl { get; set; }
    public string? SignatureUrl { get; set; }
    public string? KycDocumentFrontUrl { get; set; }
    public string? KycDocumentBackUrl { get; set; }

    // GPS Location
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    // Nominee Information
    public string NomineeName { get; set; } = string.Empty;
    public string NomineeRelation { get; set; } = string.Empty;
    public string? NomineePhone { get; set; }
    public string? NomineeAadhaar { get; set; }
    public DateTime? NomineeDateOfBirth { get; set; }

    // Guarantor Information
    public string? GuarantorName { get; set; }
    public string? GuarantorRelation { get; set; }
    public string? GuarantorPhone { get; set; }
    public string? GuarantorAddress { get; set; }
    public string? GuarantorAadhaar { get; set; }

    // Bank Account Information
    public string? BankAccountNumber { get; set; }
    public string? BankName { get; set; }
    public string? IfscCode { get; set; }

    // Status
    public bool IsBlacklisted { get; set; } = false;
    public string? BlacklistReason { get; set; }

    public ICollection<LoanApplication> LoanApplications { get; set; } = new List<LoanApplication>();
    public ICollection<SavingsAccount> SavingsAccounts { get; set; } = new List<SavingsAccount>();
}
