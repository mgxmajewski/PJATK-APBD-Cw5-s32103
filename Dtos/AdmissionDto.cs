namespace PJATK_APBD_Cw5_s32103.Dtos;

public record AdmissionDto(int Id, DateTime AdmissionDate, DateTime? DischargeDate, WardDto Ward);
