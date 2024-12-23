using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TaxService.Data;
using TaxService.DTOs;
using TaxService.Models;

namespace TaxService.Services;

public class ReportService {
    private readonly ApplicationDbContext _context;
    private readonly string _jwtKey;

    public ReportService(string jwtKey, ApplicationDbContext context) {
        _context = context;
        _jwtKey = jwtKey;
    }

    public async Task<int> SubmitReportAsync(int userId, ReportSubmissionDto reportSubmission) {
        try {
            Console.WriteLine(reportSubmission.TaxPayerID);
            Console.WriteLine(reportSubmission.ReportType);
            var report = new Report {
                Taxpayerid = reportSubmission.TaxPayerID,
                Reporttype = reportSubmission.ReportType,
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            var folderPath = Path.Combine("Reports", userId.ToString());
            Directory.CreateDirectory(folderPath);

            foreach (var file in reportSubmission.ReportFiles) {
                // Формируем строку для имени файла: дата_время_оригинальное имя_уникальный GUID.
                string fileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{DateTime.Now:yyyy.MM.dd_HH.mm.ss}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

                // Генерируем полный путь для сохранения файла
                var filePath = Path.Combine(folderPath, fileName);

                // Сохраняем файл на диск
                await using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

                // Создаем запись о файле в базе данных
                var document = new Document
                {
                    Reportid = report.Reportid,
                    Filepath = filePath
                };

                // Добавляем документ в контекст
                _context.Documents.Add(document);
            }

            await _context.SaveChangesAsync();

            return report.Reportid;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при отправке отчета: {ex.Message}");
            return -1;
        }
    }
    
    public ClaimsPrincipal ValidateJwtToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtKey);

        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return principal;
        }
        catch (Exception)
        {
            return null;  // Ошибка валидации токена
        }
    }
    
    public async Task<List<ReportWithDocumentsDto>> GetReportsWithDocumentsAsync(int taxPayerID) {
        try {
            Console.WriteLine($"Получение отчетов для TaxPayerID: {taxPayerID}");

            var reports = await _context.Reports
                .Where(r => r.Taxpayerid == taxPayerID)
                .Select(r => new ReportWithDocumentsDto
                {
                    ReportId = r.Reportid,
                    ReportType = r.Reporttype ?? "Не указан",
                    Status = r.Status ?? "Не указано",
                    SubmissionDate = r.Submissiondate ?? DateTime.MinValue,
                    ErrorDescription = r.Errordescription ?? "Нет ошибок",
                    Documents = r.Documents.Select(d => new ReportWithDocumentsDto.DocumentDto
                    {
                        DocumentId = d.Documentid,
                        Filepath = d.Filepath ?? "Путь к файлу отсутствует"
                    }).ToList()
                })
                .ToListAsync();

            Console.WriteLine($"Найдено отчетов: {reports.Count}");
            foreach (var report in reports)
            {
                Console.WriteLine($"Отчет: {report.ReportType}, Документов: {report.Documents.Count}");
            }
            return reports;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при получении отчетов: {ex.Message}");
            throw new Exception("Произошла ошибка при получении отчетов");
        }
    }

    public async Task<bool> UpdateReportStatusAsync(int reportId, string status)
    {
        var report = await _context.Reports.FindAsync(reportId);
        if (report == null) return false;

        report.Status = status;
        await _context.SaveChangesAsync();
        return true;
    }


}