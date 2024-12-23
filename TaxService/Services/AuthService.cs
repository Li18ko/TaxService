using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TaxService.Data;
using TaxService.Models;

namespace TaxService.Services;

public class AuthService {
    private readonly ApplicationDbContext _context;
    private readonly string _jwtKey;
    private readonly int _jwtExpireDays = 7;

    public AuthService(string jwtKey, ApplicationDbContext context) {
        _jwtKey = jwtKey;
        _context = context;
    }
    
    public async Task<User?> AuthenticateAsync(string email, string password) {
        var passwordHash = GetSha256Hex(password);
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == passwordHash);

        return user;
    }
    

    public string GenerateJwtToken(string email, int userId, string fullName) {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtKey);

        var claims = new[]
        {
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, fullName)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
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
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка валидации токена: {ex.Message}");
            return null;
        }
    }
    
    
    public async Task<User?> GetUserByEmailAsync(string email) {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }
    
    private static string GetSha256Hex(string password) {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            
            StringBuilder hexString = new StringBuilder(hashBytes.Length * 2);
            foreach (byte b in hashBytes)
            {
                hexString.AppendFormat("{0:x2}", b); 
            }

            return hexString.ToString();
        }
    }
}