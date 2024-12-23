using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TaxService.Services;

namespace TaxService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase {
    private readonly UserService _userService;
    private readonly LogService _logService;

    public UserController(UserService userService, LogService logService) {
        _userService = userService;
        _logService = logService;   
    }
    
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken()
    {
        var token = Request.Cookies["jwt"];
        if (string.IsNullOrEmpty(token)) {
            return Unauthorized(new { Message = "No token provided." });
        }

        var claimsPrincipal = _userService.ValidateJwtToken(token);
        if (claimsPrincipal == null) {
            return Unauthorized(new { Message = "Invalid token." });
        }

        var email = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value;
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { Message = "Token does not contain required claims." });
        }

        var user = await _userService.GetUserByEmailAsync(email);
        if (user == null || user.Userid.ToString() != userId)
        {
            return Unauthorized(new { Message = "User not found." });
        }

        // Генерация нового токена
        var newToken = _userService.GenerateJwtToken(email, user.Userid, user.Fullname);
        Response.Cookies.Append("jwt", newToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });

        await _logService.LogUserAction(user.Userid, "Токен обновлен");

        return Ok(new { Message = "Token refreshed." });
    }
    
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        // Получаем токен из cookie
        var token = Request.Cookies["jwt"];
        if (string.IsNullOrEmpty(token)) {
            return Unauthorized(new { Message = "No token provided." });
        }

        // Валидация токена и получение ClaimsPrincipal
        var principal = _userService.ValidateJwtToken(token);
        if (principal == null) {
            return Unauthorized(new { Message = "Invalid token." });
        }

        // Извлекаем userId из claim
        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) {
            return Unauthorized(new { Message = "User ID not found in token." });
        }

        var userId = int.Parse(userIdClaim.Value);

        // Получаем профиль пользователя
        var userProfile = await _userService.GetUserProfileAsync(userId);
        if (userProfile == null) {
            return NotFound(new { Message = "User not found." });
        }

        return Ok(userProfile);
    }
    
    [HttpGet("companies/{userId}")]
    public async Task<IActionResult> GetUserCompanies(int userId) {
        var companies = await _userService.GetUserCompaniesAsync(userId);
        return Ok(companies);
    }
}