using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TaxService.DTOs;
using TaxService.Services;

namespace TaxService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReportsController: Controller {
    private readonly ReportService _reportService;

    public ReportsController(ReportService reportService)
    {
        _reportService = reportService;
    }
    
    
    [HttpPost("submit")]
    public async Task<IActionResult> SubmitReport([FromForm] ReportSubmissionDto reportSubmission)
    {
        try {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                Console.WriteLine("Claim 'NameIdentifier' отсутствует или пуст.");
                return Unauthorized("Не удалось идентифицировать пользователя.");
            }

            if (!int.TryParse(userIdClaim, out var userId))
            {
                Console.WriteLine($"Не удалось преобразовать UserId '{userIdClaim}' в число.");
                return BadRequest("Некорректный идентификатор пользователя.");
            }


            var result = await _reportService.SubmitReportAsync(userId, reportSubmission);
            if (result) {
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

}