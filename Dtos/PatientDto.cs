namespace PJATK_APBD_Cw5_s32103.Dtos;

public record PatientDto(
    string Pesel,
    string FirstName,
    string LastName,
    int Age,
    string Sex,
    List<AdmissionDto> Admissions,
    List<BedAssignmentDto> BedAssignments);
