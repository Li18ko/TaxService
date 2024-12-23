using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxService.DTOs;
using TaxService.Services;

namespace TaxService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReportsController: Controller {
    private readonly ReportService _reportService;
    private readonly LogService _logService;

    public ReportsController(ReportService reportService, LogService logService) {
        _reportService = reportService;
        _logService = logService;
    }
    
    [HttpPost("submit")]
    public async Task<IActionResult> SubmitReport([FromForm] ReportSubmissionDto reportSubmission) {
        try {
            var userId = GetUserIdFromToken();
            
            Console.WriteLine($"ReportType: {reportSubmission.ReportType}");
            Console.WriteLine($"Количество файлов: {reportSubmission.ReportFiles.Count}");
            
            var reportId = await _reportService.SubmitReportAsync(userId.Value, reportSubmission);
            if (reportId > 0)
            {
                // Возвращаем ID только что созданного отчета
                return Ok(new { reportId });
            }

            return BadRequest("Ошибка при отправке отчета.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка в контроллере: {ex.Message}");
            return StatusCode(500, "Произошла ошибка на сервере.");
        }
    }
    
    [HttpGet("{taxPayerID}/documents")]
    public async Task<IActionResult> GetDocumentsByTaxPayerID(int taxPayerID) {
        try {
            var reports = await _reportService.GetReportsWithDocumentsAsync(taxPayerID);
            if (reports == null || !reports.Any()) {
                return NotFound(new { Message = "Отчеты для этой компании не найдены." });
            }

            return Ok(reports);
        }
        catch (Exception ex) {
            Console.WriteLine($"Ошибка при получении отчетов: {ex.Message}");
            return StatusCode(500, new { Message = "Произошла ошибка на сервере." });
        }
    }
    
    [HttpPost("{reportId}/updateStatus")]
    public async Task<IActionResult> UpdateReportStatus(int reportId, [FromBody] StatusUpdateDto statusUpdate) {
        try {
            var userId = GetUserIdFromToken();
            if (userId == null) {
                return Unauthorized(new { Message = "Invalid token." });
            }

            var updateSuccess = await _reportService.UpdateReportStatusAsync(reportId, statusUpdate.Status);
            if (!updateSuccess) {
                return NotFound(new { Message = "Report not found." });
            }

            return Ok(new { Message = "Status updated successfully." });
        }
        catch (Exception ex) {
            return StatusCode(500, $"Произошла ошибка: {ex.Message}");
        }
    }
    
    private int? GetUserIdFromToken() {
        var token = Request.Cookies["jwt"];
        var principal = _reportService.ValidateJwtToken(token);
        if (principal == null) return null;

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null ? int.Parse(userIdClaim.Value) : (int?)null;
    }



}