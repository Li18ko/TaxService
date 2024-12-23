namespace TaxService.DTOs;

public class ReportWithDocumentsDto {
    public int ReportId { get; set; }
    public string ReportType { get; set; }
    public string Status { get; set; }
    public DateTime SubmissionDate { get; set; }
    public string ErrorDescription { get; set; }
    public List<DocumentDto> Documents { get; set; }

    public class DocumentDto
    {
        public int DocumentId { get; set; }
        public string Filepath { get; set; }
    }
}