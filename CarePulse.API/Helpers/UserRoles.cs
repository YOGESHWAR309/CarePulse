namespace CarePulse.API.Helpers;

public static class UserRoles
{
    public const string Admin = "Admin";
    public const string Patient = "Patient";
    public const string Doctor = "Doctor";
    public const string Receptionist = "Receptionist";
    public const string Nutritionist = "Nutritionist";
    public const string Dietician = "Dietician";
    public const string Canteen = "Canteen";
    public const string Pharmacist = "Pharmacist";
    public const string LabTechnician = "LabTechnician";

    public const string RoleValidationPattern = "^(Admin|Patient|Doctor|Receptionist|Nutritionist|Dietician|Canteen|Pharmacist|LabTechnician)$";
    public const string StaffRoleValidationPattern = "^(Doctor|Receptionist|Nutritionist|Dietician|Canteen|Pharmacist|LabTechnician)$";
}
