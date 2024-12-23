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
            var token = Request.Cookies["jwt"];
            var principal = _reportService.ValidateJwtToken(token);
            if (principal == null) {
                return Unauthorized(new { Message = "Invalid token." });
            }
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) {
                return Unauthorized(new { Message = "User ID not found in token." });
            }
            var userId = int.Parse(userIdClaim.Value);
            
            
            Console.WriteLine($"ReportType: {reportSubmission.ReportType}");
            Console.WriteLine($"Количество файлов: {reportSubmission.ReportFiles.Count}");
            
            var result = await _reportService.SubmitReportAsync(userId, reportSubmission);
            if (result) {
                await _logService.LogUserAction(userId, "Пользователь успешно отправил отчет");
                return Ok("Отчет успешно отправлен.");
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


}