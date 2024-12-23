using Microsoft.EntityFrameworkCore;
using TaxService.Data;
using TaxService.DTOs;
using TaxService.Models;

namespace TaxService.Services;

public class ReportService {
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ReportService(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
    }

    public async Task<bool> SubmitReportAsync(int userId, ReportSubmissionDto reportSubmission)
    {
        try {
            var report = new Report
            {
                Taxpayerid = reportSubmission.TaxPayerID,
                Reporttype = reportSubmission.ReportType,
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            var folderPath = Path.Combine("Reports", userId.ToString());
            Directory.CreateDirectory(folderPath);

            foreach (var file in reportSubmission.ReportFiles) {
                var filePath = Path.Combine(folderPath, $"{Guid.NewGuid()}_{file.FileName}");

                await using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

                var document = new Document
                {
                    Reportid = report.Reportid,
                    Filepath = filePath
                };

                _context.Documents.Add(document);
            }

            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при отправке отчета: {ex.Message}");
            return false;
        }
    }

}