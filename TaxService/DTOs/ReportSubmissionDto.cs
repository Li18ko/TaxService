namespace TaxService.DTOs;

public class ReportSubmissionDto {
    public string ReportType { get; set; }
    public int TaxPayerID { get; set; }
    public List<IFormFile> ReportFiles { get; set; } = new();
}
