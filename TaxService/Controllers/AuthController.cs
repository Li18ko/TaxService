using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TaxService.DTOs;
using TaxService.Services;

namespace TaxService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase {
    private readonly AuthService _authService;
    private readonly LogService _logService;

    public AuthController(AuthService authService, LogService logService) {
        _authService = authService;
        _logService = logService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] RegisterDto loginDto) {
        var user = await _authService.AuthenticateAsync(loginDto.Email, loginDto.Password);

        if (user == null) {
            return Unauthorized();
        }

        var token = _authService.GenerateJwtToken(user.Email, user.Userid, user.Fullname);
        
        Response.Cookies.Append("jwt", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true, 
            SameSite = SameSiteMode.Strict, 
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });
        
        await _logService.LogUserAction(user.Userid, "Успешная авторизация");
        return Ok(new { Message = "Успешная авторизация" });
    }

    
    [HttpPost("logout")]
    public async Task<IActionResult> Logout() {
        var token = Request.Cookies["jwt"];
        if (string.IsNullOrEmpty(token)) {
            return Unauthorized(new { Message = "No active session found." });
        }
        
        var claimsPrincipal = _authService.ValidateJwtToken(token);
        if (claimsPrincipal == null) {
            return Unauthorized(new { Message = "Invalid token." });
        }
        var email = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value;
        
        if (email == null) {
            return Unauthorized(new { Message = "Invalid token." });
        }

        var user = await _authService.GetUserByEmailAsync(email.ToString());

        Response.Cookies.Delete("jwt");

        await _logService.LogUserAction(user.Userid, "Пользователь вышел из аккаунта");
        return Ok(new { Message = "You have successfully logged out." });
    }
}