using RayhanMicrofinance.Domain.Enums;

namespace RayhanMicrofinance.Application.DTOs;

public class CustomerDto
{
    public int Id { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string? BranchName { get; set; }
    public int? CenterId { get; set; }
    public string? CenterName { get; set; }
    public int? GroupId { get; set; }
    public string? GroupName { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();

    public string GuardianName { get; set; } = string.Empty;
    public string RelationWithGuardian { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public DateTime DateOfBirth { get; set; }
    public MaritalStatus MaritalStatus { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }

    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Pincode { get; set; } = string.Empty;

    public string Occupation { get; set; } = string.Empty;
    public decimal MonthlyIncome { get; set; }
    public int FamilyMemberCount { get; set; }

    public KycDocumentType PrimaryKycType { get; set; }
    public string AadhaarNumber { get; set; } = string.Empty;
    public string? PanNumber { get; set; }
    public string? VoterIdNumber { get; set; }
    public bool IsKycVerified { get; set; }

    public string? PhotoUrl { get; set; }
    public string? SignatureUrl { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public string NomineeName { get; set; } = string.Empty;
    public string NomineeRelation { get; set; } = string.Empty;
    public string? NomineePhone { get; set; }

    public string? GuarantorName { get; set; }
    public string? GuarantorPhone { get; set; }

    public bool IsBlacklisted { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateCustomerDto
{
    public int BranchId { get; set; }
    public int? CenterId { get; set; }
    public int? GroupId { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string GuardianName { get; set; } = string.Empty;
    public string RelationWithGuardian { get; set; } = "Spouse";
    public Gender Gender { get; set; } = Gender.Female;
    public DateTime DateOfBirth { get; set; }
    public MaritalStatus MaritalStatus { get; set; } = MaritalStatus.Married;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }

    public string Address { get; set; } = string.Empty;
    public string Landmark { get; set; } = string.Empty;
    public string VillageOrTown { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Pincode { get; set; } = string.Empty;

    public string Occupation { get; set; } = string.Empty;
    public decimal MonthlyIncome { get; set; }
    public int FamilyMemberCount { get; set; } = 4;
    public decimal TotalFamilyIncome { get; set; }

    public KycDocumentType PrimaryKycType { get; set; } = KycDocumentType.Aadhaar;
    public string AadhaarNumber { get; set; } = string.Empty;
    public string? PanNumber { get; set; }
    public string? VoterIdNumber { get; set; }
    public string? PassportNumber { get; set; }

    public string? PhotoUrl { get; set; }
    public string? SignatureUrl { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public string NomineeName { get; set; } = string.Empty;
    public string NomineeRelation { get; set; } = string.Empty;
    public string? NomineePhone { get; set; }
    public string? NomineeAadhaar { get; set; }

    public string? GuarantorName { get; set; }
    public string? GuarantorRelation { get; set; }
    public string? GuarantorPhone { get; set; }
    public string? GuarantorAddress { get; set; }
    public string? GuarantorAadhaar { get; set; }

    public string? BankAccountNumber { get; set; }
    public string? BankName { get; set; }
    public string? IfscCode { get; set; }
}

public class CenterDto
{
    public int Id { get; set; }
    public string CenterCode { get; set; } = string.Empty;
    public string CenterName { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string? BranchName { get; set; }
    public int? FieldOfficerId { get; set; }
    public string? FieldOfficerName { get; set; }
    public DayOfWeekEnum MeetingDay { get; set; }
    public string MeetingDayName => MeetingDay.ToString();
    public string MeetingTime { get; set; } = string.Empty;
    public string MeetingPlace { get; set; } = string.Empty;
    public string VillageOrTown { get; set; } = string.Empty;
    public int TotalGroups { get; set; }
    public int TotalMembers { get; set; }
}

public class CreateCenterDto
{
    public string CenterName { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public int? FieldOfficerId { get; set; }
    public DayOfWeekEnum MeetingDay { get; set; } = DayOfWeekEnum.Monday;
    public string MeetingTime { get; set; } = "10:00";
    public string MeetingPlace { get; set; } = string.Empty;
    public string VillageOrTown { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

public class GroupDto
{
    public int Id { get; set; }
    public string GroupCode { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public int CenterId { get; set; }
    public string? CenterName { get; set; }
    public int BranchId { get; set; }
    public string? BranchName { get; set; }
    public int? GroupLeaderId { get; set; }
    public string? GroupLeaderName { get; set; }
    public GroupStatus Status { get; set; }
    public int MemberCount { get; set; }
}

public class CreateGroupDto
{
    public string GroupName { get; set; } = string.Empty;
    public int CenterId { get; set; }
    public int BranchId { get; set; }
    public int? GroupLeaderId { get; set; }
}
